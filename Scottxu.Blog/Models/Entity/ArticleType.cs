using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.ViewModel;
using Scottxu.Blog.Models.Helper;
using Scottxu.Blog.Models.Exception;
using Microsoft.EntityFrameworkCore;

namespace Scottxu.Blog.Models.Entitys
{
    /// <summary>
    /// 文章类型
    /// </summary>
    public class ArticleType : GuidEntity
    {
        /// <summary>
        /// 获取或设置文章类型名称。
        /// </summary>
        /// <value>文章类型名称</value>
        [Required, StringLength(50)]
        public String Name { get; set; }

        /// <summary>
        /// 获取或设置排序顺序。
        /// </summary>
        /// <value>排序顺序</value>
        [Required]
        public UInt32 Sequence { get; set; }

        /// <summary>
        /// 获取或设置父级文章类型。
        /// </summary>
        /// <value>父级文章类型</value>
        public virtual ArticleType ParentArticleType { get; set; }

        /// <summary>
        /// 获取或设置父级文章类型的Guid。
        /// </summary>
        /// <value>父级文章类型的Guid</value>
        public Guid? ParentArticleTypeGuid { get; set; }

        /// <summary>
        /// 获取或设置子级文章类型的集合。
        /// </summary>
        /// <value>文章类型的集合</value>
        public virtual ICollection<ArticleType> ChildArticleTypes { get; set; }

        /// <summary>
        /// 获取或设置文章的集合。
        /// </summary>
        /// <value>文章的集合</value>
        public virtual ICollection<Article> Articles { get; set; }

        public static List<ArticleType> GetVirtualTree(BlogSystemContext dataBaseContext, Guid? articleTypeGuid = null) {
            var articleTypes = new List<ArticleType>();
            AddChildArticleTypes(dataBaseContext, articleTypeGuid, articleTypes, string.Empty);
            if (articleTypeGuid != null) articleTypes.Add(dataBaseContext.ArticleTypes.FirstOrDefault(q => q.Guid == articleTypeGuid));
            return articleTypes;
        }

        public static (IEnumerable<object>, int) GetTree(BlogSystemContext dataBaseContext, Guid? parentArticleTypeGuid = null)
        {
            IQueryable<ArticleType> q = dataBaseContext.ArticleTypes;

            q = q.Include(o => o.Articles);

            q = q.Where(o => o.ParentArticleTypeGuid == parentArticleTypeGuid);

            q.OrderBy(o => o.Sequence);

            var list = q.Select(o => new { o.Guid, o.Name, ArticlesCount = o.Articles.Count }).ToList()
                    .Select(o =>
                    {
                (object childArticleTypes, int articlesCount) = GetTree(dataBaseContext, o.Guid);
                        articlesCount += o.ArticlesCount;
                        return new { o.Guid, o.Name, ArticlesCount = articlesCount, ChildArticleTypes = childArticleTypes };
                    });
            return (list, list.Sum(s => s.ArticlesCount));
        }

        static void AddChildArticleTypes(BlogSystemContext dataBaseContext, Guid? articleTypeGuid, List<ArticleType> articleTypes, string prefix) {
            var _articleTypes = dataBaseContext.ArticleTypes.Where(q => q.ParentArticleTypeGuid == articleTypeGuid).OrderBy(o => o.Sequence).ToList();
            _articleTypes.ForEach(a => a.Name = $"{prefix}{a.Name}");
            _articleTypes.ForEach(a => {
                articleTypes.Add(a);
                AddChildArticleTypes(dataBaseContext, a.Guid, articleTypes, $"{prefix}　");
            });
        }

        public static List<ArticleType> GetData(BlogSystemContext dataBaseContext, PageInfoViewModel pageInfo, string searchMessage, out List<ArticleType> articleTypes)
        {
            articleTypes = GetVirtualTree(dataBaseContext);

            List<ArticleType> searchArticleTypes;
            // 表单搜索
            string searchText = searchMessage?.Trim();
            if (!string.IsNullOrEmpty(searchText))
                searchArticleTypes = articleTypes.Where(o => o.Name.Contains(searchText)).ToList();
            else searchArticleTypes = articleTypes;

            if (pageInfo != null)
            {
                // 在添加条件之后，排序和分页之前获取总记录数
                pageInfo.RecordCount = searchArticleTypes.Count();

                // 排列和分页
                searchArticleTypes = searchArticleTypes.Skip(pageInfo.PageIndex * pageInfo.PageSize).Take(pageInfo.PageSize).ToList();
            }
            return searchArticleTypes;
        }

        public static void Delete(BlogSystemContext dataBaseContext, Guid[] deleteGuid) => dataBaseContext.ArticleTypes.RemoveRange(dataBaseContext.ArticleTypes.Join(deleteGuid, l => l.Guid, r => r, (l, r) => l));

        public static void AddItem(BlogSystemContext dataBaseContext, string name, Guid? parentArticleTypeGuid, UInt32 sequence)
        {
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleTypeName, new ParametersFormatErrorException("类别格式错误。"));
            dataBaseContext.ArticleTypes.AddRange(new ArticleType()
            {
                Name = name,
                ParentArticleTypeGuid = parentArticleTypeGuid,
                Sequence = sequence
            });
        }

        public static void EditItem(BlogSystemContext dataBaseContext, Guid guid, string name, Guid? parentArticleTypeGuid, UInt32 sequence)
        {
            FormatVerificationHelper.FormatVerification(name, FormatVerificationHelper.FormatType.ArticleTypeName, new ParametersFormatErrorException("类别格式错误。"));
            var articleType = dataBaseContext.ArticleTypes.Where(q => q.Guid == guid).FirstOrDefault();
            articleType.Name = name;
            articleType.ParentArticleTypeGuid = parentArticleTypeGuid;
            articleType.Sequence = sequence;
        }
    }
}
