using System;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;

namespace AttackPrevent.Business
{
    public class Utils
    {
        public static T GetMemoryCache<T>(string key)
        {
            MemoryCache cache = MemoryCache.Default;
            T t = (T)cache.Get(key);
            return t;
        }

        public static void SetMemoryCache<T>(string key, T value, int timeout = 30)
        {
            MemoryCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddMinutes(timeout));
            cache.Set(key, value, policy);
        }

        public static void RemoveMemoryCache(string key)
        {
            MemoryCache cache = MemoryCache.Default;
            cache.Remove(key);
        }

        public static T GetMemoryCache<T>(string key, Func<T> func, int timeout = 30)
        {
            T t = GetMemoryCache<T>(key);
            if (t == null)
            {
                t = func();
                SetMemoryCache(key, t, timeout);
            }

            return t;
        }

        public static string AesEncrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            var toEncryptArray = Encoding.UTF8.GetBytes(str);

            var rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = rm.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray);
        }

        public static string AesDecrypt(string str, string key)
        {
            if (string.IsNullOrEmpty(str)) return null;
            byte[] toEncryptArray = Convert.FromBase64String(str);

            var rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = rm.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
    }
}
