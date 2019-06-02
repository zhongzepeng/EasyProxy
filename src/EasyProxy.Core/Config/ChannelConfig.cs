namespace EasyProxy.Core.Config
{
    public class ChannelConfig
    {
        public int ClientId { get; set; }

        public int ChannelId { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 外网端口
        /// </summary>
        public int BackendPort { get; set; }
        /// <summary>
        /// 内网端口
        /// </summary>
        public int FrontendPort { get; set; }

        /// <summary>
        /// 内网Ip
        /// </summary>
        public string FrontendIp { get; set; }
    }
}
