using Microsoft.AspNetCore.Mvc;

namespace Scottxu.Blog.Services.CaptchaService.ViewComponents
{
    [ViewComponent(Name = "CaptchaDiv")]
    public class CaptchaDivViewComponent : ViewComponent
    {
        ICaptcha Captcha { get; }

        public CaptchaDivViewComponent(ICaptcha captcha) => Captcha = captcha;

        public IViewComponentResult Invoke()
        {
            ViewContext.Writer.WriteLine(Captcha.GetDivString());
            return Content(string.Empty);
        }
    }
}