using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Linq;

namespace Scottxu.Blog.Models.Helpers
{
    public static class HttpRequestHelper
    {
        static string GetParametersString(IDictionary<string, string> parameters)
            => string.Join("&",
                parameters.ToList().Select(k => $"{WebUtility.UrlEncode(k.Key)}={WebUtility.UrlEncode(k.Value)}"));

        /// <summary>
        /// 发送http post请求
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="parameters">查询参数集合</param>
        /// <returns></returns>
        public static HttpWebResponse CreatePostResponse(string url, IDictionary<string, string> parameters)
        {
            var request = WebRequest.Create(url); //创建请求对象

            request.Method = "POST"; //请求方式
            request.ContentType = "application/x-www-form-urlencoded"; //链接类型
            //构造查询字符串
            if (parameters == null || parameters.Count == 0) return request.GetResponse() as HttpWebResponse;
            var data = Encoding.UTF8.GetBytes(GetParametersString(parameters));
            //写入请求流
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 发送http Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters">查询参数集合</param>
        /// <returns></returns>
        public static HttpWebResponse CreateGetResponse(string url, IDictionary<string, string> parameters)
        {
            var request = WebRequest.Create($"{url}?{GetParametersString(parameters)}");
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded"; //链接类型
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 从HttpWebResponse对象中提取响应的数据转换为字符串
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string GetStringFromResponse(HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                var reader = new StreamReader(stream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// 从HttpWebResponse对象中提取响应的数据转换为对象
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static object GetObjectFromJsonResponse(HttpWebResponse response)
        {
            return JsonConvert.DeserializeObject(GetStringFromResponse(response));
        }
    }
}