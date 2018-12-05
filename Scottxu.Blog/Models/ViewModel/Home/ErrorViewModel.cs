using System;

namespace Scottxu.Blog.Models.ViewModel.Home
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public int? ErrorCode { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string EnglishDescribe
        {
            get
            {
                switch (ErrorCode)
                {
                    case 100:
                        return "Continue";
                    case 101:
                        return "Switching Protocols";
                    case 200:
                        return "OK";
                    case 201:
                        return "Created";
                    case 202:
                        return "Accepted";
                    case 203:
                        return "Non - Authoritative Information";
                    case 204:
                        return "No Content";
                    case 205:
                        return "Reset Content";
                    case 206:
                        return "Partial Content";
                    case 300:
                        return "Multiple Choices";
                    case 301:
                        return "Moved Permanently";
                    case 302:
                        return "Found";
                    case 303:
                        return "See Other";
                    case 304:
                        return "Not Modified";
                    case 305:
                        return "Use Proxy";
                    case 306:
                        return "Unused";
                    case 307:
                        return "Temporary Redirect";
                    case 400:
                        return "Bad Request";
                    case 401:
                        return "Unauthorized";
                    case 402:
                        return "Payment Required";
                    case 403:
                        return "Forbidden";
                    case 404:
                        return "Not Found";
                    case 405:
                        return "Method Not Allowed";
                    case 406:
                        return "Not Acceptable";
                    case 407:
                        return "Proxy Authentication";
                    case 408:
                        return "Request Time-out";
                    case 409:
                        return "Conflict";
                    case 410:
                        return "Gone";
                    case 411:
                        return "Length Required";
                    case 412:
                        return "Precondition Failed";
                    case 413:
                        return "Request Entity Too Large";
                    case 414:
                        return "Request - URI Too Large";
                    case 415:
                        return "Unsupported Media Type";
                    case 416:
                        return "Requested range not satisfiable";
                    case 417:
                        return "Expectation Failed";
                    case 500:
                        return "Internal Server Error";
                    case 501:
                        return "Not Implemented";
                    case 502:
                        return "Bad Gateway";
                    case 503:
                        return "Service Unavailable";
                    case 504:
                        return "Gateway Time-out";
                    case 505:
                        return "HTTP Version not supported";
                    default:
                        return "Unknown Error";
                }
            }
        }

        public string ChineseDescribe
        {
            get
            {
                switch (ErrorCode)
                {
                    case 100:
                        return "继续请求。";
                    case 101:
                        return "切换协议。";
                    case 200:
                        return "请求成功。";
                    case 201:
                        return "成功请求并创建了新的资源。";
                    case 202:
                        return "已经接受请求，但未完成处理。";
                    case 203:
                        return "请求成功，但返回的meta信息不在原始的服务器，而是一个副本。";
                    case 204:
                        return "服务器处理成功，但未返回内容。";
                    case 205:
                        return "用户终端应重置文档视图。";
                    case 206:
                        return "服务器成功处理了部分GET请求。";
                    case 300:
                        return "请求的资源包括多个位置。";
                    case 301:
                        return "请求的资源已被永久移动。";
                    case 302:
                        return "请求的资源已被临时移动。";
                    case 303:
                        return "查看其它地址。";
                    case 304:
                        return "所请求的资源未修改。";
                    case 305:
                        return "所请求的资源必须通过代理访问。";
                    case 306:
                        return "已经被废弃的HTTP状态码。";
                    case 307:
                        return "临时重定向。";
                    case 400:
                        return "客户端请求的语法错误。";
                    case 401:
                        return "请求要求用户的身份认证。";
                    case 402:
                        return "保留代码。";
                    case 403:
                        return "服务器拒绝执行此请求。";
                    case 404:
                        return "所请求的资源无法找到。";
                    case 405:
                        return "客户端请求中的方法被禁止。";
                    case 406:
                        return "服务器无法根据客户端请求的内容特性完成请求。";
                    case 407:
                        return "请求要求代理的身份认证。";
                    case 408:
                        return "服务器等待客户端发送的请求时间过长。";
                    case 409:
                        return "服务器处理请求时发生了冲突。";
                    case 410:
                        return "请求的资源已经不存在。";
                    case 411:
                        return "服务器无法处理客户端发送的不带Content-Length的请求信息。";
                    case 412:
                        return "客户端请求信息的先决条件错误。";
                    case 413:
                        return "由于请求的实体过大，服务器无法处理。";
                    case 414:
                        return "请求的URI过长，服务器无法处理。";
                    case 415:
                        return "服务器无法处理请求附带的媒体格式。";
                    case 416:
                        return "请求的范围无效。";
                    case 417:
                        return "服务器无法满足Expect的请求头信息。";
                    case 500:
                        return "服务器内部错误。";
                    case 501:
                        return "服务器不支持请求的功能。";
                    case 502:
                        return "充当网关或代理的服务器，从远端服务器接收到了一个无效的请求。";
                    case 503:
                        return "由于超载或系统维护，服务器暂时的无法处理客户端的请求。";
                    case 504:
                        return "充当网关或代理的服务器，未及时从远端服务器获取请求。";
                    case 505:
                        return "服务器不支持请求的HTTP协议的版本。";
                    default:
                        return "未知错误。";
                }
            }
        }
    }
}