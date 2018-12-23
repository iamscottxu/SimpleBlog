using System.Text.RegularExpressions;

namespace Scottxu.Blog.Models.Helpers
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
            ArticleName,
            TemplateName
        }

        public static bool FormatVerification(string verifyingString, FormatType formatType,
            System.Exception formatErrorExceptionType = null)
        {
            var method = typeof(FormatVerificationHelper).GetMethod($"{formatType.ToString()}Verification");
            var pass = (bool) method.Invoke(null, new object[] {verifyingString});
            if (formatErrorExceptionType != null && !pass) throw formatErrorExceptionType;
            return pass;
        }

        public static bool EmailVerification(string verifyingString)
        {
            return (new Regex(
                    @"[\w!#$%&'*+/=?^_`{|}~-]+(?:\.[\w!#$%&'*+/=?^_`{|}~-]+)*@(?:[\w](?:[\w-]*[\w])?\.)+[\w](?:[\w-]*[\w])?")
                ).IsMatch(verifyingString);
        }

        public static bool PasswordVerification(string verifyingString)
        {
            return true;
        }

        public static bool BlogNameVerification(string verifyingString)
        {
            return (new Regex(@"^[\S ]{1,20}$")).IsMatch(verifyingString);
        }

        public static bool UserNameVerification(string verifyingString)
        {
            return (new Regex(@"^[\S ]{1,20}$")).IsMatch(verifyingString);
        }

        public static bool VirtualPathVerification(string verifyingString)
        {
            return (new Regex(@"(^(\/[^\\\/:*?""<>\|]+)+$)|(^\/$)")).IsMatch(verifyingString) &&
                   verifyingString.Length < 500;
        }

        public static bool ArticleLabelNameVerification(string verifyingString)
        {
            return (new Regex(@"^[\S ]{1,50}$")).IsMatch(verifyingString);
        }

        public static bool ArticleTypeNameVerification(string verifyingString)
        {
            return (new Regex(@"^[\S ]{1,50}$")).IsMatch(verifyingString);
        }

        public static bool ArticleNameVerification(string verifyingString)
        {
            return (new Regex(@"^[\S ]{1,50}$")).IsMatch(verifyingString);
        }
        
        public static bool TemplateNameVerification(string verifyingString)
        {
            return (new Regex(@"^[\S ]{1,100}$")).IsMatch(verifyingString);
        }
    }
}