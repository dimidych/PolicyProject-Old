using System;
using System.Linq;
using Newtonsoft.Json;
using ServiceRealizationAtribute;

namespace PolicyProjectManagementService
{
    [ServiceRealization(
        InterfaceName = "PolicyProjectManagementService.IEventService",
        Name = "PolicyProjectManagementService.EventService", OrderToLoad = 0)]
    public class EventService : IEventService
    {
        public Result<EventDataContract[]> GetEvent(EventDataContract eventFilter)
        {
            var result = new Result<EventDataContract[]>();

            try
            {
                using (var ctx = new PolicyProjectEntities())
                {
                    if (eventFilter == null)
                        result.SomeResult =
                            ctx.tbl_event.Select(
                                eventTblData =>
                                    new EventDataContract
                                    {
                                        EventId = eventTblData.id,
                                        EventName = eventTblData.event_name
                                    }).ToArray();
                    else
                        result.SomeResult = ctx.tbl_event.Where(x => x.id == eventFilter.EventId)
                            .Select(eventTblData =>
                                new EventDataContract
                                {
                                    EventId = eventTblData.id,
                                    EventName = eventTblData.event_name
                                })
                            .ToArray();

                    result.BoolRes = true;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка получения списка событий. ", ex.Message);
            }

            return result;
        }

        public string GetEventRest(EventDataContract eventFilter)
        {
            var queryResult = GetEvent(eventFilter);
            return JsonConvert.SerializeObject(queryResult);
        }

        public Result<object> AddEvent(EventDataContract evnt)
        {
            var result = new Result<object>();

            try
            {
                if (evnt == null)
                    throw new Exception("Новое событие не задано");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected =
                        ctx.tbl_event.FirstOrDefault(
                            x =>
                                x.event_name.Trim()
                                    .Equals(evnt.EventName.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (selected != null)
                        throw new Exception("Событие уже существует");

                    evnt.EventId = ctx.tbl_event.Any() ? ctx.tbl_event.Max(x => x.id) + 1 : 1;
                    var newEvent = EventDataContract.FromEventDataContractToTblEvent(evnt);
                    ctx.tbl_event.Add(newEvent);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка добавления события. ", ex.Message);
            }

            return result;
        }

        public Result<object> UpdateEvent(EventDataContract oldEvent, EventDataContract newEvent)
        {
            var result = new Result<object>();

            try
            {
                if (oldEvent == null)
                    throw new Exception("Текущее событие не задано");

                if (newEvent == null)
                    throw new Exception("Новое событие не задано");

                using (var ctx = new PolicyProjectEntities())
                {
                    var selected = ctx.tbl_event.FirstOrDefault(x => x.id == oldEvent.EventId);

                    if (selected == null)
                        throw new Exception("Текущее событие не найдено");

                    selected.event_name = newEvent.EventName;
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка изменения события. ", ex.Message);
            }

            return result;
        }

        public Result<object> DeleteEvent(EventDataContract evnt)
        {
            var result = new Result<object>();

            try
            {
                if (evnt == null)
                    throw new Exception("Событие не задано");

                using (var ctx = new PolicyProjectEntities())
                {
                    var delEvent = ctx.tbl_event.FirstOrDefault(x => x.id == evnt.EventId);
                    ctx.tbl_event.Remove(delEvent);
                    result.BoolRes = ctx.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                result.BoolRes = false;
                result.ErrorRes = string.Concat("Ошибка удаления события. ", ex.Message);
            }

            return result;
        }
    }
}