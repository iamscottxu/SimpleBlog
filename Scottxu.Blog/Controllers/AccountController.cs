using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Helpers;
using Scottxu.Blog.Models.Units;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        public AccountController(BlogSystemContext context, IOptions<SiteOptions> options) : base(context, options)
        {
        }

        // GET: /Account
        public IActionResult Index()
        {
            return View();
        }
    }
}