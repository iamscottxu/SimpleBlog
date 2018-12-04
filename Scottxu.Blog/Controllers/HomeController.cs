using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.ViewModel.Home;
using System.Linq;

namespace Scottxu.Blog.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(BlogSystemContext context, IOptions<SiteOptions> options) : base(context, options) { }

        public IActionResult Index()
        {
            return Redirect(Options.GetHomeUrl(string.Empty, Request.PathBase.Value));
        }

        [HttpGet("Error/{statusCode?}")]
        public IActionResult Error(int? statusCode)
        {
            return View(new ErrorViewModel { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorCode = statusCode
            });
        }

        [HttpGet("GoAccount/{accountAction}")]
        [HttpGet("Admin/GoAccount/{accountAction}")]
        [HttpGet("Account/GoAccount/{accountAction}")]
        public IActionResult GoAccount(string accountAction, string ReturnUrl)
        {
            if (!string.IsNullOrEmpty(ReturnUrl)){
                var path = Request.Path.Value;
                var pathLength = ($"/GoAccount/{accountAction}").Length;
                var rootPath = path.Remove(path.Length - 1 - pathLength, pathLength);
                ReturnUrl = ReturnUrl.Remove(0, rootPath.Length);
                ReturnUrl = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}{ReturnUrl}";
            }
            var accountUrl = Options.AccountUrl;
            if (string.IsNullOrEmpty(accountUrl)) accountUrl = "/Account";
            return Redirect($"{accountUrl}/{accountAction}?ReturnUrl={WebUtility.UrlEncode(string.IsNullOrEmpty(ReturnUrl) ? Request.Headers["Referer"].First() : ReturnUrl)}");
        }
    }
}
