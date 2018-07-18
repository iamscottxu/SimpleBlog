using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Helper;
using Scottxu.Blog.Models.Util;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class AuthController : BaseController
    {
        public AuthController(BlogSystemContext context) : base(context) { }

        // GET: /Auth
        public IActionResult Index()
        {
            return View();
        }

        internal class LoginEmailOrPasswordErrorException : Exception
        {
            public LoginEmailOrPasswordErrorException(string message) : base(message) { }
        }

        // GET: /Auth/Login
        [ApiAction, HttpPost]
        public async Task Login(string email, string password)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password))
                throw new MissingParametersException("缺少参数。");
            FormatVerificationHelper.FormatVerification(
                email, FormatVerificationHelper.FormatType.Email, new ParametersFormatErrorException("邮箱格式错误。"));
            //FormatVerificationHelper.FormatVerification(
                //password, FormatVerificationHelper.FormatType.Password, new ParametersFormatErrorException("密码格式错误。"));
            ConfigHelper configHelper = new ConfigHelper(DataBaseContext);
            if (configHelper.Email == email && PasswordUtil.ComparePasswords(configHelper.Password, password))
            await UserLoginAsync(email);
            else throw new LoginEmailOrPasswordErrorException("邮箱或密码错误。");
        }

        // GET: /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await UserLogoutAsync();
            return RedirectToAction(string.Empty, new { ReturnUrl = Request.Headers["Referer"] });
        }
    }
}
