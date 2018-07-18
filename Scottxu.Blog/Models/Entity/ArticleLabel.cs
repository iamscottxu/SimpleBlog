using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
    }
}
