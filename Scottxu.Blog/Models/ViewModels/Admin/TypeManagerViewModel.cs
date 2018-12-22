using System;
using Scottxu.Blog.Models.ViewModels;
using Scottxu.Blog.Models.Entities;
using System.Collections.Generic;

namespace Scottxu.Blog.Models.ViewModels.Admin
{
    public class TypeManagerViewModel
    {
        public PageInfoViewModel PageInfo { get; set; }
        public List<ArticleType> ArticleTypes { get; set; }
        public List<ArticleType> SelectedArticleTypes { get; set; }
        public string SearchMessage { get; set; }
    }
}