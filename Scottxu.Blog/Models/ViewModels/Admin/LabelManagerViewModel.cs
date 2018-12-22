using System;
using Scottxu.Blog.Models.ViewModels;
using Scottxu.Blog.Models.Entities;
using System.Collections.Generic;

namespace Scottxu.Blog.Models.ViewModels.Admin
{
    public class LabelManagerViewModel
    {
        public PageInfoViewModel PageInfo { get; set; }
        public List<ArticleLabel> ArticleLabels { get; set; }
        public string SearchMessage { get; set; }
    }
}