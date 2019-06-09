using System;

namespace EasyProxy.HttpServer.Authorization
{
    internal interface IBase64UrlEncoder
    {
        /// <summary>
        /// Encodes the byte array to a Base64 string.
        /// </summary>
        string Encode(byte[] input);

        /// <summary>
        /// Decodes the Base64 string to a byte array.
        /// </summary>
        byte[] Decode(string input);
    }

    internal class JwtBase64UrlEncoder : IBase64UrlEncoder
    {
        public byte[] Decode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0:
                    break; // No pad chars in this case
                case 2:
                    output += "==";
                    break; // Two pad chars
                case 3:
                    output += "=";
                    break; // One pad char
                default:
                    throw new FormatException("Illegal base64url string.");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }

        public string Encode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }
    }
}
