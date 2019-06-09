using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EasyProxy.HttpServer.Authorization
{
    public class JwtHelper
    {
        public static string GenerateJwtToken(string secret, Dictionary<string, string> payloadDic)
        {
            var header = Base64UrlEncode(new
            {
                alg = "HS256",
                typ = "JWT"
            });

            var payload = Base64UrlEncode(payloadDic);
            var signature = GenerateSignature(header, payload, secret);
            return $"{header}.{payload}.{signature}";
        }

        public static (bool, Dictionary<string, string>) ValidateToken(string token, string secret)
        {
            var parts = token.Split('.');
            var header = parts[0];
            var payload = parts[1];
            var signature = parts[2];
            var encoding = Encoding.UTF8;
            var ns = GetHash($"{header}.{payload}", secret);
            var verfy = ns.Equals(signature);
            var dic = Base64UrlDecode<Dictionary<string, string>>(payload);
            return (verfy, dic);
        }

        private static string Base64UrlEncode(object data)
        {
            var encoder = new JwtBase64UrlEncoder();
            return encoder.Encode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
        }

        private static T Base64UrlDecode<T>(string tokenPart)
        {
            var encoder = new JwtBase64UrlEncoder();
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(encoder.Decode(tokenPart)));
        }

        private static string GenerateSignature(string header, string payload, string secret)
        {
            return GetHash($"{header}.{payload}", secret);
        }

        public static string GetHash(string text, string key)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();

            var textBytes = encoding.GetBytes(text);
            var keyBytes = encoding.GetBytes(key);

            byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
