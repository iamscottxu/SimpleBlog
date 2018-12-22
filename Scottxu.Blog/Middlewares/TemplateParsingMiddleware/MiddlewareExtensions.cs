using Microsoft.AspNetCore.Builder;

namespace Scottxu.Blog.Middlewares.TemplateParsingMiddleware
{
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// 使用模板解析中间件。
        /// </summary>
        /// <returns>IApplicationBuilder</returns>
        /// <param name="builder">Builder</param>
        public static IApplicationBuilder UseTemplateParsing(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TemplateParsing>();
        }
    }
}