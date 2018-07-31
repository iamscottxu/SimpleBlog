using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Scottxu.Blog.Models;
using Scottxu.Blog.Models.Exception;
using Scottxu.Blog.Models.Helper;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    public class SetupController : BaseController
    {
        IHostingEnvironment HostingEnvironment { get; }
        public SetupController(BlogSystemContext context, IHostingEnvironment hostingEnvironment) : base(context) => HostingEnvironment = hostingEnvironment;

        // GET: /Setup
        public IActionResult Index()
        {
            if (DataBaseContext.DataBaseIsExist) return RedirectToAction(string.Empty, "Admin");
            return View();
        }

        internal class DataBaseIsExistException : Exception
        {
            public DataBaseIsExistException(string message) : base(message) { }
        }

        // GET: /Setup/Install
        [ApiAction]
        [HttpPost]
        public async Task Install(String email, String password, String blogName, String userName) 
        {
            if (DataBaseContext.DataBaseIsExist) throw new DataBaseIsExistException("数据库已存在。");
            if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(password) || String.IsNullOrEmpty(blogName) || String.IsNullOrEmpty(userName))
                throw new MissingParametersException("缺少参数。");
            BlogSystemDatabaseInitializer.Seed(HttpContext, DataBaseContext, HostingEnvironment, new BlogSystemDatabaseInitializer.Configs() {
                Email = email,
                Password = password,
                BlogName = blogName,
                UserName = userName
            });
            await UserLoginAsync(email);
        }
    }
}
