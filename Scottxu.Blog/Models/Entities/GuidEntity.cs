using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scottxu.Blog.Models.Entities
{
    /// <summary>
    /// GUID实体
    /// </summary>
    public class GuidEntity : IKeyGuid
    {
        /// <summary>
        /// 获取或设置GUID。
        /// </summary>
        /// <value>GUID</value>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; }
    }
}