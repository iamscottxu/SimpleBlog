using System;
using Microsoft.AspNetCore.Http;

namespace Scottxu.Blog.Models.Helper
{
    public static class IPAddressHelper
    {
        public static string GetRemoteIpAddress(HttpContext context)
        {
            string IPAddress;
            IPAddress = context.Request.Headers["Cdn-Src-Ip"];
            if (!string.IsNullOrEmpty(IPAddress)) return IPAddress;
            IPAddress = context.Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(IPAddress)) return IPAddress;
            return context.Connection.RemoteIpAddress.ToString();
        }
    }
}
