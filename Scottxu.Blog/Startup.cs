using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Scottxu.Blog.Models;
using Scottxu.Blog.TemplateParsingMiddleware;

namespace Scottxu.Blog
{
    // <summary>
    /// Web宿主入口类
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// 加入服务项到容器， 这个方法将会被runtime调用
        /// </summary>
        /// <param name="services">Services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            Action<DbContextOptionsBuilder> dbContextOptionsAction = null;
            switch (Configuration["DataBaseType"]) {
                case "SQLServer":
                    dbContextOptionsAction = options => options.UseSqlServer(Configuration.GetConnectionString("SQLServerConnection"));
                    break;
                case "SQLite":
                    dbContextOptionsAction = options => options.UseSqlite(Configuration.GetConnectionString("SQLiteConnection"));
                    break;
                case "MySQL":
                    dbContextOptionsAction = options => options.UseMySql(Configuration.GetConnectionString("MySQLConnection"));
                    break;
            }
            services.AddDbContext<BlogSystemContext>(dbContextOptionsAction);

            services.AddAuthentication(options => {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options => {
                options.LoginPath = "/sys/Auth";
                options.AccessDeniedPath = "/sys/Home/Denied";
                options.LogoutPath = "/sys/Auth/Logout";
                options.Cookie.Name = "SimpleBlog_AuthTicket";
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromDays(150);
                options.ExpireTimeSpan = TimeSpan.FromDays(150);
            });

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie = new CookieBuilder()
                {
                    Name = "SimpleBlog_SessionId",
                    Path = "/",
                    HttpOnly = true
                };
            });

            services.AddMvc();

            services.AddTransient<TemplateParsing>();
        }

        /// <summary>
        /// 配置HTTP请求管道
        /// </summary>
        /// <param name="app">被用于构建应用程序的请求管道 只可以在Startup中的Configure方法里使用</param>
        /// <param name="env">提供了访问应用程序属性，如环境变量</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/sys/Home/Error");
            }

            app.UseStatusCodePagesWithReExecute("/sys/Home/Error/{0}");

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseSession();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "sys/{controller=Home}/{action=Index}/{id?}");
            });

            app.UseTemplateParsing(); //博客模板解析中间件
        }
    }
}
