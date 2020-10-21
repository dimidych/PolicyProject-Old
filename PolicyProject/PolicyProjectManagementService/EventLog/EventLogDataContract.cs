using System;
using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class EventLogDataContract
    {
        [DataMember(Order = 0)]
        public long EventLogId { get; set; }

        [DataMember(Order = 1)]
        public DateTime EventLogDate { get; set; }

        [DataMember(Order = 2)]
        public int EventId { get; set; }

        [DataMember(Order = 3)]
        public long? DocumentId { get; set; }

        [DataMember(Order = 4)]
        public string Message { get; set; }

        [DataMember(Order = 5)]
        public string Device { get; set; }

        [DataMember(Order = 6)]
        public string Login { get; set; }

        [DataMember(Order = 7)]
        public string EventName { get; set; }

        public static tbl_activity_log FromEventLogDataContractToTblEventLog(EventLogDataContract eventLogData)
        {
            if (eventLogData.EventLogId < 1 || eventLogData.EventId < 1
                || string.IsNullOrEmpty(eventLogData.Device.Trim()) || string.IsNullOrEmpty(eventLogData.Login.Trim()))
                return null;

            return new tbl_activity_log
            {
                id = eventLogData.EventLogId,
                log_date = eventLogData.EventLogDate,
                id_event = eventLogData.EventId,
                device = eventLogData.Device.Trim(),
                login = eventLogData.Login.Trim(),
                id_document = eventLogData.DocumentId,
                message = eventLogData.Message
            };
        }

        public static EventLogDataContract FromTblEventLogToEventLogDataContract(tbl_activity_log eventLogTblData)
        {
            if (eventLogTblData.id < 1 || eventLogTblData.id_event < 1
                || string.IsNullOrEmpty(eventLogTblData.device.Trim()) ||
                string.IsNullOrEmpty(eventLogTblData.login.Trim()))
                return null;

            return new EventLogDataContract
            {
                EventLogId = eventLogTblData.id,
                EventLogDate = eventLogTblData.log_date,
                EventId = eventLogTblData.id_event,
                Device = eventLogTblData.device.Trim(),
                Login = eventLogTblData.login.Trim(),
                DocumentId = eventLogTblData.id_document,
                Message = eventLogTblData.message
            };
        }

        public static bool Compare(EventLogDataContract obj1, EventLogDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.EventLogId == obj2.EventLogId && obj1.EventLogDate == obj2.EventLogDate
                   && string.Equals(obj1.Device, obj2.Device, StringComparison.InvariantCultureIgnoreCase)
                   && string.Equals(obj1.Login, obj2.Login, StringComparison.InvariantCultureIgnoreCase)
                   && obj1.EventId == obj2.EventId && obj1.DocumentId == obj2.DocumentId;
        }
    }
}