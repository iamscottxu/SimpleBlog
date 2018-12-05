using System;
using Microsoft.Extensions.Options;

namespace Scottxu.Blog.Captcha
{
    public interface ICaptcha
    {
        string GetHeadString(string action);

        string GetDivString();

        string Validate(string captcha, string ipAddress);
    }
}