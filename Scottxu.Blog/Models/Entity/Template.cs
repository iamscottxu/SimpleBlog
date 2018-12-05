using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Scottxu.Blog.Models.Entitys
{
    /// <summary>
    /// 页面模板
    /// </summary>
    [Serializable]
    public class Template : GuidEntity
    {
        /// <summary>
        /// 获取或设置模板的名称。
        /// </summary>
        /// <value>模板的名称</value>
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置模板集合。
        /// </summary>
        /// <value>模板集合</value>
        [XmlIgnoreAttribute]
        public virtual ICollection<TemplateFile> TemplateFiles { get; set; }
    }
}