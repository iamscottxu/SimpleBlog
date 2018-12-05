using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scottxu.Blog.Models.Entitys
{
    /// <summary>
    /// 配置实体
    /// </summary>
    public class Config
    {
        /// <summary>
        /// 获取或设置键。
        /// </summary>
        /// <value>键</value>
        [Key, StringLength(50), DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Key { get; set; }

        /// <summary>
        /// 获取或设置值。
        /// </summary>
        /// <value>值</value>
        [MaxLength]
        public string Value { get; set; }
    }
}