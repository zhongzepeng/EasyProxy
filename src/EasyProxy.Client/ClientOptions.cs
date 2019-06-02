using Microsoft.Extensions.Options;

namespace EasyProxy.Client
{
    public class ClientOptions : IOptions<ClientOptions>
    {
        public ClientOptions Value => this;

        public string ServerAddress { get; set; } = "127.0.0.1";

        public int ServerPort { get; set; } = 8000;

        public int ClientId { get; set; }
    }
}
