using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using ServiceRealizationAtribute;

namespace PpcServicePool
{
    public class PolicyProjectServicePool : IPolicyProjectServicePool
    {
        private readonly string _serviceAssemblyPath;
        private readonly string _serviceHostAddress;
        private readonly IEnumerable<string> _serviceNameList;
        private string _error;

        public PolicyProjectServicePool(IEnumerable<string> serviceNameList, string serviceAssemblyPath,
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
            ServicesHosts = CreateServiceHosts(out _error);
        }

        public IEnumerable<ServiceHost> ServicesHosts { get; private set; }

        public string Error
        {
            get { return _error; }
        }

        public ServiceHost GetServiceFromPool(Type serviceType)
        {
            if (ServicesHosts == null || !ServicesHosts.Any())
                ServicesHosts = CreateServiceHosts(out _error);

            var result = ServicesHosts.FirstOrDefault(host => host.GetType() == serviceType);

            if (result == null)
                _error = "Service does not exists in pool";

            return result;
        }

        private IEnumerable<ServiceHost> CreateServiceHosts(out string error)
        {
            error = string.Empty;

            try
            {
                var serviceAsm = Assembly.LoadFrom(_serviceAssemblyPath);
                var serviceTypes = serviceAsm.GetTypes()
                    .Where(
                        typ =>
                            typ.IsClass && _serviceNameList.Contains(typ.FullName.Trim())
                            && typ.CustomAttributes != null && typ.CustomAttributes.Any());

                var result = new List<ServiceHost>();

                foreach (var serviceType in serviceTypes)
                {
                    var svcRealizationAttr = serviceType.CustomAttributes.FirstOrDefault(
                        attr => attr.AttributeType == typeof (ServiceRealizationAttribute));

                    if (svcRealizationAttr == null)
                        continue;

                    var svcContractTypeName =
                        (string)
                            svcRealizationAttr.NamedArguments.FirstOrDefault(
                                arg => arg.MemberName.Equals("InterfaceName")).TypedValue.Value;
                    var svcContractType = serviceType.GetInterface(svcContractTypeName, true);
                    result.Add(CreateServiceInstance(serviceType, svcContractType));
                }

                return result;
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return null;
        }

        private ServiceHost CreateServiceInstance(Type serviceType, Type serviceContractType)
        {
            var svcUri = new Uri(string.Concat(_serviceHostAddress, "/", serviceType.FullName.Replace('.', '/'), "/"));
            var svcHost = new ServiceHost(serviceType, svcUri);
            var webBinding = new WebHttpBinding("WebHttpBinding_PolicyProjectManagementService");
            var metadataBehavior = svcHost.Description.Behaviors.Find<ServiceMetadataBehavior>() ??
                                   new ServiceMetadataBehavior();
            metadataBehavior.HttpGetEnabled = true;
            svcHost.Description.Behaviors.Add(metadataBehavior);
            var webHttpBehaivior = svcHost.Description.Behaviors.Find<WebHttpBehavior>() ?? new WebHttpBehavior();
            var webHttpEndPoint = svcHost.AddServiceEndpoint(serviceContractType, webBinding, svcUri);
            webHttpEndPoint.Behaviors.Add(webHttpBehaivior);
            svcHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
            return svcHost;
        }
    }
}