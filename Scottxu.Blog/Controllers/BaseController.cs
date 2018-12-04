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
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class BaseController : Controller
    {
        internal BlogSystemContext DataBaseContext { get; }
        
        internal SiteOptions Options { get; }

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

        internal BaseController(BlogSystemContext context, IOptions<SiteOptions> options)
        {
            DataBaseContext = context;
            Options = options.Value;
        }

        class JsonResult
        {
            public bool Success { get; set; }
            public string ErrorType { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }
        }

        internal class ApiAction : Attribute { }

        internal async Task UserLoginAsync(string email)
        {
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
            if (apiActionAttributes.Any())
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
    }
}

