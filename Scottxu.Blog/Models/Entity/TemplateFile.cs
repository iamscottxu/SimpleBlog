using System;
using System.ComponentModel.DataAnnotations;

namespace Scottxu.Blog.Models.Entitys
{
    /// <summary>
    /// 页面模板文件
    /// </summary>
    public class TemplateFile : GuidEntity
    {
        /// <summary>
        /// 获取或设置文件的名称。
        /// </summary>
        /// <value>文件的名称</value>
        [StringLength(260)]
        public String Name { get; set; }

        /// <summary>
        /// 获取或设置虚拟路径。
        /// </summary>
        /// <value>虚拟路径</value>
        [Required, StringLength(500)]
        public String VirtualPath { get; set; }

        /// <summary>
        /// 获取或设置上传文件。
        /// </summary>
        /// <value>上传文件</value>
        [Required]
        public virtual UploadedFile UploadedFile { get; set; }

        [Required]
        public Guid UploadedFileGuid { get; set; }

        /// <summary>
        /// 获取或设置文件MIME类型。
        /// </summary>
        /// <value>MIME类型</value>
        [MaxLength]
        public String MIME { get; set; }
    }
}
