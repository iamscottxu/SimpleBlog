using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Helper;
using Scottxu.Blog.Models.ViewModel;

namespace Scottxu.Blog.Models.Entitys
{
    /// <summary>
    /// 文章标签
    /// </summary>
    public class ArticleLabel : GuidEntity
    {
        /// <summary>
        /// 获取或设置文章标签的名称。
        /// </summary>
        /// <value>文章标签的名称</value>
        [Required, StringLength(50)]
        public String Name { get; set; }

        /// <summary>
        /// 获取或设置文章标签文章的集合。
        /// </summary>
        /// <value>文章标签文章的集合</value>
        public virtual ICollection<ArticleLabelArticle> ArticleLabelArticles { get; set; }

        public static List<ArticleLabel> GetData(BlogSystemContext dataBaseContext, PageInfoViewModel pageInfo, string searchMessage)
        {
            var efHelper = new EFHelper(dataBaseContext);

            IQueryable<ArticleLabel> q = dataBaseContext.ArticleLabels;

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

            return q.ToList();
        }

        public static void Delete(BlogSystemContext dataBaseContext, Guid[] deleteGuid) => dataBaseContext.ArticleLabels.RemoveRange(dataBaseContext.ArticleLabels.Join(deleteGuid, l => l.Guid, r => r, (l, r) => l));

        public static void AddItem(BlogSystemContext dataBaseContext, string name)
        {
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleLabelName, new ParametersFormatErrorException("标签格式错误。"));
            dataBaseContext.ArticleLabels.AddRange(new ArticleLabel()
            {
                Name = name
            });
        }

        public static void EditItem(BlogSystemContext dataBaseContext, Guid guid, string name)
        {
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleLabelName, new ParametersFormatErrorException("标签格式错误。"));
            var articleLabel = dataBaseContext.ArticleLabels.Where(q => q.Guid == guid).FirstOrDefault();
            articleLabel.Name = name;
        }

        public static List<ArticleLabel> GetAllDataByArticle(BlogSystemContext dataBaseContext, Article article) => dataBaseContext.ArticleLabelArticles.Include(o => o.ArticleLabel).Where(o => o.Article == article).Select(o => o.ArticleLabel).ToList();
    }
}
