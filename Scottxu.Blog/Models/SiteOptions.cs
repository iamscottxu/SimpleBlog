using System;
namespace Scottxu.Blog.Models
{
    public class SiteOptions
    {
        public string AccountUrl { get; set; }
        public string CDNUrl { get; set; }
        public string AdminUrl { get; set; }
        public string HomeUrl { get; set; }
        public string ApiUrl { get; set; }

        public string GetCDNUrl(string url, string pathBase) => string.IsNullOrEmpty(CDNUrl) ?
                  $"{pathBase}/cdn/{url}" :
                  $"{CDNUrl}/{url}";

        public string GetAdminUrl(string url, string pathBase) => string.IsNullOrEmpty(AdminUrl) ?
                  $"{pathBase}/Admin/{url}" :
                  $"{AdminUrl}/{url}";

        public string GetApiUrl(string url, string pathBase) => string.IsNullOrEmpty(ApiUrl) ?
                  $"{pathBase}/Api/{url}" :
                  $"{ApiUrl}/{url}";

        public string GetHomeUrl(string url, string pathBase) => string.IsNullOrEmpty(HomeUrl) ?
                 $"{pathBase}/blog/{url}" :
                 $"{HomeUrl}/{url}";
    }
}
