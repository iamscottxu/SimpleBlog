using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;
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