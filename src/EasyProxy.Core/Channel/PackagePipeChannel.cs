namespace EasyProxy.Core.Channel
{
    //public abstract class PipeChannel<TPackage> : ChannelBase<TPackage>, IChannel<TPackage>, IChannel where TPackage : class
    public abstract class PackagePipeChannel<TPackage> : PackageChannelBase<TPackage>, IChannel<TPackage>, IChannel where TPackage : class
    {
    }
}
