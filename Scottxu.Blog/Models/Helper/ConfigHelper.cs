using System;
using System.Collections.Generic;
using System.Linq;
using Scottxu.Blog.Models.Entitys;

namespace Scottxu.Blog.Models.Helper
{
    /// <summary>
    /// 配置帮助类
    /// </summary>
    public class ConfigHelper
    {
        #region fields & constructor

        readonly BlogSystemContext dataBaseContext;

        public List<Config> Configs { get; private set; }

        List<String> changedKeys = new List<string>();

        List<Config> addKeys = new List<Config>();

        #endregion

        #region methods

        /// <summary>
        /// 初始化 <see cref="T:Scottxu.Blog.Models.Helper.ConfigHelper"/> 的实例。
        /// </summary>
        /// <param name="dataBaseContext">Data base context.</param>
        public ConfigHelper(BlogSystemContext dataBaseContext)
        {
            Configs = dataBaseContext.Configs.ToList();
            this.dataBaseContext = dataBaseContext;
        }

        /// <summary>
        /// 获取或设置 <see cref="T:Scottxu.Blog.Models.Helper.ConfigHelper"/> 的值
        /// </summary>
        /// <param name="key">键</param>
        public String this[String key] {
            get => Configs.Where(c => c.Key == key).Select(c => c.Value).FirstOrDefault();
            set {
                var config = Configs.FirstOrDefault(c => c.Key == key);
                if (config == null) {
                    addKeys.Add(new Config(){
                        Key = key,
                        Value = value
                    });
                }
                else if (config.Value != value)
                {
                    changedKeys.Add(key);
                    config.Value = value;
                }
            }
        }

        /// <summary>
        /// 保存所有更改的配置项
        /// </summary>
        public void SaveAll()
        {
            var changedConfigs = dataBaseContext.Configs.Where(c => changedKeys.Contains(c.Key));
            changedConfigs.ToList().ForEach(changed => changed.Value = this[changed.Key]);
            dataBaseContext.Configs.AddRange(addKeys);
            dataBaseContext.SaveChanges();
            Configs = dataBaseContext.Configs.ToList();
        }

        #endregion

        #region properties

        /// <summary>
        /// 后台登录邮箱
        /// </summary>
        public string Email
        {
            get => this["Email"];
            set => this["Email"] = value;
        }

        /// <summary>
        /// 后台登录密码
        /// </summary>
        public string Password
        {
            get => this["Password"];
            set => this["Password"] = value;
        }

        /// <summary>
        /// 博客名称
        /// </summary>
        public string BlogName
        {
            get => this["BlogName"];
            set => this["BlogName"] = value;
        }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName
        {
            get => this["UserName"];
            set => this["UserName"] = value;
        }
        #endregion
    }
}
