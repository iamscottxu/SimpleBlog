using System.Text;

namespace Scottxu.Blog.Services.CaptchaService
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