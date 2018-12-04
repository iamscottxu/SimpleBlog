using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Entitys;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Web;
using System.Text;
using Scottxu.Blog.Models.Helper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Scottxu.Blog.TemplateParsingMiddleware
{
    /// <summary>
    /// 模板解析中间件
    /// </summary>
    public class TemplateParsing
    {
        readonly RequestDelegate _next;

        public TemplateParsing(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, BlogSystemContext dataBaseContext, IOptions<SiteOptions> options, IHostingEnvironment hostingEnvironment)
        {
            string requestUrl = HttpUtility.UrlDecode(context.Request.Path);
            //如果路径不是以/blog就跳过
            if (!requestUrl.ToLower().StartsWith("/blog", StringComparison.Ordinal))
            {
                await _next(context);
                return;
            }
            if (!dataBaseContext.DataBaseIsExist)
            {
                context.Response.Redirect(string.IsNullOrEmpty(options.Value.AdminUrl) ? "~/Admin/Setup" : $"{options.Value.AdminUrl}/Setup");
                return;
            }
            requestUrl = requestUrl.Remove(0, 5);
            if (string.IsNullOrEmpty(requestUrl)) requestUrl = "/";

            var templateFile = GetUploadedFile(dataBaseContext, requestUrl);

            if (templateFile == null)
            {
                await _next(context);
                return;
            }

            var uploadHelper = new UploadHelper(dataBaseContext, hostingEnvironment);
            var fileInfo = uploadHelper.GetFileInfo(templateFile.UploadedFile);
            if (!fileInfo.Exists)
            {
                context.Response.StatusCode = 404;
                return;
            }
            //向Cookie添加站点设置信息
            context.Response.Cookies.Append("SimpleBlog_Settings", JsonConvert.SerializeObject(new
            {
                adminUrl = options.Value.GetAdminUrl(string.Empty, context.Request.PathBase),
                apiUrl = options.Value.GetApiUrl(string.Empty, context.Request.PathBase)
            }), new CookieOptions()
            {
                HttpOnly = false,
                Path = string.IsNullOrEmpty(options.Value.HomeUrl) ? options.Value.GetHomeUrl(string.Empty, context.Request.PathBase) : "/"
            });

            context.Response.ContentType = templateFile.MIME;
            context.Response.Headers["Content-Disposition"] =
                $"{(context.Request.Query["download"].Any() ? "attachment;" : string.Empty)}filename*={Encoding.Default.BodyName}''{HttpUtility.UrlEncode(templateFile.Name)}";
            context.Response.Headers["Content-Length"] = fileInfo.Length.ToString();
            await context.Response.SendFileAsync(fileInfo.FullName);
        }

        TemplateFile GetUploadedFile(BlogSystemContext dataBaseContext, string requestUrl)
        {
            if (requestUrl == "/")
            {
                var templateFile = dataBaseContext.TemplateFiles.Include(o => o.UploadedFile)
                                  .Where(p => p.VirtualPath == "/")
                                  .FirstOrDefault();
                if (templateFile == null) templateFile = dataBaseContext.TemplateFiles.Include(o => o.UploadedFile)
                                  .Where(p => p.VirtualPath == "/index.html")
                                  .FirstOrDefault();
                if (templateFile == null) templateFile = dataBaseContext.TemplateFiles.Include(o => o.UploadedFile)
                                  .Where(p => p.VirtualPath == "/index.htm")
                                  .FirstOrDefault();
                return templateFile;
            }
            return dataBaseContext.TemplateFiles.Include(o => o.UploadedFile)
                                  .Where(p => p.VirtualPath == requestUrl)
                                  .FirstOrDefault();
        }
    }
}
