using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Scottxu.Blog.Models.Helpers
{
    /// <summary>
    /// 加密解密帮助类
    /// </summary>
    public static class EncryptionHelper
    {
        /// <summary>
        /// 创建字符串的MD5哈希值
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns>字符串MD5哈希值的十六进制字符串</returns>
        public static string StringToMD5Hash(string inputString)
        {
            var md5 = new MD5CryptoServiceProvider();
            var encryptedBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(inputString));
            var sb = new StringBuilder();
            encryptedBytes.ToList().ForEach(t => sb.AppendFormat("{0:x2}", t));
            return sb.ToString();
        }
    }
}