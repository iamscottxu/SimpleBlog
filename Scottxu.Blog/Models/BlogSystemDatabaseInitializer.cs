using System;
using Scottxu.Blog.Models.Entitys;
using Scottxu.Blog.Models.Helper;

namespace Scottxu.Blog.Models
{
    public static class BlogSystemDatabaseInitializer
    {
        public struct Configs {
            public String Email { get; set; }
            public String Password { get; set; }
            public String BlogName { get; set; }
            public string UserName { get; set; }
        }

        internal static void Seed(BlogSystemContext dataBaseContext, Configs configs)
        {
            Config(dataBaseContext, configs);
        }

        static void Config(BlogSystemContext dataBaseContext, Configs configs) {
            ConfigHelper configHelper = new ConfigHelper(dataBaseContext)
            {
                Email = configs.Email,
                Password = Util.PasswordUtil.CreateDbPassword(configs.Password, false),
                BlogName = configs.BlogName,
                UserName = configs.UserName
            };
            configHelper.SaveAll();
        }

    }
}
