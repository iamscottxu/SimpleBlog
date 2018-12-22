using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Scottxu.Blog.Services.CaptchaService;

namespace Scottxu.Blog.Services.CaptchaService.ViewComponents
{
    [ViewComponent(Name = "CaptchaHead")]
    public class CaptchaHeadViewComponent : ViewComponent
    {
        ICaptcha Captcha { get; }

        public CaptchaHeadViewComponent(ICaptcha captcha) => Captcha = captcha;

        public IViewComponentResult Invoke(string action)
        {
            ViewContext.Writer.WriteLine(Captcha.GetHeadString(action));
            return Content(string.Empty);
        }
    }
}