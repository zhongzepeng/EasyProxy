namespace EasyProxy.Core.Common
{
    public class ProxyConfigurationItem
    {
        /// <summary>
        /// 唯一标志
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 连接名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 外网端口
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// 内网端口
        /// </summary>
        public int BackendProt { get; set; }

        /// <summary>
        /// 内网Ip
        /// </summary>
        public string BackendAddress { get; set; }

        /// <summary>
        /// 最大连接数量
        /// </summary>
        public int MaxConnection { get; set; } = 20;
    }
}
