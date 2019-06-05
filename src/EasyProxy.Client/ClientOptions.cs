using Microsoft.Extensions.Options;

namespace EasyProxy.Client
{
    public class ClientOptions : IOptions<ClientOptions>
    {
        public ClientOptions Value => this;

        /// <summary>
        /// 服务端地址
        /// </summary>
        public string ServerAddress { get; set; } = "127.0.0.1";

        /// <summary>
        /// 服务端端口
        /// </summary>
        public int ServerPort { get; set; } = 8000;

        /// <summary>
        /// 客户端Id
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// 客户端密钥
        /// </summary>
        public string SecretKey { get; set; }
    }
}
