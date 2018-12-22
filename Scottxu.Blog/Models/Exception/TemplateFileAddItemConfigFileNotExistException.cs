using System;

namespace Scottxu.Blog.Models.Exception
{
    public class TemplateFileAddItemConfigFileNotExistException : System.Exception
    {
        public TemplateFileAddItemConfigFileNotExistException(string message) : base(message)
        {
        }
    }
}