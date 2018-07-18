using System;
using System.Linq;
using Scottxu.Blog.Models;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Scottxu.Blog.Models.ViewModel;
using Scottxu.Blog.Models.Util;
using Scottxu.Blog.Models.Entitys;
using System.Collections.Generic;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class BaseController : Controller
    {
        internal BlogSystemContext DataBaseContext { get; }

        internal enum OSPlatformEnum 
        {
            Linux,
            Windows,
            MacOS,
            Unknowed
        }

        internal OSPlatformEnum OSPlatform 
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux)) return OSPlatformEnum.Linux;
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) return OSPlatformEnum.Windows;
                if (RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX)) return OSPlatformEnum.MacOS;
                return OSPlatformEnum.Unknowed;
            }
        }

        internal Version AssemblyVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        internal BaseController(BlogSystemContext context)
        {
            DataBaseContext = context;
        }

        internal class MissingParametersException : Exception 
        {
            public MissingParametersException(string message) : base(message) {}
        }

        internal class ParametersFormatErrorException : Exception
        {
            public ParametersFormatErrorException(string message) : base(message) { }
        }

        class JsonResult 
        {
            public bool Success { get; set; }
            public string ErrorType { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }
        }

        internal class ApiAction : Attribute {}

        internal async Task UserLoginAsync(string email){
            var identity = new ClaimsIdentity("Forms");
            identity.AddClaim(new Claim(ClaimTypes.Email, email));
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);
        }

        internal async Task UserLogoutAsync()
        {
            await HttpContext.SignOutAsync();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            if (controllerActionDescriptor.ControllerName != "Setup" && !DataBaseContext.DataBaseIsExist)
                context.Result = RedirectToAction(String.Empty, "Setup");
        }

        public override void OnActionExecuted(ActionExecutedContext context) 
        {
            var controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            var controllerActionMethodInfo = controllerActionDescriptor.MethodInfo;
            var apiActionAttributes = controllerActionMethodInfo.GetCustomAttributes(typeof(ApiAction), false);
            if (apiActionAttributes.Count() > 0)
            {
                if (context.Exception != null)
                {
                    context.Result = Json(new JsonResult()
                    {
                        Success = false,
                        ErrorType = context.Exception.GetType().FullName,
                        Message = context.Exception.Message
                    });
                    context.Exception = null;
                }
                else context.Result = Json(new JsonResult()
                {
                    Success = true,
                    Data = context.Result.GetType() == typeof(ObjectResult) ? ((ObjectResult)context.Result).Value : null
                });
            }
        }

        #region EF相关

        protected IQueryable<T> Sort<T>(IQueryable<T> q, PageInfoViewModel pageInfo) => q.SortBy(pageInfo.SortField + " " + pageInfo.SortDirection);

        // 排序
        protected IQueryable<T> Sort<T>(IQueryable<T> q, string sortField, string sortDirection) => q.SortBy(sortField + " " + sortDirection);

        protected IQueryable<T> SortAndPage<T>(IQueryable<T> q, PageInfoViewModel pageInfo) => SortAndPage(q, pageInfo.PageIndex, pageInfo.PageSize, pageInfo.RecordCount, pageInfo.SortField, pageInfo.SortDirection);

        // 排序后分页
        protected IQueryable<T> SortAndPage<T>(IQueryable<T> q, int pageIndex, int pageSize, int recordCount, string sortField, string sortDirection)
        {
            if (recordCount == 0) return q;
            //// 对传入的 pageIndex 进行有效性验证//////////////
            int pageCount = recordCount / pageSize;
            if (recordCount % pageSize != 0)
            {
                pageCount++;
            }
            if (pageIndex > pageCount - 1)
            {
                pageIndex = pageCount - 1;
            }
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }
            ///////////////////////////////////////////////

            return Sort(q, sortField, sortDirection).Skip(pageIndex * pageSize).Take(pageSize);
        }


        // 附加实体到数据库上下文中（首先在Local中查找实体是否存在，不存在才Attach，否则会报错）
        // http://patrickdesjardins.com/blog/entity-framework-4-3-an-object-with-the-same-key-already-exists-in-the-objectstatemanager
        protected T Attach<T>(Guid keyGuid) where T : class, IKeyGuid, new()
        {
            T t = DataBaseContext.Set<T>().Local.Where(x => x.Guid == keyGuid).FirstOrDefault();
            if (t == null)
            {
                t = new T { Guid = keyGuid };
                DataBaseContext.Set<T>().Attach(t);
            }
            return t;
        }

        // 向现有实体集合中添加新项
        protected void AddEntities<T>(ICollection<T> existItems, Guid[] newItemGuids) where T : class, IKeyGuid, new()
        {
            foreach (var roleGuid in newItemGuids)
            {
                T t = Attach<T>(roleGuid);
                existItems.Add(t);
            }
        }

        // 替换现有实体集合中的所有项
        // http://stackoverflow.com/questions/2789113/entity-framework-update-entity-along-with-child-entities-add-update-as-necessar
        protected void ReplaceEntities<T>(ICollection<T> existEntities, Guid[] newEntityGuids) where T : class, IKeyGuid, new()
        {
            if (newEntityGuids.Length == 0)
            {
                existEntities.Clear();
            }
            else
            {
                Guid[] tobeAdded = newEntityGuids.Except(existEntities.Select(x => x.Guid)).ToArray();
                Guid[] tobeRemoved = existEntities.Select(x => x.Guid).Except(newEntityGuids).ToArray();

                AddEntities<T>(existEntities, tobeAdded);

                existEntities.Where(x => tobeRemoved.Contains(x.Guid)).ToList().ForEach(e => existEntities.Remove(e));
            }
        }

        // http://patrickdesjardins.com/blog/validation-failed-for-one-or-more-entities-see-entityvalidationerrors-property-for-more-details-2
        // ((System.Data.Entity.Validation.DbEntityValidationException)$exception).EntityValidationErrors

        #endregion
    }
}
