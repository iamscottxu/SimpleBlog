using System;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;

namespace Scottxu.Blog.TagHelpers
{
    [HtmlTargetElement("a", TagStructure = TagStructure.WithoutEndTag)]
    public class ATagHelper : TagHelper
    {
        SiteOptions Options { get; }

        public string BlogHref { get; set; }

        public ATagHelper(IOptions<SiteOptions> options) => Options = Options;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("a", string.IsNullOrEmpty(Options.CDNDomain) ? $"~/sys/cdn/{BlogHref}" : $"//{Options.CDNDomain}/{BlogHref}");
        }
    }
}
