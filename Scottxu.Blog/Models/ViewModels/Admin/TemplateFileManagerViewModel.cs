using System;
using Scottxu.Blog.Models.ViewModels;
using Scottxu.Blog.Models.Entities;
using System.Collections.Generic;

namespace Scottxu.Blog.Models.ViewModels.Admin
{
    public class TemplateFileManagerViewModel
    {
        public PageInfoViewModel PageInfo { get; set; }
        public Template Template { get; set; }
        public List<TemplateFile> TemplateFiles { get; set; }
        public string SearchMessage { get; set; }
    }
}