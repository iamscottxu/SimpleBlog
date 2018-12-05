using System;
using Scottxu.Blog.Models.ViewModel;
using Scottxu.Blog.Models.Entitys;
using System.Collections.Generic;

namespace Scottxu.Blog.Models.ViewModel.Admin
{
    public class TypeManagerViewModel
    {
        public PageInfoViewModel PageInfo { get; set; }
        public List<ArticleType> ArticleTypes { get; set; }
        public List<ArticleType> SelectedArticleTypes { get; set; }
        public string SearchMessage { get; set; }
    }
}