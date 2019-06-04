using EasyProxy.Core;
using EasyProxy.Core.Codec;
using EasyProxy.Core.Common;
using EasyProxy.Core.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Threading.Tasks;

namespace EasyProxy.Client
{
    public class ProxyClient : IProxyHost
    {
        private readonly ILogger<ProxyClient> logger;
        private readonly ClientOptions options;
        private readonly IPackageEncoder<ProxyPackage> encoder;
        private readonly IPackageDecoder<ProxyPackage> decoder;
        private readonly ConfigHelper configHelper;

        public ProxyClient(IOptions<ClientOptions> options
            , ILogger<ProxyClient> logger
            , ConfigHelper configHelper
            , IPackageEncoder<ProxyPackage> encoder
            , IPackageDecoder<ProxyPackage> decoder)
        {
            this.configHelper = configHelper;
            this.logger = logger;
            this.options = options?.Value;
            Checker.NotNull(this.options);
            this.encoder = encoder;
            this.decoder = decoder;
        }

        public async Task StartAsync()
        {
            var channels = await configHelper.GetChannelsAsync(options.ClientId);

            if (channels.Count == 0)
            {
                logger.LogInformation("not channel in config");
                return;
            }
            foreach (var channel in channels)
            {
                logger.LogInformation($"channel:{channel.ClientId}");
                var connection = new ProxyClientConnection(logger, IPAddress.Parse(options.ServerAddress), options.ServerPort, channel, encoder, decoder);
                _ = connection.StartAsync();
            }
            await Task.CompletedTask;
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
