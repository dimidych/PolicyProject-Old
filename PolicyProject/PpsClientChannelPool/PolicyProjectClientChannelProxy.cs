using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using ServiceDefinitionInterface;

namespace PpsClientChannelProxy
{
    public class PolicyProjectClientChannelProxy : IPolicyProjectClientChannelProxy
    {
        private readonly string _serviceAssemblyPath;
        private readonly string _serviceHostAddress;
        private readonly IEnumerable<string> _serviceNameList;

        public PolicyProjectClientChannelProxy(IEnumerable<string> serviceNameList, string serviceAssemblyPath,
            string serviceHostAddress)
        {
            if (serviceNameList == null || !serviceNameList.Any())
                throw new ArgumentException("Пустой список служб для запуска");

            if (string.IsNullOrEmpty(serviceAssemblyPath) || !File.Exists(serviceAssemblyPath))
                throw new ArgumentException("Файл сборки служб не существует");

            if (string.IsNullOrEmpty(serviceHostAddress))
                throw new ArgumentException("IP адрес хоста служб не может быть пустым");

            _serviceNameList = serviceNameList;
            _serviceAssemblyPath = serviceAssemblyPath;
            _serviceHostAddress = serviceHostAddress;
        }

        public string Error { get; private set; }

        /*private IEnumerable<ChannelFactory> CreateChannelFactories(out string error)
        {
            error = string.Empty;

            try
            {
                var serviceAsm = Assembly.LoadFrom(_serviceAssemblyPath);
                var serviceTypes = serviceAsm.GetTypes()
                    .Where(typ =>
                            typ.IsClass &&
                            _serviceNameList.Contains(typ.FullName.Trim()));

                var  result=new List<ChannelFactory>();
                 
                foreach (var serviceType in serviceTypes)
                {
                    var svcRealizationAttr = serviceType.CustomAttributes.FirstOrDefault(
                        attr => attr.AttributeType == typeof(ServiceRealizationAttribute));

                    if (svcRealizationAttr == null)
                        continue;

                    var svcContractTypeName =
                        (string)
                            (svcRealizationAttr.NamedArguments.FirstOrDefault(
                                arg => arg.MemberName.Equals("InterfaceName")).TypedValue.Value);
                    var svcContractType = serviceType.GetInterface(svcContractTypeName, true);
                    var serviceUri = new Uri(string.Concat(_serviceHostAddress, "/", serviceType.FullName.Replace('.', '/'), "/"));
                    var channelFactoryType= GetGenericChannelFactory(svcContractType);
                    var channelFactory = (ChannelFactory)Activator.CreateInstance(channelFactoryType, new WebHttpBinding(), serviceUri.AbsoluteUri);
                    result.Add(channelFactory);
                }

                return result;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return null;
        }

        private Type GetGenericChannelFactory(Type serviceContractType)
        {
            var channelFactoryType = typeof (ChannelFactory);
            channelFactoryType = channelFactoryType.MakeGenericType(serviceContractType);
            return channelFactoryType;
        }*/

        public ChannelFactory<T> GetPpsChannelFactory<T>() where T : IServiceDefinitionInterface
        {
            ChannelFactory<T> channelFactory = null;

            try
            {
                var serviceAsm = Assembly.LoadFrom(_serviceAssemblyPath);
                var type = typeof (T);
                var serviceType =
                    serviceAsm.GetTypes()
                        .FirstOrDefault(
                            typ =>
                                typ.IsClass && type.IsAssignableFrom(typ) &&
                                _serviceNameList.Contains(typ.FullName.Trim()));

                if (serviceType == null)
                    throw new Exception("Неизвестный сервис-контракт");

                var serviceUri =
                    new Uri(string.Concat(_serviceHostAddress, "/", serviceType.FullName.Replace('.', '/'), "/"));
                channelFactory = new ChannelFactory<T>(new WebHttpBinding(), serviceUri.AbsoluteUri);
                channelFactory.Endpoint.Behaviors.Add(new WebHttpBehavior());
                //channelFactory = new ChannelFactory<T>(new BasicHttpBinding(), serviceUri.AbsoluteUri);
                return channelFactory;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return channelFactory;
            }
        }
    }
}