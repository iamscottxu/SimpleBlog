using System;
using Scottxu.Blog.Models.ViewModel;
using Scottxu.Blog.Models.Entitys;
using System.Collections.Generic;

namespace Scottxu.Blog.Models.ViewModel.Admin
{
    public class ArticleManagerViewModel
    {
        public PageInfoViewModel PageInfo { get; set; }
        public ArticleType ArticleType { get; set; }
        public List<Article> Articles { get; set; }
        public List<ArticleType> SelectedArticleTypes { get; set; }
        public List<ArticleLabel> SelectedArticleLabels { get; set; }
        public string SearchMessage { get; set; }
    }
}
