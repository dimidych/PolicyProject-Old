using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using ServiceDefinitionInterface;

namespace PolicyProjectManagementService
{
    [ServiceContract]
    public interface IEventLogService : IServiceDefinitionInterface
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetEventLog))]
        Result<EventLogDataContract[]> GetEventLog(EventLogDataContract eventLogFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(GetFilteredEventLog))]
        Result<EventLogDataContract[]> GetFilteredEventLog(DateTime fromDate, DateTime? toDate, int? eventId);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetEventLogRest))]
        string GetEventLogRest(EventLogDataContract eventLogFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(GetEventLogRestJson))]
        string GetEventLogRestJson(string eventLogFilter);

        [OperationContract]
        [WebInvoke(Method = "POST",
           RequestFormat = WebMessageFormat.Json,
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Wrapped,
           UriTemplate = nameof(GetFilteredEventLogRest))]
        string GetFilteredEventLogRest(DateTime fromDate, DateTime? toDate, int? eventId);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = nameof(AddEventLog))]
        Result<object> AddEventLog(EventLogDataContract eventLog);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(AddEventLogExplicit))]
        Result<object> AddEventLogExplicit(string device, string login, int eventId, long documentId, string message);

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Wrapped,
            UriTemplate = nameof(DeleteEventLog))]
        Result<object> DeleteEventLog(DateTime fromDate, DateTime? toDate, int? eventId);
    }
}