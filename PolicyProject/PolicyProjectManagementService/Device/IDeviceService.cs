using System.ServiceModel;
using System.ServiceModel.Web;
using ServiceDefinitionInterface;

namespace PolicyProjectManagementService
{
    [ServiceContract]
    public interface IDeviceService : IServiceDefinitionInterface
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetDevice))]
        Result<DeviceInfoDataContract[]> GetDevice(DeviceInfoDataContract deviceFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetDeviceRest))]
        string GetDeviceRest(DeviceInfoDataContract deviceFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(AddDevice))]
        Result<object> AddDevice(DeviceInfoDataContract device);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(UpdateDevice))]
        Result<object> UpdateDevice(DeviceInfoDataContract oldDevice, DeviceInfoDataContract newDevice);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(DeleteDevice))]
        Result<object> DeleteDevice(DeviceInfoDataContract device);
    }
}