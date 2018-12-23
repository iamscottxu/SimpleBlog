using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scottxu.Blog.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scottxu.Blog.Controllers
{
    [Authorize]
    public class EditorController : BaseController
    {
        IHostingEnvironment HostingEnvironment { get; }

        public EditorController(BlogSystemContext context, IOptions<SiteOptions> options,
            IHostingEnvironment hostingEnvironment)
            : base(context, options) => HostingEnvironment = hostingEnvironment;

        // GET: /Editor/Config
        [HttpGet]
        public object Config()
        {
            return null;
        }

        // POST: /Editor/UploadImage
        public object UploadImage()
        {
            return null;
        }

        // POST: /Editor/UploadScrawl
        public object UploadScrawl()
        {
            return null;
        }

        // POST: /Editor/UploadScrawl
        public object UploadVideo()
        {
            return null;
        }

        // POST: /Editor/UploadFile
        public object UploadFile()
        {
            return null;
        }

        // POST: /Editor/CatchImage
        public object CatchImage()
        {
            return null;
        }

        // POST: /Editor/ListImage
        public object ListImage()
        {
            return null;
        }

        // POST: /Editor/ListFile
        public object ListFile()
        {
            return null;
        }
    }
}