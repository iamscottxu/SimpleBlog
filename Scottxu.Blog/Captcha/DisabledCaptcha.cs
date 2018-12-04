using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Scottxu.Blog.Models.Helper;
using System.Linq;

namespace Scottxu.Blog.Captcha
{
    public class DisabledCaptcha : ICaptcha
    {
        public string GetHeadString(string action)
        {

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<script>");
            stringBuilder.AppendLine("    getCaptchaText = function(loginFun, data) {");
            stringBuilder.AppendLine("        loginFun('.', data);");
            stringBuilder.AppendLine("    }");
            stringBuilder.AppendLine("    resetCaptcha = function() { }");
            stringBuilder.AppendLine("</script>");
            return stringBuilder.ToString();
        }

        public string GetDivString() => string.Empty;

        public string Validate(string captcha, string ipAddress) => null;

        public DisabledCaptcha()
        {

        }
    }
}
