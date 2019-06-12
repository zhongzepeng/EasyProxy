using DotLiquid;

namespace EasyProxy.Server.Dtos
{
    public class ClientListOutputDto : ILiquidizable
    {
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string SecretKey { get; set; }
        public int ChannelCount { get; set; }

        public object ToLiquid()
        {
            return new
            {
                ClientId,
                Name,
                SecretKey,
                ChannelCount
            };
        }
    }
}
