using System;
using System.Collections.Generic;
using System.Linq;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Entitys;
using Scottxu.Blog.Models.ViewModel;
using Scottxu.Blog.Models.Helper;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class APIController : BaseController
    {
        public APIController(BlogSystemContext context) : base(context) { }

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
            var article = DataBaseContext.Articles.Include(o => o.ArticleType).FirstOrDefault(o => o.Guid == articleGuid);
            var articleLabel = DataBaseContext.ArticleLabelArticles.Include(o => o.ArticleLabel).Where(o => o.Article == article).Select(o => o.ArticleLabel).ToList();
            article.ClickTraffic++;
            DataBaseContext.SaveChanges();
            return new {
                article.Name,
                article.ClickTraffic,
                article.Content,
                article.PublishDate,
                ArticleType = new {
                    article.ArticleType.Guid,
                    article.ArticleType.Name
                },
                ArticleLabels = articleLabel.Select(o => new {
                    o.Guid,
                    o.Name
                })
            };
        }

        // GET: /API/GetArticleList
        [ApiAction]
        public APIModel GetArticleList(string searchMessage, Guid? articleTypeGuid, Guid? articleLabelGuid, string sortField = "Name", string sortDirection = "ASC", int pageSize = -1, int pageIndex = 0, bool getArticleType = false, bool getArticleLabels = false, bool getText = false)
        {
            return GetArticleList_LoadData(searchMessage, articleTypeGuid, articleLabelGuid, sortField, sortDirection, pageSize, pageIndex, getArticleType, getArticleLabels, getText);
        }

        APIModel GetArticleList_LoadData(string searchMessage, Guid? articleTypeGuid, Guid? articleLabelGuid, string sortField, string sortDirection, int pageSize, int pageIndex, bool getArticleType, bool getArticleLabels, bool getText)
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
                List = GetArticleList_GetData(pageInfo, searchMessage, articleTypeGuid, articleLabelGuid, getArticleType, getArticleLabels, getText)
            };
        }

        object GetArticleList_GetData(PageInfoViewModel pageInfo, string searchMessage, Guid? articleTypeGuid, Guid? articleLabelGuid, bool getArticleType, bool getArticleLabels, bool getText)
        {
            IQueryable<Article> q;
            if (articleLabelGuid.HasValue)
                q = DataBaseContext.ArticleLabelArticles.Where(
                    o => o.ArticleLabelGuid == articleLabelGuid).Select(o => o.Article);
            else q = DataBaseContext.Articles;

            IQueryable<ArticleLabelArticle> m = DataBaseContext.ArticleLabelArticles;

            m = m.Include(o => o.Article)
                .Include(o => o.ArticleLabel);

            q = q.Include(o => o.ArticleType);
                
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

            if (pageInfo != null)
            {
                // 在添加条件之后，排序和分页之前获取总记录数
                pageInfo.RecordCount = q.Count();

                // 排列和数据库分页
                q = SortAndPage(q, pageInfo);
            }

            var articles = q.ToList();

            articles.ForEach(o => o.ArticleLabelArticles = m.Where(p => p.Article == o).ToList());

            return articles.Select(o => new {
                o.Guid, 
                o.Name,
                o.PublishDate,
                o.ClickTraffic,
                ArticleType = getArticleType ? new {
                    o.ArticleType.Guid,
                    o.ArticleType.Name
                } : null,
                ArticleLabels = getArticleLabels ? o.ArticleLabelArticles.Select(p => new {
                    p.ArticleLabel.Guid,
                    p.ArticleLabel.Name
                }) : null,
                text = getText ? GetArticleList_GetHtmlText(o.Content) : null
            });
        }

        string GetArticleList_GetHtmlText(string html)
        {
            try {
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
        public APIModel GetLabelList(string searchMessage, string sortField = "Name", string sortDirection = "ASC", int pageSize = -1, int pageIndex = 0)
        {
            
            return GetLabelList_LoadData(searchMessage, sortField, sortDirection, pageSize, pageIndex);
        }

        APIModel GetLabelList_LoadData(string searchMessage, string sortField, string sortDirection, int pageSize, int pageIndex)
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
                List = GetLabelList_GetData(pageInfo, searchMessage)
            };
        }

        object GetLabelList_GetData(PageInfoViewModel pageInfo, string searchMessage)
        {
            IQueryable<ArticleLabel> q = DataBaseContext.ArticleLabels;

            // 表单搜索
            string searchText = searchMessage?.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                q = q.Where(o => o.Name.Contains(searchText));
            }

            if (pageInfo != null)
            {
                // 在添加条件之后，排序和分页之前获取总记录数
                pageInfo.RecordCount = q.Count();

                // 排列和数据库分页
                q = SortAndPage(q, pageInfo);
            }

            return q.Select(o => new { o.Guid, o.Name }).ToList();
        }

        // GET: /API/GetTypeList
        [ApiAction]
        public object GetTypeList()
        {
            
            return GetTypeList_LoadData();
        }

        object GetTypeList_LoadData()
        {
            (object childArticleTypes, int articlesCount) = GetTypeList_GetData();
            return childArticleTypes;
        }

        (IEnumerable<object>, int) GetTypeList_GetData(Guid? parentArticleTypeGuid = null)
        {
            IQueryable<ArticleType> q = DataBaseContext.ArticleTypes;

            q = q.Include(o => o.Articles);

            q = q.Where(o => o.ParentArticleTypeGuid == parentArticleTypeGuid);

            q.OrderBy(o => o.Sequence);

            var list = q.Select(o => new { o.Guid, o.Name, ArticlesCount = o.Articles.Count }).ToList()
                    .Select(o => 
            {
                (object childArticleTypes, int articlesCount) = GetTypeList_GetData(o.Guid);
                articlesCount += o.ArticlesCount;
                return new { o.Guid, o.Name, ArticlesCount = articlesCount, ChildArticleTypes = childArticleTypes };
            });
            return (list, list.Sum(s => s.ArticlesCount));
        }
         
        //GET: /API/GetBaseInfo
        [ApiAction]
        public object GetBaseInfo(bool getBlogName, bool getUserName) {
            var configHelper = new ConfigHelper(DataBaseContext);
            var blogName = getBlogName ? configHelper.BlogName : null;
            var userName = getUserName ? configHelper.UserName : null;
            return new { blogName, userName };
        }
    }
}
