using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Scottxu.Blog.Models;

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

        private static void AddChildArticleTypes(BlogSystemContext dataBaseContext, Guid? articleTypeGuid, List<ArticleType> articleTypes, string prefix) {
            var _articleTypes = dataBaseContext.ArticleTypes.Where(q => q.ParentArticleTypeGuid == articleTypeGuid).OrderBy(o => o.Sequence).ToList();
            _articleTypes.ForEach(a => a.Name = $"{prefix}{a.Name}");
            _articleTypes.ForEach(a => {
                articleTypes.Add(a);
                AddChildArticleTypes(dataBaseContext, a.Guid, articleTypes, $"{prefix}　");
            });
        }
    }
}
