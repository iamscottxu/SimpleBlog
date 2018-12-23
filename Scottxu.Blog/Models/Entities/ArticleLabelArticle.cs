using System;

namespace Scottxu.Blog.Models.Entities
{
    /// <summary>
    /// 文章标签文章实体
    /// </summary>
    public class ArticleLabelArticle
    {
        /// <summary>
        /// 获取或设置文章标签。
        /// </summary>
        /// <value>文章标签</value>
        public virtual ArticleLabel ArticleLabel { get; set; }

        public Guid ArticleLabelGuid { get; set; }

        /// <summary>
        /// 获取或设置文章。
        /// </summary>
        /// <value>文章</value>
        public virtual Article Article { get; set; }

        public Guid ArticleGuid { get; set; }
    }
}