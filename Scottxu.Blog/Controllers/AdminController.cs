using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.ViewModel.Admin;
using Scottxu.Blog.Models.ViewModel;
using Scottxu.Blog.Models.Entitys;
using Scottxu.Blog.Models.Helper;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    [Authorize]
    public class AdminController : BaseController
    {
        IHostingEnvironment HostingEnvironment { get; }
        public AdminController(BlogSystemContext context, IHostingEnvironment hostingEnvironment) : base(context) => HostingEnvironment = hostingEnvironment;

        #region 概览
        // GET: /Admin/
        public IActionResult Index()
        {
            var indexViewModel = new IndexViewModel();
            switch(OSPlatform) {
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
        public IActionResult ArticleManager(string searchMessage, Guid? articleTypeGuid, int page = 0){
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
            List<ArticleLabel> articleLabels;


            return new ArticleManagerViewModel()
            {
                Articles = ArticleManager_GetData(pageInfo, searchMessage, articleTypeGuid, out articleTypes, out articleLabels),
                PageInfo = pageInfo,
                SearchMessage = searchMessage,
                SelectedArticleTypes = articleTypes,
                SelectedArticleLabels = articleLabels,
                ArticleType = articleTypes.FirstOrDefault(q => q.Guid == articleTypeGuid)
            };
        }

        List<Article> ArticleManager_GetData(PageInfoViewModel pageInfo, string searchMessage, Guid? articleTypeGuid, out List<ArticleType> articleTypes, out List<ArticleLabel> articleLabels)
        {
            articleTypes = ArticleType.GetVirtualTree(DataBaseContext);

            articleLabels = DataBaseContext.ArticleLabels.OrderBy(o => o.Name).ToList();

            IQueryable<Article> q = DataBaseContext.Articles;

            IQueryable<ArticleLabelArticle> m = DataBaseContext.ArticleLabelArticles;

            m = m.Include(o => o.Article)
                .Include(o => o.ArticleLabel);
                
            if (articleTypeGuid.HasValue) {
                var selectedArticleTypeGuids = ArticleType.GetVirtualTree(DataBaseContext, articleTypeGuid.Value);
                q = q.Join(selectedArticleTypeGuids, l => l.ArticleType, r => r, (l, r) => l);
            }

            // 表单搜索
            string searchText = searchMessage?.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                q = q.Where(o => o.Name.Contains(searchText));
            }

            // 在添加条件之后，排序和分页之前获取总记录数
            pageInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage(q, pageInfo);

            var articles = q.ToList();

            articles.ForEach(o => o.ArticleLabelArticles = m.Where(p => p.Article == o).ToList());

            return articles;
        }

        // POST: /Admin/ArticleManager_Delete
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void ArticleManager_Delete(Guid[] deleteGuid) {
            if (deleteGuid == null || deleteGuid.Count() == 0) 
                throw new MissingParametersException("缺少参数。");
            using (var transaction = DataBaseContext.Database.BeginTransaction())
            {
                var uploadHelper = new UploadHelper(DataBaseContext, HostingEnvironment);
                deleteGuid.ToList().ForEach(d =>
                {
                    var article = DataBaseContext.Articles.FirstOrDefault(q => q.Guid == d);
                    var uploadedFileArticles = DataBaseContext.UploadedFileArticles.Where(q => q.Article == article).ToList();
                    uploadedFileArticles.ForEach(q => {
                        DataBaseContext.UploadedFileArticles.Remove(q);
                        DataBaseContext.SaveChanges();
                        uploadHelper.CheckAndDeleteFile(q.UploadedFileGuid);
                    });
                    DataBaseContext.Articles.Remove(article);
                });
                DataBaseContext.SaveChanges();
                transaction.Commit();
            }
        }

        // POST: /Admin/ArticleManager_AddItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void ArticleManager_AddItem(string name, Guid articleTypeGuid, Guid[] articleLabelGuids, string content)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(content))
                throw new MissingParametersException("缺少参数。");
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleName, new ParametersFormatErrorException("文章名格式错误。"));
            var article = new Article()
            {
                Name = name,
                ArticleTypeGuid = articleTypeGuid,
                Content = content
            };
            DataBaseContext.Articles.Add(article);
            DataBaseContext.ArticleLabelArticles.AddRange(articleLabelGuids.Select(o => new ArticleLabelArticle
            {
                ArticleLabelGuid = o,
                Article = article
            }));
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/ArticleManager_GetItem
        [ApiAction]
        [HttpPost]
        public object ArticleManager_GetItem(Guid guid)
        {
            var article = DataBaseContext.Articles.Include(o => o.ArticleType).FirstOrDefault(o => o.Guid == guid);
            var articleLabel = DataBaseContext.ArticleLabelArticles.Include(o => o.ArticleLabel).Where(o => o.Article == article).Select(o => o.ArticleLabel).ToList();
            article.ClickTraffic++;
            DataBaseContext.SaveChanges();
            return new {
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
        public void ArticleManager_EditItem(Guid guid, string name, Guid articleTypeGuid, Guid[] articleLabelGuids, string content)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(content))
                throw new MissingParametersException("缺少参数。");
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleName, new ParametersFormatErrorException("文章名格式错误。"));
            var article = DataBaseContext.Articles.FirstOrDefault(q => q.Guid == guid);
            article.Name = name;
            article.ArticleTypeGuid = articleTypeGuid;
            article.Content = content;
            DataBaseContext.ArticleLabelArticles.RemoveRange(DataBaseContext.ArticleLabelArticles.Where(o => o.Article == article));
            DataBaseContext.ArticleLabelArticles.AddRange(articleLabelGuids.Select(o => new ArticleLabelArticle
            {
                ArticleLabelGuid = o,
                Article = article
            }));
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
            List<ArticleType> articleTypes;
            return new TypeManagerViewModel()
            {
                ArticleTypes = TypeManager_GetData(pageInfo, searchMessage, out articleTypes),
                PageInfo = pageInfo,
                SearchMessage = searchMessage,
                SelectedArticleTypes = articleTypes
            };
        }

        List<ArticleType> TypeManager_GetData(PageInfoViewModel pageInfo, string searchMessage, out List<ArticleType> articleTypes)
        {
            articleTypes = ArticleType.GetVirtualTree(DataBaseContext);

            List<ArticleType> searchArticleTypes;
            // 表单搜索
            string searchText = searchMessage?.Trim();
            if (!string.IsNullOrEmpty(searchText))
                searchArticleTypes = articleTypes.Where(o => o.Name.Contains(searchText)).ToList();
            else searchArticleTypes = articleTypes;

            // 在添加条件之后，排序和分页之前获取总记录数
            pageInfo.RecordCount = searchArticleTypes.Count();

            // 排列和数据库分页
            return searchArticleTypes.Skip(pageInfo.PageIndex * pageInfo.PageSize).Take(pageInfo.PageSize).ToList();
        }

        // POST: /Admin/TypeManager_Delete
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TypeManager_Delete(Guid[] deleteGuid)
        {
            if (deleteGuid == null || deleteGuid.Count() == 0)
                throw new MissingParametersException("缺少参数。");
            DataBaseContext.ArticleTypes.RemoveRange(DataBaseContext.ArticleTypes.Join(deleteGuid, l => l.Guid, r => r, (l, r) => l));
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
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleTypeName, new ParametersFormatErrorException("类别格式错误。"));
            DataBaseContext.ArticleTypes.AddRange(new ArticleType()
            {
                Name = name,
                ParentArticleTypeGuid = parentArticleTypeGuid,
                Sequence = sequence
            });
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
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleTypeName, new ParametersFormatErrorException("类别格式错误。"));
            var articleType = DataBaseContext.ArticleTypes.Where(q => q.Guid == guid).FirstOrDefault();
            articleType.Name = name;
            articleType.ParentArticleTypeGuid = parentArticleTypeGuid;
            articleType.Sequence = sequence;
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
                ArticleLabels = LabelManager_GetData(pageInfo, searchMessage),
                PageInfo = pageInfo,
                SearchMessage = searchMessage
            };
        }

        List<ArticleLabel> LabelManager_GetData(PageInfoViewModel pageInfo, string searchMessage)
        {
            IQueryable<ArticleLabel> q = DataBaseContext.ArticleLabels;

            // 表单搜索
            string searchText = searchMessage?.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                q = q.Where(o => o.Name.Contains(searchText));
            }

            // 在添加条件之后，排序和分页之前获取总记录数
            pageInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage(q, pageInfo);

            return q.ToList();
        }

        // POST: /Admin/LabelManager_Delete
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void LabelManager_Delete(Guid[] deleteGuid)
        {
            if (deleteGuid == null || deleteGuid.Count() == 0)
                throw new MissingParametersException("缺少参数。");
            DataBaseContext.ArticleLabels.RemoveRange(DataBaseContext.ArticleLabels.Join(deleteGuid, l => l.Guid, r => r, (l, r) => l));
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
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleLabelName, new ParametersFormatErrorException("标签格式错误。"));
            DataBaseContext.ArticleLabels.AddRange(new ArticleLabel()
            {
                Name = name
            });
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
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleLabelName, new ParametersFormatErrorException("标签格式错误。"));
            var articleLabel = DataBaseContext.ArticleLabels.Where(q => q.Guid == guid).FirstOrDefault();
            articleLabel.Name = name;
            DataBaseContext.SaveChanges();
        }
        #endregion

        #region 模板管理
        // GET: /Admin/TemplateManager
        public IActionResult TemplateManager(string searchMessage, int page = 0)
        {
            return View(TemplateManager_LoadData(page, searchMessage));
        }

        TemplateManagerViewModel TemplateManager_LoadData(int page, string searchMessage) {
            var pageInfo = new PageInfoViewModel
            {
                PageSize = 10,
                SortDirection = "ASC",
                SortField = "VirtualPath",
                PageIndex = page
            };
            return new TemplateManagerViewModel()
            {
                TemplateFiles = TemplateManager_GetData(pageInfo, searchMessage),
                PageInfo = pageInfo,
                SearchMessage = searchMessage
            };
        }

        List<TemplateFile> TemplateManager_GetData(PageInfoViewModel pageInfo, string searchMessage)
        {
            IQueryable<TemplateFile> q = DataBaseContext.TemplateFiles.Include(o => o.UploadedFile);

            // 表单搜索
            string searchText = searchMessage?.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                q = q.Where(o => o.Name.Contains(searchText));
            }

            // 在添加条件之后，排序和分页之前获取总记录数
            pageInfo.RecordCount = q.Count();

            // 排列和数据库分页
            q = SortAndPage(q, pageInfo);

            return q.ToList();
        }

        // POST: /Admin/TemplateManager_Delete
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateManager_Delete(Guid[] deleteGuid) {
            if (deleteGuid == null || deleteGuid.Count() == 0) 
                throw new MissingParametersException("缺少参数。");
            var uploadHelper = new UploadHelper(DataBaseContext, HostingEnvironment);
            deleteGuid.ToList().ForEach(d =>
            {
                var templateFiles = DataBaseContext.TemplateFiles.FirstOrDefault(q => q.Guid == d);
                var uploadedFileGuid = templateFiles.UploadedFileGuid;
                DataBaseContext.TemplateFiles.Remove(templateFiles);
                uploadHelper.CheckAndDeleteFile(uploadedFileGuid);
            });
            DataBaseContext.SaveChanges();
        }

        class TemplateManager_AddItemVirtualPathIsExistException : Exception
        {
            public TemplateManager_AddItemVirtualPathIsExistException(string message) : base(message) { }
        }

        // POST: /Admin/TemplateManager_AddItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateManager_AddItem(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath) || Request.Form.Files.Count() != 1)
                throw new MissingParametersException("缺少参数。");
            FormatVerificationHelper.FormatVerification(virtualPath, FormatVerificationHelper.FormatType.VirtualPath, new ParametersFormatErrorException("虚拟路径格式错误。"));
            if (DataBaseContext.TemplateFiles.Where(q => q.VirtualPath == virtualPath).Count() > 0)
                throw new TemplateManager_AddItemVirtualPathIsExistException("此虚拟路径已经存在。");
            var uploadHelper = new UploadHelper(DataBaseContext, HostingEnvironment);
            var formFileInfos = uploadHelper.SaveFiles(HttpContext);
            DataBaseContext.TemplateFiles.AddRange(formFileInfos.Select(formFile => new TemplateFile()
            {
                MIME = formFile.MIME,
                Name = formFile.FileName,
                UploadedFile = formFile.UploadedFile,
                VirtualPath = virtualPath
            }));
            DataBaseContext.SaveChanges();
        }

        // POST: /Admin/TemplateManager_EditItem
        [ApiAction]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public void TemplateManager_EditItem(Guid guid, string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                throw new MissingParametersException("缺少参数。");
            FormatVerificationHelper.FormatVerification(virtualPath, FormatVerificationHelper.FormatType.VirtualPath, new ParametersFormatErrorException("虚拟路径格式错误。"));
            if (DataBaseContext.TemplateFiles.Where(q => q.VirtualPath == virtualPath).Count() > 0)
                throw new TemplateManager_AddItemVirtualPathIsExistException("此虚拟路径已经存在。");
            var templateFiles = DataBaseContext.TemplateFiles.Where(q => q.Guid == guid).FirstOrDefault();
            templateFiles.VirtualPath = virtualPath;
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
            var uploadHelper = new UploadHelper(DataBaseContext, HostingEnvironment);
            using (var transaction = DataBaseContext.Database.BeginTransaction())
            {
                //清除所有模板文件
                DataBaseContext.TemplateFiles.Select(o => o.Guid).ToList().ForEach(d =>
                {
                    var templateFile = DataBaseContext.TemplateFiles.FirstOrDefault(q => q.Guid == d);
                    var uploadedFileGuid = templateFile.UploadedFileGuid;
                    DataBaseContext.TemplateFiles.Remove(templateFile);
                    uploadHelper.CheckAndDeleteFile(uploadedFileGuid);
                });
                DataBaseContext.SaveChanges();
                var formFileInfos = uploadHelper.SaveZipFiles(HttpContext);
                DataBaseContext.TemplateFiles.AddRange(formFileInfos.Select(formFile => new TemplateFile()
                {
                    MIME = formFile.MIME,
                    Name = formFile.FileName,
                    UploadedFile = formFile.UploadedFile,
                    VirtualPath = formFile.VirtualPath
                }));
                DataBaseContext.SaveChanges();
                transaction.Commit();
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            base.OnActionExecuting(context);
            if (context.Result != null) return;
            ConfigHelper configHelper = new ConfigHelper(DataBaseContext);
            ViewBag.userName = configHelper.UserName;
            ViewBag.articlesCount = DataBaseContext.Articles.Count();
            ViewBag.articleTypesCount = DataBaseContext.ArticleTypes.Count();
            ViewBag.articleLabelsCount = DataBaseContext.ArticleLabels.Count();
        }
        #endregion
    }
}
