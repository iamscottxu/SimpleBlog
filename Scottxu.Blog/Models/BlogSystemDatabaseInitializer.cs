﻿using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Scottxu.Blog.Models.Entitys;
using Scottxu.Blog.Models.Helpers;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Units;

namespace Scottxu.Blog.Models
{
    public static class BlogSystemDatabaseInitializer
    {
        public struct Configs
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string BlogName { get; set; }
            public string UserName { get; set; }
        }

        internal static void Seed(HttpContext httpContext, BlogSystemContext dataBaseContext,
            IHostingEnvironment hostingEnvironment, Configs configs)
        {
            FormatVerificationHelper.FormatVerification(
                configs.Email, FormatVerificationHelper.FormatType.Email,
                new ParametersFormatErrorException("邮箱格式错误。"));
            FormatVerificationHelper.FormatVerification(
                configs.Password, FormatVerificationHelper.FormatType.Password,
                new ParametersFormatErrorException("密码格式错误。"));
            FormatVerificationHelper.FormatVerification(
                configs.BlogName, FormatVerificationHelper.FormatType.BlogName,
                new ParametersFormatErrorException("博客名称格式错误。"));
            FormatVerificationHelper.FormatVerification(
                configs.UserName, FormatVerificationHelper.FormatType.UserName,
                new ParametersFormatErrorException("昵称格式错误。"));
            dataBaseContext.Database.EnsureDeleted();
            dataBaseContext.Database.EnsureCreated();
            using (var transaction = dataBaseContext.Database.BeginTransaction())
            {
                Config(dataBaseContext, configs);
                TemplateFile(httpContext, dataBaseContext, hostingEnvironment);
                transaction.Commit();
            }
        }

        static void Config(BlogSystemContext dataBaseContext, Configs configs)
        {
            var configUnit = new ConfigUnit(dataBaseContext)
            {
                Email = configs.Email,
                Password = PasswordHelper.CreateDbPassword(configs.Password, false),
                BlogName = configs.BlogName,
                UserName = configs.UserName
            };
            configUnit.SaveAll();
        }

        static void TemplateFile(HttpContext httpContext, BlogSystemContext dataBaseContext,
            IHostingEnvironment hostingEnvironment)
        {
            var uploadUnit = new UploadUnit(dataBaseContext, hostingEnvironment);
            var formFileInfos = uploadUnit.SaveZipFiles(httpContext,
                File.OpenRead($"{hostingEnvironment.ContentRootPath}/default-template.zip"));
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