using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Entities;
using Scottxu.Blog.Models.Units;

namespace Scottxu.Blog.Middlewares.TemplateParsingMiddleware
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

        public async Task InvokeAsync(HttpContext context, BlogSystemContext dataBaseContext,
            IOptions<SiteOptions> options, IHostingEnvironment hostingEnvironment, IDistributedCache cache)
        {
            var requestUrl = HttpUtility.UrlDecode(context.Request.Path);
            //如果路径不是以/blog就跳过
            if (!requestUrl.ToLower().StartsWith("/blog", StringComparison.Ordinal))
            {
                await _next(context);
                return;
            }

            requestUrl = requestUrl.Remove(0, 5);
            if (string.IsNullOrEmpty(requestUrl)) requestUrl = "/";

            TemplateFile templateFile;
            try
            {
                templateFile = GetUploadedFile(GetUploadedFiles(dataBaseContext, cache), requestUrl);
            }
            catch (NotDataBaseException)
            {
                context.Response.Redirect(string.IsNullOrEmpty(options.Value.AdminUrl)
                    ? "~/Admin/Setup"
                    : $"{options.Value.AdminUrl}/Setup");
                return;
            }

            if (templateFile == null)
            {
                await _next(context);
                return;
            }

            var uploadUnit = new UploadUnit(dataBaseContext, hostingEnvironment);
            var fileInfo = uploadUnit.GetFileInfo(templateFile.UploadedFile);
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
                Path = string.IsNullOrEmpty(options.Value.HomeUrl)
                    ? options.Value.GetHomeUrl(string.Empty, context.Request.PathBase)
                    : "/"
            });

            context.Response.ContentType = templateFile.MIME;
            context.Response.ContentLength = fileInfo.Length;
            context.Response.Headers.Add("Content-Encoding", "gzip");
            context.Response.Headers.Add("Content-Disposition",
                $"{(context.Request.Query["download"].Any() ? "attachment;" : string.Empty)}filename*={Encoding.Default.BodyName}''{HttpUtility.UrlEncode(templateFile.Name)}");

            await context.Response.SendFileAsync(fileInfo.FullName);
        }

        TemplateFile GetUploadedFile(List<TemplateFile> templateFiles, string requestUrl)
        {
            if (requestUrl != "/")
                return templateFiles.FirstOrDefault(p => p.VirtualPath == requestUrl);
            {
                var templateFile = (templateFiles.FirstOrDefault(p => p.VirtualPath == "/") ??
                                    templateFiles.FirstOrDefault(p => p.VirtualPath == "/index.html")) ??
                                   templateFiles.FirstOrDefault(p => p.VirtualPath == "/index.htm");
                return templateFile;
            }
        }

        List<TemplateFile> GetUploadedFiles(BlogSystemContext dataBaseContext, IDistributedCache cache)
        {
            var templateFilesJson = cache.GetString("TemplateFiles");
            List<TemplateFile> templateFiles;
            if (string.IsNullOrEmpty(templateFilesJson))
            {
                if (!dataBaseContext.DataBaseIsExist) throw new NotDataBaseException();
                var configUnit = new ConfigUnit(dataBaseContext);
                templateFiles = dataBaseContext.TemplateFiles
                    .Include(o => o.UploadedFile)
                    .Where(q => q.TemplateGuid == Guid.Parse(configUnit.TemplateGuid))
                    .ToList();
                templateFilesJson = JsonConvert.SerializeObject(templateFiles.Select(q => new
                {
                    q.Name,
                    q.VirtualPath,
                    q.MIME,
                    UploadedFile = new {q.UploadedFile.FileName}
                }));
                cache.SetStringAsync("TemplateFiles", templateFilesJson);
            }
            else templateFiles = JsonConvert.DeserializeObject<List<TemplateFile>>(templateFilesJson);

            return templateFiles;
        }

        class NotDataBaseException : Exception
        {
        }
    }
}