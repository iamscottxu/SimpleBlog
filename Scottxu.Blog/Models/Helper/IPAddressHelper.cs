using System;
using Microsoft.AspNetCore.Http;

namespace Scottxu.Blog.Models.Helper
{
    public static class IPAddressHelper
    {
        public static string GetRemoteIpAddress(HttpContext context)
        {
            string IPAddress;
            IPAddress = context.Request.Headers["X-Forwarded-For"];
            return !string.IsNullOrEmpty(IPAddress) ? IPAddress : context.Connection.RemoteIpAddress.ToString();
        }
    }
}
