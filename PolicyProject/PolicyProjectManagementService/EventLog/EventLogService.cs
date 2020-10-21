using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.IEventLogService",
        Name = "PolicyProjectManagementService.EventLogService", OrderToLoad = 0)]
    public class EventLogService : IEventLogService
    {
        public Result<EventLogDataContract[]> GetEventLog(EventLogDataContract eventLogFilter)
        {
            var result = new Result<EventLogDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (eventLogFilter == null)
                        result.SomeResult =
                            ctx.tbl_activity_log.Select(
                                eventLogTblData => new EventLogDataContract
                                {
                                    EventLogId = eventLogTblData.id,
                                    EventLogDate = eventLogTblData.log_date,
                                    EventId = eventLogTblData.id_event,
                                    Login = eventLogTblData.login,
                                    Device = eventLogTblData.device,
                                    DocumentId = eventLogTblData.id_document,
                                    Message = eventLogTblData.message,
                                    EventName =
                                        ctx.tbl_event.FirstOrDefault(x => x.id == eventLogTblData.id_event).event_name
                                }).ToArray();
                    else
                        result.SomeResult = ctx.tbl_activity_log.Where(x => x.id == eventLogFilter.EventLogId)
                            .Select(eventLogTblData => new EventLogDataContract
                            {
                                EventLogId = eventLogTblData.id,
                                EventLogDate = eventLogTblData.log_date,
                                EventId = eventLogTblData.id_event,
                                Login = eventLogTblData.login,
                                Device = eventLogTblData.device,
                                DocumentId = eventLogTblData.id_document,
                                Message = eventLogTblData.message,
                                EventName =
                                        ctx.tbl_event.FirstOrDefault(x => x.id == eventLogTblData.id_event).event_name
                            })
                            .ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения лога событий. ", ex.Message);
            }

            return result;
        }

        public Result<EventLogDataContract[]> GetFilteredEventLog(DateTime fromDate, DateTime? toDate, int? eventId)
        {
            var result = new Result<EventLogDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    var finalDate = (toDate ?? DateTime.Now).AddDays(1);
                    var filteredLog = ctx.tbl_activity_log.Where(x => x.log_date >= fromDate &&
                                                                      x.log_date <= finalDate);

                    if (eventId != null)
                        filteredLog = filteredLog.Where(x => x.id_event == eventId);

                    var res = new List<EventLogDataContract>();

                    foreach (var activity in filteredLog)
                    {
                        var evntName = ctx.tbl_event.FirstOrDefault(x => x.id == activity.id_event).event_name;
                        res.Add(new EventLogDataContract
                        {
                            EventLogId = activity.id,
                            EventLogDate = activity.log_date,
                            EventId = activity.id_event,
                            Device = activity.device,
                            DocumentId = activity.id_document,
                            Message = activity.message,
                            Login = activity.login,
                            EventName = evntName
                        });
                    }

                    result.SomeResult = res.ToArray();
                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения лога событий. ", ex.Message);
            }

            return result;
        }

        public string GetEventLogRest(EventLogDataContract eventLogFilter)
        {
            var queryResult = GetEventLog(eventLogFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public string GetEventLogRestJson(string eventLogFilter)
        {
            var logFilter = JsonWorker.JsonWorker<EventLogDataContract>.Deserialize(eventLogFilter);
            var queryResult = GetEventLog(logFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public string GetFilteredEventLogRest(DateTime fromDate, DateTime? toDate, int? eventId)
        {
            var queryResult = GetFilteredEventLog(fromDate, toDate, eventId);
            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<object> AddEventLog(EventLogDataContract eventLog)
        {
            var result = new Result<object>();

            try
            {
                if (eventLog == null)
                    throw new Exception("Новая запись лога не задана");

                using (var ctx = new PolicyProjectEntities())
                {
                    eventLog.EventLogId = ctx.tbl_activity_log.Any() ? ctx.tbl_activity_log.Max(x => x.id) + 1 : 1;
                    var newEventLog = EventLogDataContract.FromEventLogDataContractToTblEventLog(eventLog);
                    newEventLog.log_date = DateTime.Now;
                    ctx.tbl_activity_log.Add(newEventLog);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка добавления записи лога. ", ex.Message);
            }

            return result;
        }

        public Result<object> AddEventLogExplicit(string device, string login, int eventId, long documentId,
            string message)
        {
            var result = new Result<object>();

            try
            {
                if (string.IsNullOrEmpty(device.Trim()) || string.IsNullOrEmpty(login.Trim()) || eventId < 1)
                    throw new Exception("Новая запись лога не задана");

                using (var ctx = new PolicyProjectEntities())
                {
                    var eventLog = new EventLogDataContract
                    {
                        EventLogId = ctx.tbl_activity_log.Any() ? ctx.tbl_activity_log.Max(x => x.id) + 1 : 1,
                        EventId = eventId,
                        DocumentId = documentId,
                        Message = message,
                        Login = login,
                        Device=device
                    };
                    var newEventLog = EventLogDataContract.FromEventLogDataContractToTblEventLog(eventLog);
                    newEventLog.log_date = DateTime.Now;
                    ctx.tbl_activity_log.Add(newEventLog);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка добавления записи лога. ", ex.Message);
            }

            return result;
        }

        public Result<object> DeleteEventLog(DateTime fromDate, DateTime? toDate, int? eventId)
        {
            var result = new Result<object>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    var finalDate = (toDate ?? DateTime.Now).AddDays(1);
                    var logForDelete = ctx.tbl_activity_log.Where(x => x.log_date >= fromDate
                                                                       && x.log_date <= finalDate);

                    if (eventId != null)
                        logForDelete = logForDelete.Where(x => x.id_event == eventId);

                    foreach (var log in logForDelete)
                        ctx.tbl_activity_log.Remove(log);

                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка удаления лога событий. ", ex.Message);
            }

            return result;
        }
    }
}