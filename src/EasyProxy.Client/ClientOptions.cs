using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyProxy.Client
{
    public class ClientOptions : IOptions<ClientOptions>
    {
        public ClientOptions Value => this;

        public string ServerAddress { get; set; }

        /// <summary>
        /// 用于客户端和服务端之间传输数据端口
        /// </summary>
        public int ProxyPort { get; set; } = 7000;
    }
}
