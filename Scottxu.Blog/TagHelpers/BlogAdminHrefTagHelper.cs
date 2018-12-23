using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;

namespace Scottxu.Blog.TagHelpers
{
    [HtmlTargetElement(Attributes = "blog-admin-href", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement(Attributes = "blog-admin-href")]
    public class BlogAdminHrefTagHelper : TagHelper
    {
        public string BlogAdminHref { get; set; }

        [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var options = (IOptions<SiteOptions>) ViewContext.ViewData["SiteOptions"];
            output.Attributes.RemoveAll("blog-admin-href");
            output.Attributes.SetAttribute("href",
                options.Value.GetAdminUrl(BlogAdminHref, ViewContext.HttpContext.Request.PathBase));
        }
    }
}