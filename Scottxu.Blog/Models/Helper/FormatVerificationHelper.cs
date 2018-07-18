using System;
using System.Text.RegularExpressions;

namespace Scottxu.Blog.Models.Helper
{
    public static class FormatVerificationHelper
    {
        public enum FormatType
        {
            Email,
            Password,
            BlogName,
            UserName,
            VirtualPath,
            ArticleLabelName,
            ArticleTypeName,
            ArticleName
        }

        public static bool FormatVerification(string verifyingString, FormatType formatType, Exception formatErrorExceptionType = null)
        {
            var method = typeof(FormatVerificationHelper).GetMethod($"{formatType.ToString()}Verification");
            var pass = (bool)method.Invoke(null, new[] {verifyingString});
            if (formatErrorExceptionType != null && !pass) throw formatErrorExceptionType;
            return pass;
        }

        public static bool EmailVerification(string VerifyingString) 
        {
            return (new Regex(@"[\w!#$%&'*+/=?^_`{|}~-]+(?:\.[\w!#$%&'*+/=?^_`{|}~-]+)*@(?:[\w](?:[\w-]*[\w])?\.)+[\w](?:[\w-]*[\w])?")).IsMatch(VerifyingString);
        }

        public static bool PasswordVerification(string VerifyingString)
        {
            return true;
        }

        public static bool BlogNameVerification(string VerifyingString)
        {
            return (new Regex(@"^[\S ]{1,20}$")).IsMatch(VerifyingString); 
        }

        public static bool UserNameVerification(string VerifyingString)
        {
            return (new Regex(@"^[\S ]{1,20}$")).IsMatch(VerifyingString);
        }

        public static bool VirtualPathVerification(string VerifyingString)
        {
            return (new Regex(@"(^(\/[^\\\/:*?""<>\|]+)+$)|(^\/$)")).IsMatch(VerifyingString) && VerifyingString.Length < 500;
        }

        public static bool ArticleLabelNameVerification(string VerifyingString)
        {
            return (new Regex(@"^[\S ]{1,50}$")).IsMatch(VerifyingString);
        }

        public static bool ArticleTypeNameVerification(string VerifyingString)
        {
            return (new Regex(@"^[\S ]{1,50}$")).IsMatch(VerifyingString);
        }

        public static bool ArticleNameVerification(string VerifyingString)
        {
            return (new Regex(@"^[\S ]{1,50}$")).IsMatch(VerifyingString);
        }
    }
}
