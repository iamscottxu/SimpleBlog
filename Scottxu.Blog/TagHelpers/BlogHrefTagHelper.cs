using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;

namespace Scottxu.Blog.TagHelpers
{
    [HtmlTargetElement(Attributes = "blog-src")]
    public class BlogSrcTagHelper : TagHelper
    {
        SiteOptions Options { get; }

        public string BlogSrc { get; set; }

        public BlogSrcTagHelper(IOptions<SiteOptions> options) => Options = Options;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll("blog-src");
            output.Attributes.SetAttribute("src", string.IsNullOrEmpty(Options.CDNDomain) ? $"~/sys/cdn/{BlogSrc}" : $"//{Options.CDNDomain}/{BlogSrc}");
        }
    }
}
