using System;
using System.ComponentModel.DataAnnotations;

namespace Scottxu.Blog.Models.Entities
{
    /// <summary>
    /// 网盘文件实体
    /// </summary>
    public class DiskFile
    {
        /// <summary>
        /// 获取或设置上传文件。
        /// </summary>
        /// <value>上传文件</value>
        public virtual UploadedFile UploadedFile { get; set; }

        public Guid UploadedFileGuid { get; set; }


        public Guid ArticleGuid { get; set; }

        /// <summary>
        /// 获取或设置文件的名称。
        /// </summary>
        /// <value>文件的名称</value>
        [Required, StringLength(260)]
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置文件MIME类型。
        /// </summary>
        /// <value>MIME类型</value>
        [MaxLength]
        public string MIME { get; set; }

        /// <summary>
        /// 获取或设置文件夹。
        /// </summary>
        /// <value>文件夹</value>
        public virtual DiskFileFolder DiskFileFolder { get; set; }

        public Guid DiskFileFolderGuid { get; set; }
    }
}