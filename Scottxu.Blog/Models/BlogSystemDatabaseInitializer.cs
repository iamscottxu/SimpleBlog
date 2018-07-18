using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Scottxu.Blog.Models.Entitys;
using Scottxu.Blog.Models.Helper;

namespace Scottxu.Blog.Models
{
    public static class BlogSystemDatabaseInitializer
    {
        public struct Configs
        {
            public String Email { get; set; }
            public String Password { get; set; }
            public String BlogName { get; set; }
            public string UserName { get; set; }
        }

        internal static void Seed(HttpContext httpContext, BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment, Configs configs)
        {
            using (var transaction = dataBaseContext.Database.BeginTransaction())
            {
                Config(dataBaseContext, configs);
                TemplateFile(httpContext, dataBaseContext, hostingEnvironment);
                transaction.Commit();
            }
        }

        static void Config(BlogSystemContext dataBaseContext, Configs configs)
        {
            ConfigHelper configHelper = new ConfigHelper(dataBaseContext)
            {
                Email = configs.Email,
                Password = Util.PasswordUtil.CreateDbPassword(configs.Password, false),
                BlogName = configs.BlogName,
                UserName = configs.UserName
            };
            configHelper.SaveAll();
        }

        static void TemplateFile(HttpContext httpContext, BlogSystemContext dataBaseContext, IHostingEnvironment hostingEnvironment)
        {
            var uploadHelper = new UploadHelper(dataBaseContext, hostingEnvironment);
            var formFileInfos = uploadHelper.SaveZipFiles(httpContext, File.OpenRead($"{hostingEnvironment.ContentRootPath}/default-template.zip"));
            dataBaseContext.TemplateFiles.AddRange(formFileInfos.Select(formFile => new TemplateFile()
            {
                MIME = formFile.MIME,
                Name = formFile.FileName,
                UploadedFile = formFile.UploadedFile,
                VirtualPath = formFile.VirtualPath
            }));
            dataBaseContext.SaveChanges();
        }
    }
}
