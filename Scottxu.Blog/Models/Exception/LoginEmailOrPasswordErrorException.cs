using System;

namespace Scottxu.Blog.Models.Exception
{
    public class LoginEmailOrPasswordErrorException : System.Exception
    {
        public LoginEmailOrPasswordErrorException(string message) : base(message)
        {
        }
    }
}