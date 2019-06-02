using Microsoft.Extensions.Options;

namespace EasyProxy.Server
{
    public class ServerOptions : IOptions<ServerOptions>
    {
        public ServerOptions Value => this;
        public int ServerPort { get; set; } = 8000;
        public bool EanbleDashboard { get; set; } = false;
        public int DashboardPort { get; set; } = 8090;
        public int MaxConnection { get; set; } = 20;
    }
}
