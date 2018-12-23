using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Scottxu.Blog.Models.Entities;
using Scottxu.Blog.Models.Exception;

namespace Scottxu.Blog.Models.Units
{
    /// <summary>
    /// 配置管理类
    /// </summary>
    public class ConfigUnit
    {
        #region fields & constructor

        readonly BlogSystemContext dataBaseContext;

        private readonly IDistributedCache cache;

        public List<Config> Configs { get; private set; }

        readonly List<string> _changedKeys = new List<string>();

        readonly List<Config> _addKeys = new List<Config>();

        #endregion

        #region methods

        /// <summary>
        /// 初始化 <see cref="T:Scottxu.Blog.Models.Units.ConfigHelper"/> 的实例。
        /// </summary>
        /// <param name="dataBaseContext">数据库上下文</param>
        /// <param name="cache">Redis缓存</param>
        public ConfigUnit(BlogSystemContext dataBaseContext, IDistributedCache cache)
        {
            var configsJson = cache.GetString("Configs");
            if (string.IsNullOrEmpty(configsJson))
            {
                if (!dataBaseContext.DataBaseIsExist) throw new NotDataBaseException();
                Configs = dataBaseContext.Configs.ToList();
                cache.SetString("Configs",JsonConvert.SerializeObject(Configs));
            }
            else Configs = JsonConvert.DeserializeObject<List<Config>>(configsJson);
            this.dataBaseContext = dataBaseContext;
            this.cache = cache;
        }

        /// <summary>
        /// 获取或设置 <see cref="T:Scottxu.Blog.Models.Units.ConfigHelper"/> 的值
        /// </summary>
        /// <param name="key">键</param>
        public string this[string key]
        {
            get
            {
                return Configs.Where(c => c.Key == key).Select(c => c.Value).FirstOrDefault();
            }
            set
            {
                var config = Configs.FirstOrDefault(c => c.Key == key);
                if (config == null)
                {
                    _addKeys.Add(new Config()
                    {
                        Key = key,
                        Value = value
                    });
                }
                else if (config.Value != value)
                {
                    _changedKeys.Add(key);
                    config.Value = value;
                }
            }
        }

        /// <summary>
        /// 保存所有更改的配置项
        /// </summary>
        public void SaveAll()
        {
            var changedConfigs = dataBaseContext.Configs.Where(c => _changedKeys.Contains(c.Key));
            changedConfigs.ToList().ForEach(changed => changed.Value = this[changed.Key]);
            dataBaseContext.Configs.AddRange(_addKeys);
            dataBaseContext.SaveChanges();
            Configs = dataBaseContext.Configs.ToList();
            cache.SetString("Configs",JsonConvert.SerializeObject(Configs));
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

        /// <summary>
        /// 模板GUID
        /// </summary>
        public Guid? TemplateGuid
        {
            get
            {
                if (string.IsNullOrEmpty(this["TemplateGuid"])) return null;
                return Guid.Parse(this["TemplateGuid"]);
            }
            set
            {
                this["TemplateGuid"] = value?.ToString();
                cache.Remove("TemplateFiles");
            }
        }

        #endregion
    }
}