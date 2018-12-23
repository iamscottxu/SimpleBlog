using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Exception;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class SetupController : BaseController
    {
        IHostingEnvironment HostingEnvironment { get; }

        IDistributedCache Cache { get; }

        public SetupController(BlogSystemContext context, IOptions<SiteOptions> options,
            IHostingEnvironment hostingEnvironment, IDistributedCache cache)
            : base(context, options) => (HostingEnvironment, Cache) = (hostingEnvironment, cache);

        // GET: /Setup
        public IActionResult Index()
        {
            if (DataBaseContext.DataBaseIsExist) return Redirect(Options.GetHomeUrl(string.Empty, Request.PathBase));
            return View();
        }

        internal class DataBaseIsExistException : Exception
        {
            public DataBaseIsExistException(string message) : base(message)
            {
            }
        }

        // GET: /Setup/Install
        [ApiAction]
        [HttpPost]
        public async Task Install(string email, string password, string blogName, string userName)
        {
            if (DataBaseContext.DataBaseIsExist) throw new DataBaseIsExistException("数据库已存在。");
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(blogName) ||
                string.IsNullOrEmpty(userName))
                throw new MissingParametersException("缺少参数。");
            BlogSystemDatabaseInitializer.Seed(DataBaseContext, Cache, HostingEnvironment,
                new BlogSystemDatabaseInitializer.Configs()
                {
                    Email = email,
                    Password = password,
                    BlogName = blogName,
                    UserName = userName
                });
            await UserLoginAsync(email);
        }
    }
}