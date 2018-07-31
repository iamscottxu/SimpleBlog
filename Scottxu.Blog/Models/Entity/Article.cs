using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Helper;
using Scottxu.Blog.Models.ViewModel;

namespace Scottxu.Blog.Models.Entitys
{
    /// <summary>
    /// 文章实体
    /// </summary>
    public class Article : GuidEntity
    {
        /// <summary>
        /// 获取或设置文章名。
        /// </summary>
        /// <value>文章名</value>
        [Required, StringLength(50)]
        public String Name { get; set; }

        /// <summary>
        /// 获取或设置发布时间。
        /// </summary>
        /// <value>发布时间</value>
        [Required]
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// 获取或设置内容。
        /// </summary>
        /// <value>内容</value>
        [MaxLength]
        public String Content { get; set; }

        /// <summary>
        /// 获取或设置点击次数。
        /// </summary>
        /// <value>点击次数</value>
        [Required]
        [ConcurrencyCheck]
        public UInt64 ClickTraffic { get; set; }

        /// <summary>
        /// 获取或设置文章类型。
        /// </summary>
        /// <value>文章类型</value>
        [Required]
        public virtual ArticleType ArticleType { get; set; }

        [Required]
        public Guid ArticleTypeGuid { get; set; }

        /// <summary>
        /// 获取或设置文章标签文章的集合。
        /// </summary>
        /// <value>文章标签文章的集合</value>
        public virtual ICollection<ArticleLabelArticle> ArticleLabelArticles { get; set; }

        /// <summary>
        /// 获取或设置上传文件文章的集合
        /// </summary>
        /// <value>上传文件文章</value>
        public virtual ICollection<UploadedFileArticle> UploadedFileArticles { get; set; }

        /// <summary>
        /// 初始化类 <see cref="T:Scottxu.Blog.Models.Article"/> 的一个新的实例。
        /// </summary>
        public Article() {
            ClickTraffic = 0;
            PublishDate = DateTime.UtcNow;
        }

        public static List<Article> GetData(BlogSystemContext dataBaseContext, PageInfoViewModel pageInfo, string searchMessage, Guid? articleTypeGuid = null, Guid? articleLabelGuid = null, bool getArticleType = true, bool getArticleLabels = true, bool getText = true)
        {
            var efHelper = new EFHelper(dataBaseContext);

            IQueryable<Article> q;
            if (articleLabelGuid.HasValue)
                q = dataBaseContext.ArticleLabelArticles.Where(
                    o => o.ArticleLabelGuid == articleLabelGuid).Select(o => o.Article);
            else q = dataBaseContext.Articles;

            IQueryable<ArticleLabelArticle> m = dataBaseContext.ArticleLabelArticles;

            m = m.Include(o => o.Article)
                .Include(o => o.ArticleLabel);

            q = q.Include(o => o.ArticleType);

            if (articleTypeGuid.HasValue)
            {
                var selectedArticleTypeGuids = ArticleType.GetVirtualTree(dataBaseContext, articleTypeGuid.Value);
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
                q = efHelper.SortAndPage(q, pageInfo);
            }

            var articles = q.ToList();

            articles.ForEach(o => o.ArticleLabelArticles = m.Where(p => p.Article == o).ToList());

            return articles;
        }

        public static void Delete(BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment, Guid[] deleteGuid)
        {
            var uploadHelper = new UploadHelper(dataBaseContext, hostingEnvironment);
            deleteGuid.ToList().ForEach(d =>
            {
                var article = dataBaseContext.Articles.FirstOrDefault(q => q.Guid == d);
                var uploadedFileArticles = dataBaseContext.UploadedFileArticles.Where(q => q.Article == article).ToList();
                uploadedFileArticles.ForEach(q => {
                    dataBaseContext.UploadedFileArticles.Remove(q);
                    uploadHelper.CheckAndDeleteFile(q.UploadedFileGuid);
                });
                dataBaseContext.Articles.Remove(article);
            });
        }

        public static void AddItem(BlogSystemContext dataBaseContext, string name, Guid articleTypeGuid, Guid[] articleLabelGuids, string content)
        {
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleName, new ParametersFormatErrorException("文章名格式错误。"));
            var article = new Article()
            {
                Name = name,
                ArticleTypeGuid = articleTypeGuid,
                Content = content
            };
            dataBaseContext.Articles.Add(article);
            dataBaseContext.ArticleLabelArticles.AddRange(articleLabelGuids.Select(o => new ArticleLabelArticle
            {
                ArticleLabelGuid = o,
                Article = article
            }));
        }

        public static void EditItem(BlogSystemContext dataBaseContext, Guid guid, string name, Guid articleTypeGuid, Guid[] articleLabelGuids, string content)
        {
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleName, new ParametersFormatErrorException("文章名格式错误。"));
            var article = dataBaseContext.Articles.FirstOrDefault(q => q.Guid == guid);
            article.Name = name;
            article.ArticleTypeGuid = articleTypeGuid;
            article.Content = content;
            dataBaseContext.ArticleLabelArticles.RemoveRange(dataBaseContext.ArticleLabelArticles.Where(o => o.Article == article));
            dataBaseContext.ArticleLabelArticles.AddRange(articleLabelGuids.Select(o => new ArticleLabelArticle
            {
                ArticleLabelGuid = o,
                Article = article
            }));
        }

        public static Article GetArticle(BlogSystemContext dataBaseContext, Guid guid, bool addUpClickTraffic = false)
        {
            var article = dataBaseContext.Articles.Include(o => o.ArticleType).FirstOrDefault(o => o.Guid == guid);
            if (addUpClickTraffic) article.ClickTraffic++;
            return article;
        }
    }
}
