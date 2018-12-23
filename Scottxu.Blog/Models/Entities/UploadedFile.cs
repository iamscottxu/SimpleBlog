using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Scottxu.Blog.Models.Entities
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
        /// 获取或设置文件大小。
        /// </summary>
        /// <value>文件大小</value>
        [Required]
        public long Size { get; set; }

        /// <summary>
        /// 获取或设置实际文件名。
        /// </summary>
        /// <value>实际文件名</value>
        [StringLength(260)]
        public string FileName { get; set; }
        
        
        /// <summary>
        /// 获取或设置Gzip压缩到实际文件名。
        /// </summary>
        /// <value>实际文件名</value>
        [StringLength(260)]
        public string GzipFileName { get; set; }

        /// <summary>
        /// 获取或设置上传文件文章的集合
        /// </summary>
        /// <value>上传文件文章</value>
        public virtual ICollection<DiskFile> DiskFiles { get; set; }

        /// <summary>
        /// 获取或设置模板集合。
        /// </summary>
        /// <value>模板集合</value>
        public virtual ICollection<TemplateFile> TemplateFiles { get; set; }
    }
}