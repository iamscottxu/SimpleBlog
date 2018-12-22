using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Scottxu.Blog.Services.CaptchaService;
using Scottxu.Blog.Models.Helpers;

namespace Scottxu.Blog.Services.CaptchaService
{
    public class GoogleReCaptchaV2 : ICaptcha
    {
        Options _options { get; }

        public string GetHeadString(string action)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<script src='https://www.recaptcha.net/recaptcha/api.js'></script>");
            stringBuilder.AppendLine("<script>");
            stringBuilder.AppendLine("    getCaptchaText = function(loginFun, data) {");
            stringBuilder.AppendLine("        loginFun(grecaptcha.getResponse(), data);");
            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("    resetCaptcha = function() { grecaptcha.reset(); }");
            stringBuilder.AppendLine("</script>");
            return stringBuilder.ToString();
        }

        public string GetDivString() => $"<div class='g-recaptcha' data-sitekey='{_options.SiteKey}'></div>";

        public string Validate(string captcha, string ipAddress)
        {
            if (string.IsNullOrEmpty(captcha)) return "验证码为空。";

            try
            {
                JObject jObject;
                using (var response = HttpRequestHelper.CreatePostResponse(
                    "https://www.recaptcha.net/recaptcha/api/siteverify", new Dictionary<string, string>
                    {
                        {"secret", _options.SecretKey},
                        {"response", captcha},
                        {"remoteip", ipAddress}
                    }))
                {
                    jObject = (JObject) HttpRequestHelper.GetObjectFromJsonResponse(response);
                }

                return jObject.GetValue("success").Value<bool>()
                    ? null
                    : "人机验证时发生错误：" + string.Join(",", jObject["error-codes"].ToList());
            }
            catch (Exception ex)
            {
                return "人机验证时发生错误：" + ex.Message;
            }
        }

        public GoogleReCaptchaV2(IOptions<Options> options)
        {
            _options = options.Value;
        }

        public class Options : ICaptchaOptions
        {
            public string SiteKey { get; set; }

            public string SecretKey { get; set; }
        }
    }
}