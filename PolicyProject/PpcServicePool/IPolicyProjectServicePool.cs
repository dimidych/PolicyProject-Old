using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace PpcServicePool
{
    public interface IPolicyProjectServicePool
    {
        IEnumerable<ServiceHost> ServicesHosts { get; }
        string Error { get; }
        ServiceHost GetServiceFromPool(Type serviceType);
    }
}