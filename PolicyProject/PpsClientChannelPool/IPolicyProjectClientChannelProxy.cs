using System.ServiceModel;
using ServiceDefinitionInterface;

namespace PpsClientChannelProxy
{
    public interface IPolicyProjectClientChannelProxy
    {
        string Error { get; }

        ChannelFactory<T> GetPpsChannelFactory<T>() where T : IServiceDefinitionInterface;
    }
}