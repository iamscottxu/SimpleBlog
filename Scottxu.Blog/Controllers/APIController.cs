using System;
using System.Linq;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Entities;
using Scottxu.Blog.Models.ViewModels;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Scottxu.Blog.Models.Units;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class APIController : BaseController
    {
        
        IDistributedCache Cache  { get; }

        public APIController(BlogSystemContext context, IDistributedCache cache, IOptions<SiteOptions> options)
            : base(context, options) => Cache = cache;

        public class APIModel
        {
            public PageInfoViewModel PageInfo { get; set; }
            public object List { get; set; }
            public string SearchMessage { get; set; }
        }

        // GET: /API/GetArticle
        [ApiAction]
        public object GetArticle(Guid articleGuid)
        {
            var article = Article.GetArticle(DataBaseContext, articleGuid, true);
            var articleLabel = ArticleLabel.GetAllDataByArticle(DataBaseContext, article);
            DataBaseContext.SaveChanges();
            return new
            {
                article.Name,
                article.ClickTraffic,
                article.Content,
                article.PublishDate,
                ArticleType = new
                {
                    article.ArticleType.Guid,
                    article.ArticleType.Name
                },
                ArticleLabels = articleLabel.Select(o => new
                {
                    o.Guid,
                    o.Name
                })
            };
        }

        // GET: /API/GetArticleList
        [ApiAction]
        public APIModel GetArticleList(string searchMessage, Guid? articleTypeGuid, Guid? articleLabelGuid,
            string sortField = "Name", string sortDirection = "ASC", int pageSize = -1, int pageIndex = 0,
            bool getArticleType = false, bool getArticleLabels = false, bool getText = false)
        {
            return GetArticleList_LoadData(searchMessage, articleTypeGuid, articleLabelGuid, sortField, sortDirection,
                pageSize, pageIndex, getArticleType, getArticleLabels, getText);
        }

        APIModel GetArticleList_LoadData(string searchMessage, Guid? articleTypeGuid, Guid? articleLabelGuid,
            string sortField, string sortDirection, int pageSize, int pageIndex, bool getArticleType,
            bool getArticleLabels, bool getText)
        {
            PageInfoViewModel pageInfo = null;
            if (pageSize > 0)
                pageInfo = new PageInfoViewModel
                {
                    PageSize = pageSize,
                    SortDirection = sortDirection,
                    SortField = sortField,
                    PageIndex = pageIndex
                };

            var articles = Article.GetData(DataBaseContext, pageInfo, searchMessage, articleTypeGuid, articleLabelGuid,
                getArticleType, getArticleLabels, getText);

            return new APIModel()
            {
                PageInfo = pageInfo,
                SearchMessage = searchMessage,
                List =
                    articles.Select(o => new
                    {
                        o.Guid,
                        o.Name,
                        o.PublishDate,
                        o.ClickTraffic,
                        ArticleType = getArticleType
                            ? new
                            {
                                o.ArticleType.Guid,
                                o.ArticleType.Name
                            }
                            : null,
                        ArticleLabels = getArticleLabels
                            ? o.ArticleLabelArticles.Select(p => new
                            {
                                p.ArticleLabel.Guid,
                                p.ArticleLabel.Name
                            })
                            : null,
                        text = getText ? GetArticleList_GetHtmlText(o.Content) : null
                    })
            };
        }

        string GetArticleList_GetHtmlText(string html)
        {
            try
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                var text = htmlDocument.DocumentNode.InnerText;
                if (text.Count() > 100) text = text.Remove(100) + "……";
                return text;
            }
            catch
            {
                return null;
            }
        }

        // GET: /API/GetLabelList
        [ApiAction]
        public APIModel GetLabelList(string searchMessage, string sortField = "Name", string sortDirection = "ASC",
            int pageSize = -1, int pageIndex = 0)
        {
            return GetLabelList_LoadData(searchMessage, sortField, sortDirection, pageSize, pageIndex);
        }

        APIModel GetLabelList_LoadData(string searchMessage, string sortField, string sortDirection, int pageSize,
            int pageIndex)
        {
            PageInfoViewModel pageInfo = null;
            if (pageSize > 0)
                pageInfo = new PageInfoViewModel
                {
                    PageSize = pageSize,
                    SortDirection = sortDirection,
                    SortField = sortField,
                    PageIndex = pageIndex
                };
            return new APIModel()
            {
                PageInfo = pageInfo,
                SearchMessage = searchMessage,
                List = ArticleLabel.GetData(DataBaseContext, pageInfo, searchMessage).Select(
                    o => new {o.Guid, o.Name}).ToList()
            };
        }

        // GET: /API/GetTypeList
        [ApiAction]
        public object GetTypeList()
        {
            return GetTypeList_LoadData();
        }

        object GetTypeList_LoadData()
        {
            (object childArticleTypes, var articlesCount) = ArticleType.GetTree(DataBaseContext);
            return childArticleTypes;
        }

        //GET: /API/GetBaseInfo
        [ApiAction]
        public object GetBaseInfo(bool getBlogName, bool getUserName)
        {
            var configUnit = new ConfigUnit(DataBaseContext, Cache);
            var blogName = getBlogName ? configUnit.BlogName : null;
            var userName = getUserName ? configUnit.UserName : null;
            return new {blogName, userName};
        }
    }
}