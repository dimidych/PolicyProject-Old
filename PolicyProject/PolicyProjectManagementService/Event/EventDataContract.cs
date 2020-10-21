using System;
using System.Runtime.Serialization;

namespace PolicyProjectManagementService
{
    [DataContract]
    public class EventDataContract
    {
        [DataMember(Order = 0)]
        public int EventId { get; set; }

        [DataMember(Order = 1)]
        public string EventName { get; set; }

        public static tbl_event FromEventDataContractToTblEvent(EventDataContract eventData)
        {
            if (eventData.EventId < 1 || string.IsNullOrEmpty(eventData.EventName))
                return null;

            return new tbl_event
            {
                id = eventData.EventId,
                event_name = eventData.EventName
            };
        }

        public static EventDataContract FromTblEventToEventDataContract(tbl_event eventTblData)
        {
            if (eventTblData.id < 1 || string.IsNullOrEmpty(eventTblData.event_name))
                return null;

            return new EventDataContract {EventId = eventTblData.id, EventName = eventTblData.event_name};
        }

        public static bool Compare(EventDataContract obj1, EventDataContract obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;

            if (obj1 == null && obj2 != null)
                return false;

            if (obj1 != null && obj2 == null)
                return false;

            return obj1.EventId == obj2.EventId &&
                   string.Equals(obj1.EventName.Trim(), obj2.EventName.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}