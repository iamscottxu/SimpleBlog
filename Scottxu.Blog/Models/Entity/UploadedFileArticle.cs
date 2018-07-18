using System;
using System.ComponentModel.DataAnnotations;
using Scottxu.Blog.Models.Entitys;
namespace Scottxu.Blog.Models.Entitys
{
    /// <summary>
    /// 上传文件文章实体
    /// </summary>
    public class UploadedFileArticle
    {
        /// <summary>
        /// 获取或设置上传文件。
        /// </summary>
        /// <value>上传文件</value>
        public virtual UploadedFile UploadedFile { get; set; }

        public Guid UploadedFileGuid { get; set; }

        /// <summary>
        /// 获取或设置文章。
        /// </summary>
        /// <value>文章</value>
        public virtual Article Article { get; set; }

        public Guid ArticleGuid { get; set; }

        /// <summary>
        /// 获取或设置文件的名称。
        /// </summary>
        /// <value>文件的名称</value>
        [StringLength(260)]
        public String Name { get; set; }

        /// <summary>
        /// 获取或设置文件MIME类型。
        /// </summary>
        /// <value>MIME类型</value>
        [MaxLength]
        public String MIME { get; set; }
    }
}
