using System;
using System.Security.Cryptography;
using System.Text;
namespace Scottxu.Blog.Models.Helper
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
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] encryptedBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(inputString));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                sb.AppendFormat("{0:x2}", encryptedBytes[i]);
            }
            return sb.ToString();
        }
    }
}
