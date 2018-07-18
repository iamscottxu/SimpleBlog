using System;
using Scottxu.Blog.Models.ViewModel;
using Scottxu.Blog.Models.Entitys;
using System.Collections.Generic;

namespace Scottxu.Blog.Models.ViewModel.Admin
{
    public class TemplateManagerViewModel
    {
        public PageInfoViewModel PageInfo { get; set; }
        public List<TemplateFile> TemplateFiles { get; set; }
        public string SearchMessage { get; set; }
    }
}
