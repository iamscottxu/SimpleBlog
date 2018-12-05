using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scottxu.Blog.Models.Units
{
    public class AppSettingsUnit
    {
        public IConfiguration Configuration { get; }

        public AppSettingsUnit(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Action<DbContextOptionsBuilder> GetDbContextOptionsBuilder()
        {
            var connectionString = Configuration.GetConnectionString("BloggingDatabase");
            switch (Configuration["DataBaseType"])
            {
                case "SQLServer":
                    return options => options.UseSqlServer(connectionString);
                case "SQLite":
                    return options => options.UseSqlite(connectionString);
                case "MySQL":
                    return options => options.UseMySql(connectionString);
                default:
                    return null;
            }
        }

        public Action<CookieAuthenticationOptions> GetCookieAuthenticationOptions()
        {
            var sectionConfiguration = Configuration.GetSection("CookieAuthenticationOptions");
            var cookieName = sectionConfiguration["Name"];
            var cookieDomain = sectionConfiguration["Domain"];
            if (string.IsNullOrEmpty(cookieName)) cookieName = "SimpleBlog_AuthTicket";
            return options =>
            {
                options.LoginPath = "/GoAccount/Login";
                options.LogoutPath = "/GoAccount/Logout";
                options.Cookie.Name = cookieName;
                options.Cookie.Path = "/";
                if (!string.IsNullOrEmpty(cookieDomain)) options.Cookie.Domain = cookieDomain;
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromDays(150);
                options.ExpireTimeSpan = TimeSpan.FromDays(150);
            };
        }
    }
}