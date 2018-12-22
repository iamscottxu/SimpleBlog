using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Helpers;
using Scottxu.Blog.Models.Units;
using System.Net;
using Scottxu.Blog.Services.CaptchaService;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class AuthController : BaseController
    {
        ICaptcha Captcha { get; }

        public AuthController(BlogSystemContext context, IOptions<SiteOptions> options, ICaptcha captcha) :
            base(context, options) => Captcha = captcha;

        // GET: /Account/Login
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Account/Login/PostBack
        [ApiAction, HttpPost]
        public async Task Login_PostBack(string email, string password, string captchaText)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                throw new MissingParametersException("缺少参数。");
            var result = Captcha.Validate(captchaText, IPAddressHelper.GetRemoteIpAddress(HttpContext));
            if (!string.IsNullOrEmpty(result)) throw new ValidateCaptchaErrorException(result);
            FormatVerificationHelper.FormatVerification(
                email, FormatVerificationHelper.FormatType.Email, new ParametersFormatErrorException("电子邮箱地址格式错误。"));
            FormatVerificationHelper.FormatVerification(
                password, FormatVerificationHelper.FormatType.Password, new ParametersFormatErrorException("密码格式错误。"));
            var configUnit = new ConfigUnit(DataBaseContext);
            if (configUnit.Email == email && PasswordHelper.ComparePasswords(configUnit.Password, password))
                await UserLoginAsync(email);
            else throw new LoginEmailOrPasswordErrorException("电子邮箱地址或密码错误。");
        }

        // GET: /Account/Logout
        public async Task<IActionResult> Logout(string ReturnUrl)
        {
            await UserLogoutAsync();

            return Redirect(
                $"./Login?ReturnUrl={(string.IsNullOrEmpty(ReturnUrl) ? WebUtility.UrlEncode(Request.Path) : ReturnUrl)}");
        }
    }
}