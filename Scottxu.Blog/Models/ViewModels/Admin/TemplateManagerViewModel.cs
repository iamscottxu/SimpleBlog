using System;
using Scottxu.Blog.Models.Entities;
using System.Collections.Generic;

namespace Scottxu.Blog.Models.ViewModels.Admin
{
    public class TemplateManagerViewModel
    {
        public PageInfoViewModel PageInfo { get; set; }
        public List<Template> Templates { get; set; }
        public string SearchMessage { get; set; }
        
        public Guid? EnableTemplateGuid { get; set; }
    }
}