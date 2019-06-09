using Microsoft.Extensions.Options;

namespace EasyProxy.Server
{
    public class ServerOptions : IOptions<ServerOptions>
    {
        public ServerOptions Value => this;
        public int ServerPort { get; set; } = 8000;
        public bool EanbleDashboard { get; set; } = true;
        //public int DashboardPort { get; set; } = 8090;
        //public string DashboardHost { get; set; } = "localhost";
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Secret { get; set; }
        public int MaxConnection { get; set; } = 20;
    }
}
