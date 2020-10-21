using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using ServiceDefinitionInterface;

namespace PolicyProjectManagementService
{
    [ServiceContract]
    public interface ILoginDevicesService : IServiceDefinitionInterface
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetLoginIdAndDeviceIdDct))]
        Result<Dictionary<long, long[]>> GetLoginIdAndDeviceIdDct();

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetLoginDevices))]
        Result<LoginDevicesDataContract[]> GetLoginDevices(LoginDevicesDataContract loginDevicesFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetLoginDevicesRest))]
        string GetLoginDevicesRest(LoginDevicesDataContract loginDevicesFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(UpdateLoginDevices))]
        Result<object> UpdateLoginDevices(long loginId, LoginDevicesDataContract[] loginDevicesList);


        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(UpdateLoginDevicesRest))]
        string UpdateLoginDevicesRest(LoginDevicesDataContract loginDevice);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(SetDevicesForUpdate))]
        Result<object> SetDevicesForUpdate(List<long> loginIdList);
    }
}