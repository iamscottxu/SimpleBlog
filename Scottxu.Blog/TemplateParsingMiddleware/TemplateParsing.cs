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

        public async Task InvokeAsync(HttpContext context, BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment)
        {
            string requestUrl = HttpUtility.UrlDecode(context.Request.Path);
            if (requestUrl.ToLower().StartsWith("/sys", StringComparison.Ordinal)) {
                await _next(context);
                return;
            }
            if (!dataBaseContext.DataBaseIsExist)
            {
                context.Response.Redirect("/sys/Setup");
                return;
            }

            var templateFile = getUploadedFile(dataBaseContext, requestUrl);
                
            if (templateFile == null) {
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
            context.Response.ContentType = templateFile.MIME;
            context.Response.Headers["Content-Disposition"] = 
                $"{(context.Request.Query["download"].Count() > 0 ? "attachment;": string.Empty)}filename*={Encoding.Default.BodyName}''{HttpUtility.UrlEncode(templateFile.Name)}";
            context.Response.Headers["Content-Length"] = fileInfo.Length.ToString();
            await context.Response.SendFileAsync(fileInfo.FullName);
        }

        TemplateFile getUploadedFile(BlogSystemContext dataBaseContext, string requestUrl){
            if (requestUrl == "/") {
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
