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
using Scottxu.Blog.Models.Exception;
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


            //如果没有设置模板直接返回404
            Guid? templateGuid;
            try
            {
                if ((templateGuid = new ConfigUnit(dataBaseContext, cache).TemplateGuid) == null)
                {
                    context.Response.StatusCode = 404;
                    return;
                }
            }
            catch (NotDataBaseException)
            {
                //数据库不存在，跳转到初始化页面
                context.Response.Redirect(options.Value.GetAdminUrl("Setup", context.Request.PathBase));
                return;
            }

            requestUrl = requestUrl.Remove(0, 5);
            if (string.IsNullOrEmpty(requestUrl)) requestUrl = "/";

            var templateFile =
                GetUploadedFile(GetUploadedFiles(dataBaseContext, cache, templateGuid.Value), requestUrl);

            if (templateFile == null)
            {
                await _next(context);
                return;
            }

            var uploadUnit = new UploadUnit(dataBaseContext, hostingEnvironment);
            var enableGzip = false;
            var acceptEncoding = context.Request.Headers["Accept-Encoding"].FirstOrDefault();
            if (acceptEncoding != null && acceptEncoding.Split(',').Any(q => q == "gzip")) enableGzip = true;

            var fileInfo = uploadUnit.GetFileInfo(templateFile.UploadedFile, enableGzip);
            if (fileInfo == null || !fileInfo.Exists)
            {
                enableGzip = !enableGzip;
                fileInfo = uploadUnit.GetFileInfo(templateFile.UploadedFile, enableGzip);
                if (fileInfo == null || !fileInfo.Exists)
                {
                    context.Response.StatusCode = 404;
                    return;
                }
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

            if (enableGzip) context.Response.Headers.Add("Content-Encoding", "gzip");
            context.Response.Headers.Add("Content-Disposition",
                $"{(context.Request.Query["download"].Any() ? "attachment;" : string.Empty)}filename*={Encoding.Default.BodyName}''{HttpUtility.UrlEncode(templateFile.Name)}");

            await context.Response.SendFileAsync(fileInfo.FullName);
        }

        /// <summary>
        /// 获取上传文件。
        /// </summary>
        /// <param name="templateFiles">上传文件的列表</param>
        /// <param name="requestUrl">请求的URL</param>
        /// <returns>上传文件</returns>
        TemplateFile GetUploadedFile(List<TemplateFile> templateFiles, string requestUrl)
        {
            //依次查找“/”，“/index.html”,“index.htm”
            if (requestUrl != "/")
                return templateFiles.FirstOrDefault(p => p.VirtualPath == requestUrl);
            {
                var templateFile = templateFiles.FirstOrDefault(p => p.VirtualPath == "/" ||
                                                                     p.VirtualPath == "/index.html" ||
                                                                     p.VirtualPath == "/index.htm");
                return templateFile;
            }
        }

        /// <summary>
        /// 获取上传文件的列表。
        /// </summary>
        /// <param name="dataBaseContext">数据库上下文</param>
        /// <param name="cache">Redis缓存对象</param>
        /// <param name="templateGuid">模板Guid</param>
        /// <returns>上传文件的列表</returns>
        /// <exception cref="NotDataBaseException">数据库不存在引发的异常</exception>
        List<TemplateFile> GetUploadedFiles(BlogSystemContext dataBaseContext, IDistributedCache cache,
            Guid templateGuid)
        {
            var templateFilesJson = cache.GetString("TemplateFiles");
            List<TemplateFile> templateFiles;
            //如果缓存数据存在，则直接从缓存获取数据，否则从数据库获取数据并把数据保存到缓存
            if (string.IsNullOrEmpty(templateFilesJson))
            {
                //从数据库获取数据
                templateFiles = dataBaseContext.TemplateFiles
                    .Include(o => o.UploadedFile)
                    .Where(q => q.TemplateGuid == templateGuid)
                    .ToList();
                //将数据保存到缓存
                templateFilesJson = JsonConvert.SerializeObject(templateFiles.Select(q => new
                {
                    q.Name,
                    q.VirtualPath,
                    q.MIME,
                    UploadedFile = new
                    {
                        q.UploadedFile.FileName,
                        q.UploadedFile.GzipFileName,
                        q.UploadedFile.Size
                    }
                }));
                cache.SetStringAsync("TemplateFiles", templateFilesJson);
            }
            else templateFiles = JsonConvert.DeserializeObject<List<TemplateFile>>(templateFilesJson); //从缓存获取数据

            return templateFiles;
        }
    }
}