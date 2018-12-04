using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scottxu.Blog.Models;
using Scottxu.Blog.TemplateParsingMiddleware;
using Scottxu.Blog.Captcha;

namespace Scottxu.Blog
{
    /// <summary>
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
            var appSettings = new Models.Util.AppSettingsUtil(Configuration);

            services.AddDbContext<BlogSystemContext>(appSettings.GetDbContextOptionsBuilder());

            services.AddAuthentication(options => {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(appSettings.GetCookieAuthenticationOptions());

            /*
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
            */

            services.AddCaptcha(Configuration);

            services.AddMvc();

            services.AddTransient<TemplateParsing>();

            services.Configure<ForwardedHeadersOptions>(Configuration.GetSection("ForwardedHeadersOptions"));

            services.Configure<SiteOptions>(Configuration.GetSection("SiteOptions"));
        }

        /// <summary>
        /// 配置HTTP请求管道
        /// </summary>
        /// <param name="app">被用于构建应用程序的请求管道 只可以在Startup中的Configure方法里使用</param>
        /// <param name="env">提供了访问应用程序属性，如环境变量</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseStaticFiles();

            app.UseAuthentication();

            //app.UseSession();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "home",
                    template: "{action=Index}",
                    defaults: new { controller = "Home" }
                );
                routes.MapRoute(
                    name: "goAccount_admin",
                    template: "Admin/GoAccount/{accountAction}",
                    defaults: new { controller = "Home", action = "GoAccount" }
                );
                routes.MapRoute(
                    name: "goAccount_account",
                    template: "Account/GoAccount/{accountAction}",
                    defaults: new { controller = "Home", action = "GoAccount" }
                );
                routes.MapRoute(
                    name: "account",
                    template: "Account/{action=Index}",
                    defaults: new { controller = "Account" }
                );
                routes.MapRoute(
                    name: "admin",
                    template: "Admin/{action=Index}",
                    defaults: new { controller = "Admin" }
                );
                routes.MapRoute(
                    name: "api",
                    template: "API/{action=Index}",
                    defaults: new { controller = "API" }
                );

                routes.MapRoute(
                    name: "editor",
                    template: "API/Editor/{action=Index}",
                    defaults: new { controller = "Editor" }
                );
                routes.MapRoute(
                    name: "setup",
                    template: "Admin/Setup/{action=Index}",
                    defaults: new { controller = "Setup" }
                );

                routes.MapRoute(
                    name: "file",
                    template: "File/{articleGuid}/{uploadedFileGuid}",
                    defaults: new { controller = "File", action = "Get" }
                );
                routes.MapRoute(
                    name: "login",
                    template: "Account/Login",
                    defaults: new { controller = "Auth", action = "Index" }
                );
                routes.MapRoute(
                    name: "login_postback",
                    template: "Account/Login/PostBack",
                    defaults: new { controller = "Auth", action = "Login_PostBack" }
                );
                routes.MapRoute(
                    name: "logout",
                    template: "Account/Logout",
                    defaults: new { controller = "Auth", action = "Logout" }
                );
            });

            app.UseTemplateParsing(); //博客模板解析中间件
        }
    }
}
