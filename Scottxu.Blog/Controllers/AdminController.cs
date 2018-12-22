using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.ViewModels.Admin;
using Scottxu.Blog.Models.ViewModels;
using Scottxu.Blog.Models.Entities;
using Scottxu.Blog.Models.Helpers;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Scottxu.Blog.Models.Exception;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models.Units;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        IHostingEnvironment HostingEnvironment { get; }

        public AdminController(BlogSystemContext context, IOptions<SiteOptions> options,
            IHostingEnvironment hostingEnvironment) :
            base(context, options) => HostingEnvironment = hostingEnvironment;

        #region 概览

        // GET: /Admin/
        public IActionResult Index()
        {
            var indexViewModel = new IndexViewModel();
            switch (OSPlatform)
            {
                case OSPlatformEnum.Linux:
                    indexViewModel.OSType = "Linux";
                    indexViewModel.OSIcon = "linux";
                    break;
                case OSPlatformEnum.Windows:
                    indexViewModel.OSType = "Windows";
                    indexViewModel.OSIcon = "windows";
                    break;
                case OSPlatformEnum.MacOS:
                    indexViewModel.OSType = "MacOS";
                    indexViewModel.OSIcon = "apple";
                    break;
                default:
                    indexViewModel.OSType = "未知";
                    indexViewModel.OSIcon = "question-circle";
                    break;
            }

            indexViewModel.Version = $"V{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";
            return View(indexViewModel);
        }

        #endregion

        #region 文章管理

        // GET: /Admin/ArticleManager
        public IActionResult ArticleManager(string searchMessage, Guid? articleTypeGuid, int page = 0)
        {
            return View(ArticleManager_LoadData(page, searchMessage, articleTypeGuid));
        }

        ArticleManagerViewModel ArticleManager_LoadData(int page, string searchMessage, Guid? articleTypeGuid)
        {
            var pageInfo = new PageInfoViewModel
            {
                PageSize = 10,
                PageIndex = page
            };
            List<ArticleType> articleTypes;

            return new ArticleManagerViewModel()
            {
                Articles = Article.GetData(DataBaseContext, pageInfo, searchMessage, articleTypeGuid),
                PageInfo = pageInfo,
                SearchMessage = searchMessage,
                SelectedArticleTypes = articleTypes = ArticleType.GetVirtualTree(DataBaseContext),
                SelectedArticleLabels = DataBaseContext.ArticleLabels.OrderBy(o => o.Name).ToList(),
                ArticleType = articleTypes.FirstOrDefault(q => q.Guid == articleTypeGuid)
            };
        }

        // POST: /Admin/ArticleManager_Delete
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void ArticleManager_Delete(Guid[] deleteGuid)
        {
            if (deleteGuid == null || !deleteGuid.Any())
                throw new MissingParametersException("缺少参数。");
            Article.Delete(DataBaseContext, HostingEnvironment, deleteGuid);
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/ArticleManager_AddItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void ArticleManager_AddItem(string name, Guid articleTypeGuid, Guid[] articleLabelGuids, string content)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(content))
                throw new MissingParametersException("缺少参数。");
            Article.AddItem(DataBaseContext, name, articleTypeGuid, articleLabelGuids, content);
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/ArticleManager_GetItem
        [ApiAction]
        [HttpPost]
        public object ArticleManager_GetItem(Guid guid)
        {
            var article = Article.GetArticle(DataBaseContext, guid);
            var articleLabel = ArticleLabel.GetAllDataByArticle(DataBaseContext, article);
            return new
            {
                article.Name,
                article.Content,
                ArticleTypeGuid = article.ArticleType.Guid,
                ArticleLabelGuids = articleLabel.Select(o => o.Guid)
            };
        }

        // POST: /Admin/ArticleManager_EditItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void ArticleManager_EditItem(Guid guid, string name, Guid articleTypeGuid, Guid[] articleLabelGuids,
            string content)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(content))
                throw new MissingParametersException("缺少参数。");
            Article.EditItem(DataBaseContext, guid, name, articleTypeGuid, articleLabelGuids, content);
            DataBaseContext.SaveChanges();
        }

        #endregion

        #region 类别管理

        // GET: /Admin/TypeManager
        public IActionResult TypeManager(string searchMessage, int page = 0)
        {
            return View(TypeManager_LoadData(page, searchMessage));
        }

        TypeManagerViewModel TypeManager_LoadData(int page, string searchMessage)
        {
            var pageInfo = new PageInfoViewModel
            {
                PageSize = 10,
                PageIndex = page
            };
            return new TypeManagerViewModel()
            {
                ArticleTypes = ArticleType.GetData(DataBaseContext, pageInfo, searchMessage, out var articleTypes),
                PageInfo = pageInfo,
                SearchMessage = searchMessage,
                SelectedArticleTypes = articleTypes
            };
        }

        // POST: /Admin/TypeManager_Delete
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TypeManager_Delete(Guid[] deleteGuid)
        {
            if (deleteGuid == null || !deleteGuid.Any())
                throw new MissingParametersException("缺少参数。");
            ArticleType.Delete(DataBaseContext, deleteGuid);
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/TypeManager_AddItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TypeManager_AddItem(string name, Guid? parentArticleTypeGuid, UInt32 sequence)
        {
            if (string.IsNullOrEmpty(name))
                throw new MissingParametersException("缺少参数。");
            ArticleType.AddItem(DataBaseContext, name, parentArticleTypeGuid, sequence);
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/TypeManager_EditItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TypeManager_EditItem(Guid guid, string name, Guid? parentArticleTypeGuid, UInt32 sequence)
        {
            if (string.IsNullOrEmpty(name))
                throw new MissingParametersException("缺少参数。");
            ArticleType.EditItem(DataBaseContext, guid, name, parentArticleTypeGuid, sequence);
            DataBaseContext.SaveChanges();
        }

        #endregion

        #region 标签管理

        // GET: /Admin/LabelManager
        public IActionResult LabelManager(string searchMessage, int page = 0)
        {
            return View(LabelManager_LoadData(page, searchMessage));
        }

        LabelManagerViewModel LabelManager_LoadData(int page, string searchMessage)
        {
            var pageInfo = new PageInfoViewModel
            {
                PageSize = 10,
                SortDirection = "ASC",
                SortField = "Name",
                PageIndex = page
            };
            return new LabelManagerViewModel()
            {
                ArticleLabels = ArticleLabel.GetData(DataBaseContext, pageInfo, searchMessage),
                PageInfo = pageInfo,
                SearchMessage = searchMessage
            };
        }

        // POST: /Admin/LabelManager_Delete
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void LabelManager_Delete(Guid[] deleteGuid)
        {
            if (deleteGuid == null || !deleteGuid.Any())
                throw new MissingParametersException("缺少参数。");
            ArticleLabel.Delete(DataBaseContext, deleteGuid);
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/LabelManager_AddItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void LabelManager_AddItem(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new MissingParametersException("缺少参数。");
            ArticleLabel.AddItem(DataBaseContext, name);
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/LabelManager_EditItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void LabelManager_EditItem(Guid guid, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new MissingParametersException("缺少参数。");
            ArticleLabel.EditItem(DataBaseContext, guid, name);
            DataBaseContext.SaveChanges();
        }

        #endregion

        #region 模板管理

        // GET: /Admin/TemplateManager
        public IActionResult TemplateManager(string searchMessage, int page = 0)
        {
            return View(TemplateManager_LoadData(page, searchMessage));
        }

        TemplateManagerViewModel TemplateManager_LoadData(int page, string searchMessage)
        {
            var pageInfo = new PageInfoViewModel
            {
                PageSize = 10,
                SortDirection = "ASC",
                SortField = "Name",
                PageIndex = page
            };
            return new TemplateManagerViewModel()
            {
                Templates = Template.GetData(DataBaseContext, pageInfo, searchMessage),
                PageInfo = pageInfo,
                SearchMessage = searchMessage
            };
        }
        
        // POST: /Admin/TemplateManager_Delete
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateManager_Delete(Guid[] deleteGuid)
        {
            if (deleteGuid == null || !deleteGuid.Any())
                throw new MissingParametersException("缺少参数。");
            Template.Delete(DataBaseContext, HostingEnvironment, deleteGuid);
            DataBaseContext.SaveChanges();
        }
        
        // POST: /Admin/TemplateManager_AddItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateManager_AddItem(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new MissingParametersException("缺少参数。");
            Template.AddItem(DataBaseContext, HostingEnvironment, name);
            DataBaseContext.SaveChanges();
        }
        
        // POST: /Admin/TemplateManager_EditItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateManager_EditItem(Guid guid, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new MissingParametersException("缺少参数。");
            Template.EditItem(DataBaseContext, guid, name);
            DataBaseContext.SaveChanges();
        }
        
        // POST: /Admin/TemplateManager_UploadZip
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateManager_UploadZip()
        {
            if (Request.Form.Files.Count() != 1)
                throw new MissingParametersException("缺少参数。");
            Template.AddZipFile(DataBaseContext, HostingEnvironment,
                (dataBaseContext, hostingEnvironment) =>
                    new UploadUnit(dataBaseContext, hostingEnvironment).SaveZipFiles(HttpContext));
            DataBaseContext.SaveChanges();
        }
        
        // GET: /Admin/TemplateFileManager
        public IActionResult TemplateFileManager(string searchMessage, Guid templateGuid, int page = 0)
        {
            return View(TemplateFileManager_LoadData(page, searchMessage, templateGuid));
        }
        
        TemplateFileManagerViewModel TemplateFileManager_LoadData(int page, string searchMessage, Guid templateGuid)
        {
            var pageInfo = new PageInfoViewModel
            {
                PageSize = 10,
                SortDirection = "ASC",
                SortField = "VirtualPath",
                PageIndex = page
            };
            return new TemplateFileManagerViewModel()
            {
                TemplateFiles = TemplateFile.GetData(DataBaseContext, pageInfo, searchMessage, templateGuid),
                Template = DataBaseContext.Templates.FirstOrDefault(q => q.Guid == templateGuid),
                PageInfo = pageInfo,
                SearchMessage = searchMessage
            };
        }

        // POST: /Admin/TemplateFileManager_Delete
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateFileManager_Delete(Guid[] deleteGuid)
        {
            if (deleteGuid == null || !deleteGuid.Any())
                throw new MissingParametersException("缺少参数。");
            TemplateFile.Delete(DataBaseContext, HostingEnvironment, deleteGuid);
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/TemplateFileManager_AddItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateFileManager_AddItem(string virtualPath, Guid templateGuid)
        {
            if (string.IsNullOrEmpty(virtualPath) || Request.Form.Files.Count() != 1)
                throw new MissingParametersException("缺少参数。");
            TemplateFile.AddItem(DataBaseContext, HostingEnvironment, virtualPath, templateGuid,
                (dataBaseContext, hostingEnvironment) =>
                    new UploadUnit(dataBaseContext, hostingEnvironment).SaveFiles(HttpContext));
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/TemplateFileManager_EditItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateFileManager_EditItem(Guid guid, string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                throw new MissingParametersException("缺少参数。");
            TemplateFile.EditItem(DataBaseContext, guid, virtualPath);
            DataBaseContext.SaveChanges();
        }


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (context.Result != null) return;
            var configUnit = new ConfigUnit(DataBaseContext);
            ViewBag.userName = configUnit.UserName;
            ViewBag.articlesCount = DataBaseContext.Articles.Count();
            ViewBag.articleTypesCount = DataBaseContext.ArticleTypes.Count();
            ViewBag.articleLabelsCount = DataBaseContext.ArticleLabels.Count();
        }

        #endregion
    }
}