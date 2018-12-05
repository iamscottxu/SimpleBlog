using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;

namespace Scottxu.Blog.TagHelpers
{
    [HtmlTargetElement(Attributes = "blog-src", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement(Attributes = "blog-src")]
    public class BlogSrcTagHelper : TagHelper
    {
        public string BlogSrc { get; set; }

        [ViewContext] [HtmlAttributeNotBound] public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var options = (IOptions<SiteOptions>) ViewContext.ViewData["SiteOptions"];
            output.Attributes.RemoveAll("blog-src");
            output.Attributes.SetAttribute("src",
                options.Value.GetCDNUrl(BlogSrc, ViewContext.HttpContext.Request.PathBase));
        }
    }
}