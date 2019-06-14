namespace EasyProxy.Server.Dtos
{
    public class AddChannleInputDto
    {
        public int ClientId { get; set; }

        public string Name { get; set; }

        public int PublicPort { get; set; }

        public int PrivatePort { get; set; }

        public string PrivateIp { get; set; }
    }
}
