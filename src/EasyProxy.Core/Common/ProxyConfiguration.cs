using System;
using System.Collections.Generic;

namespace EasyProxy.Core.Common
{
    public class ProxyConfiguration
    {
        public Guid ClientId { get; set; }

        public string ClientName { get; set; }

        public List<ProxyConfigurationItem> Connections { get; set; }
    }
}
