using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using ServiceDefinitionInterface;

namespace PolicyProjectManagementService
{
    [ServiceContract]
    public interface IPolicySetService : IServiceDefinitionInterface
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetGroupIdAndPolicyIdDct))]
        Result<Dictionary<int, int[]>> GetGroupIdAndPolicyIdDct();

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetPolicySet))]
        Result<PolicySetDataContract[]> GetPolicySet(PolicySetDataContract policySetFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetPolicySetRest))]
        string GetPolicySetRest(PolicySetDataContract policySetFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetPolicySetForLoginRest))]
        string GetPolicySetForLoginRest(PolicySetDataContract policySetFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetPolicySetForGroup))]
        Result<PolicySetDataContract[]> GetPolicySetForGroup(int groupId);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetPolicySetForLogin))]
        Result<PolicySetDataContract[]> GetPolicySetForLogin(long loginId);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(AddPolicySet))]
        Result<object> AddPolicySet(PolicySetDataContract policySet);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(UpdatePolicySet))]
        Result<object> UpdatePolicySet(PolicySetDataContract oldPolicySet, PolicySetDataContract newPolicySet);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(DeletePolicySet))]
        Result<object> DeletePolicySet(PolicySetDataContract policySet);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(UpdatePolicySetForGroup))]
        Result<object> UpdatePolicySetForGroup(int groupId, PolicySetDataContract[] policySetList);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(UpdatePolicySetForLogin))]
        Result<object> UpdatePolicySetForLogin(long loginId, PolicySetDataContract[] policySetList);
    }
}