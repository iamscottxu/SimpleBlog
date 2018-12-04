using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;

namespace Scottxu.Blog.TagHelpers
{
    [HtmlTargetElement(Attributes = "blog-admin-action")]
    public class BlogAdminActionTagHelper : TagHelper
    {
        public string BlogAdminAction { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var options = (IOptions<SiteOptions>)ViewContext.ViewData["SiteOptions"];
            output.Attributes.RemoveAll("blog-admin-action");
            output.Attributes.SetAttribute("action", options.Value.GetAdminUrl(BlogAdminAction, ViewContext.HttpContext.Request.PathBase));
        }
    }
}
