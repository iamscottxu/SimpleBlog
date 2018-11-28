using System;
namespace Scottxu.Blog.Models.Exception
{
    public class ValidateCaptchaErrorException : System.Exception
    {
        public ValidateCaptchaErrorException(string message) : base(message) { }
    }
}
