using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.ViewModel.Home;

namespace Scottxu.Blog.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(BlogSystemContext context) : base(context) { }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet("/sys/Home/Error/{statusCode?}")]
        public IActionResult Error(int? statusCode)
        {
            return View(new ErrorViewModel { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorCode = statusCode
            });
        }
    }
}
