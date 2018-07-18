using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
    }
}
