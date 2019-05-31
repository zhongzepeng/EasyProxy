using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyProxy.Core
{
    public interface IConnection
    {
        Task StartAsync();

        Task StopAsync();
    }
}
