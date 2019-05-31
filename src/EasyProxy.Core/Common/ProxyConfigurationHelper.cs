using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyProxy.Core.Common
{
    public class ProxyConfigurationHelper
    {
        public static List<ProxyConfigurationItem> GetAllConnections()
        {
            return new List<ProxyConfigurationItem>
                    {
                        new ProxyConfigurationItem {
                             BackendAddress = "127.0.0.1",
                             BackendProt = 9001,
                             Name = "web",
                             ServerPort = 9002
                        }
                    };
        }

        public static ProxyConfigurationItem GetConnectionByServerPort(int serverPort)
        {
            var all = GetAllConnections();

            return all.SingleOrDefault(x => x.ServerPort == serverPort);
        }

        public static List<ProxyConfigurationItem> GetConnectionByClientId(Guid clientId)
        {
            return new List<ProxyConfigurationItem>
                    {
                        new ProxyConfigurationItem {
                             BackendAddress = "127.0.0.1",
                             BackendProt = 9001,
                             Name = "web",
                             ServerPort = 9002
                        }
                    };
        }
    }
}
