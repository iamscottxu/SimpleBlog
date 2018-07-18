﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Helper;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class SetupController : BaseController
    {
        public SetupController(BlogSystemContext context) : base(context) { }

        // GET: /Setup
        public IActionResult Index()
        {
            if (DataBaseContext.DataBaseIsExist) return RedirectToAction(string.Empty, "Admin");
            return View();
        }

        internal class DataBaseIsExistException : Exception
        {
            public DataBaseIsExistException(string message) : base(message) { }
        }

        // GET: /Setup/Install
        [ApiAction]
        [HttpPost]
        public async Task Install(String email, String password, String blogName, String userName) 
        {
            if (DataBaseContext.DataBaseIsExist) throw new DataBaseIsExistException("数据库已存在。");
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(blogName) || String.IsNullOrEmpty(userName))
                throw new MissingParametersException("缺少参数。");
            FormatVerificationHelper.FormatVerification(
                email, FormatVerificationHelper.FormatType.Email, new ParametersFormatErrorException("邮箱格式错误。"));
            FormatVerificationHelper.FormatVerification(
                password, FormatVerificationHelper.FormatType.Password, new ParametersFormatErrorException("密码格式错误。"));
            FormatVerificationHelper.FormatVerification(
                blogName, FormatVerificationHelper.FormatType.BlogName, new ParametersFormatErrorException("博客名称格式错误。"));
            FormatVerificationHelper.FormatVerification(
                userName, FormatVerificationHelper.FormatType.UserName, new ParametersFormatErrorException("昵称格式错误。"));
            DataBaseContext.Database.EnsureDeleted();
            DataBaseContext.Database.EnsureCreated();
            BlogSystemDatabaseInitializer.Seed(DataBaseContext, new BlogSystemDatabaseInitializer.Configs() {
                Email = email,
                Password = password,
                BlogName = blogName,
                UserName = userName
            });
            await UserLoginAsync(email);
        }
    }
}
