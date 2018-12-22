namespace Scottxu.Blog.Services.CaptchaService
{
    public interface ICaptcha
    {
        string GetHeadString(string action);

        string GetDivString();

        string Validate(string captcha, string ipAddress);
    }
}