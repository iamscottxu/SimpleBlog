using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Scottxu.Blog.Models.Helpers
{
    /// <summary>
    /// 单相混淆加密用户密码，并比较密码是否一致的类
    /// </summary>
    public static class PasswordHelper
    {
        #region field & constructor

        /// <summary>
        /// 盐的长度
        /// </summary>
        private const int SaltLength = 16;

        #endregion

        /// <summary>
        /// 对比用户二次加密密码是否和三次加密后密码一致
        /// </summary>
        /// <param name="dbPassword">数据库中加密后的密码</param>
        /// <param name="userPassword">用户MD5加密密码</param>
        /// <returns></returns>
        public static bool ComparePasswords(string dbPassword, string userPassword)
        {
            var dbPwd = Convert.FromBase64String(dbPassword);

            var hashedPwd = HashString(userPassword);

            if (dbPwd.Length == 0 || hashedPwd.Length == 0 || dbPwd.Length != hashedPwd.Length + SaltLength)
            {
                return false;
            }

            var saltValue = new byte[SaltLength];
            //  int saltOffset = dbPwd.Length - hashedPwd.Length;
            var saltOffset = hashedPwd.Length;
            for (var i = 0; i < SaltLength; i++)
                saltValue[i] = dbPwd[saltOffset + i];

            var saltedPassword = CreateSaltedPassword(saltValue, hashedPwd);

            // compare the values
            return CompareByteArray(dbPwd, saltedPassword);
        }

        /// <summary>
        /// 创建用户的数据库密码
        /// </summary>
        /// <param name="userPassword"></param>
        /// <param name="md5Encryption"></param>
        /// <returns></returns>
        public static string CreateDbPassword(string userPassword, bool md5Encryption)
        {
            //MD5加密
            if (md5Encryption)
                userPassword = EncryptionHelper.StringToMD5Hash(EncryptionHelper.StringToMD5Hash(userPassword));

            var unsaltedPassword = HashString(userPassword);

            //Create a salt value
            var saltValue = new byte[SaltLength];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(saltValue);

            var saltedPassword = CreateSaltedPassword(saltValue, unsaltedPassword);


            return Convert.ToBase64String(saltedPassword);
        }

        #region 私有函数

        /// <summary>
        /// 将一个字符串哈希化
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static byte[] HashString(string str)
        {
            var pwd = System.Text.Encoding.UTF8.GetBytes(str);

            var sha1 = SHA1.Create();
            var saltedPassword = sha1.ComputeHash(pwd);
            return saltedPassword;
        }

        static bool CompareByteArray(IReadOnlyList<byte> array1, IReadOnlyList<byte> array2)
        {
            if (array1.Count != array2.Count)
                return false;
            return !array1.Where((t, i) => t != array2[i]).Any();
        }

        // create a salted password given the salt value
        static byte[] CreateSaltedPassword(byte[] saltValue, byte[] unsaltedPassword)
        {
            // add the salt to the hash
            var rawSalted = new byte[unsaltedPassword.Length + saltValue.Length];
            unsaltedPassword.CopyTo(rawSalted, 0);
            saltValue.CopyTo(rawSalted, unsaltedPassword.Length);

            //Create the salted hash            
            var sha1 = SHA1.Create();
            var saltedPassword = sha1.ComputeHash(rawSalted);

            // add the salt value to the salted hash
            var dbPassword = new byte[saltedPassword.Length + saltValue.Length];
            saltedPassword.CopyTo(dbPassword, 0);
            saltValue.CopyTo(dbPassword, saltedPassword.Length);

            return dbPassword;
        }

        #endregion
    }
}