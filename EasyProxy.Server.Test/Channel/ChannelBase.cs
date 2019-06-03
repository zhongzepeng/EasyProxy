namespace EasyProxy.Server.Test.Channel
{
    public abstract class ChannelBase : IChannel
    {
    }

    public abstract class ChannelBase<TPackage> : ChannelBase, IChannel<TPackage>, IChannel where TPackage : class
    {

    }
}
