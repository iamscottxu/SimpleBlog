using System;
using Microsoft.AspNetCore.Http;

namespace Scottxu.Blog.Models.Helpers
{
    public static class IPAddressHelper
    {
        public static string GetRemoteIpAddress(HttpContext context)
        {
            string ipAddress = context.Request.Headers["X-Forwarded-For"];
            return !string.IsNullOrEmpty(ipAddress) ? ipAddress : context.Connection.RemoteIpAddress.ToString();
        }
    }
}