using System;
using Scottxu.Blog.Models.ViewModel;
using Scottxu.Blog.Models.Entitys;
using System.Collections.Generic;

namespace Scottxu.Blog.Models.ViewModel.Admin
{
    public class LabelManagerViewModel
    {
        public PageInfoViewModel PageInfo { get; set; }
        public List<ArticleLabel> ArticleLabels { get; set; }
        public string SearchMessage { get; set; }
    }
}
