using System.ServiceModel;
using System.ServiceModel.Web;
using ServiceDefinitionInterface;

namespace PolicyProjectManagementService
{
    [ServiceContract]
    public interface ILoginService : IServiceDefinitionInterface
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetLoginRest))]
        string GetLoginRest(LoginDataContract loginFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetLogin))]
        Result<LoginDataContract[]> GetLogin(LoginDataContract loginFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetCertificate))]
        Result<string[]> GetCertificate(long loginId);

        [OperationContract]
        [WebInvoke(Method = "POST",
             RequestFormat = WebMessageFormat.Json,
             ResponseFormat = WebMessageFormat.Json,
             BodyStyle = WebMessageBodyStyle.Bare,
             UriTemplate = nameof(GetCertificateRest))]
        string GetCertificateRest(long loginId);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(AddLogin))]
        Result<object> AddLogin(LoginDataContract login);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(UpdateLogin))]
        Result<object> UpdateLogin(LoginDataContract oldLogin, LoginDataContract newLogin);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(DeleteLogin))]
        Result<object> DeleteLogin(LoginDataContract login);
    }
}