using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Scottxu.Blog.Models.Entities
{
    /// <summary>
    /// 网盘文件夹实体
    /// </summary>
    public class DiskFileFolder : GuidEntity
    {
        /// <summary>
        /// 获取或设置文件夹的名称。
        /// </summary>
        /// <value>文件夹的名称</value>
        [Required, StringLength(260)]
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置父级文件夹。
        /// </summary>
        /// <value>父级文件夹</value>
        public virtual DiskFileFolder ParentDiskFileFolder { get; set; }
        
        /// <summary>
        /// 获取或设置文件的集合。
        /// </summary>
        /// <value>文件的集合</value>
        public virtual ICollection<DiskFile> DiskFiles { get; set; }
        
        /// <summary>
        /// 获取或设置文件夹的集合。
        /// </summary>
        /// <value>文件夹的集合</value>
        public ICollection<DiskFileFolder> ChildDiskFileFolders  { get; set; }

        public Guid ParentDiskFileFolderGuid { get; set; }
    }
}