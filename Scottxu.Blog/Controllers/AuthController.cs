using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Helper;
using Scottxu.Blog.Models.Util;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class AuthController : BaseController
    {
        Captcha.ICaptcha Captcha { get; }
        public AuthController(BlogSystemContext context, Captcha.ICaptcha captcha) :
            base(context) => Captcha = captcha;

        // GET: /Auth
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Auth/Login
        [ApiAction, HttpPost]
        public async Task Login(string email, string password, string captchaText)
        {
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password))
                throw new MissingParametersException("缺少参数。");
            var result = Captcha.Validate(captchaText, IPAddressHelper.GetRemoteIpAddress(HttpContext));
            if (!string.IsNullOrEmpty(result)) throw new ValidateCaptchaErrorException(result);
            FormatVerificationHelper.FormatVerification(
                email, FormatVerificationHelper.FormatType.Email, new ParametersFormatErrorException("电子邮箱地址格式错误。"));
            FormatVerificationHelper.FormatVerification(
                password, FormatVerificationHelper.FormatType.Password, new ParametersFormatErrorException("密码格式错误。"));
            ConfigHelper configHelper = new ConfigHelper(DataBaseContext);
            if (configHelper.Email == email && PasswordUtil.ComparePasswords(configHelper.Password, password))
            await UserLoginAsync(email);
            else throw new LoginEmailOrPasswordErrorException("电子邮箱地址或密码错误。");
        }

        // GET: /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await UserLogoutAsync();
            return RedirectToAction(string.Empty, new { ReturnUrl = Request.Headers["Referer"] });
        }
    }
}
