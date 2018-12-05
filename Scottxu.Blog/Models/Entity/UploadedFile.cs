using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Scottxu.Blog.Models.Entitys
{
    /// <summary>
    /// 上传的文件实体
    /// </summary>
    public class UploadedFile : GuidEntity
    {
        /// <summary>
        /// 获取或设置SHA1值。
        /// </summary>
        /// <value>SHA1值</value>
        [Required, MaxLength]
        public string SHA1 { get; set; }

        /// <summary>
        /// 获取或设置实际文件名。
        /// </summary>
        /// <value>实际文件名</value>
        [Required, StringLength(260)]
        public string FileName { get; set; }

        /// <summary>
        /// 获取或设置上传文件文章的集合
        /// </summary>
        /// <value>上传文件文章</value>
        public virtual ICollection<UploadedFileArticle> UploadedFileArticles { get; set; }

        /// <summary>
        /// 获取或设置模板集合。
        /// </summary>
        /// <value>模板集合</value>
        public virtual ICollection<TemplateFile> TemplateFiles { get; set; }
    }
}