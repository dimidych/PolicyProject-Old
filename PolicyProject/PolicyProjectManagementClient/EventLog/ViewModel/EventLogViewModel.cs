using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class EventLogViewModel : Notifier
    {
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private DateTime _dateFrom;
        private DateTime _dateTo;
        private EventDataContract _selectedEvent;

        public EventLogViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            _ppsClientViewModel.OnEmbeddedCollectionRefreshed += _ppsClientViewModel_OnEmbeddedCollectionRefreshed;
            DateFrom = DateTo = DateTime.Now;
            FilterLogCmd = new RelayCommand(FilterLog);
            DeleteLogByFilterCmd = new RelayCommand(DeleteLogByFilter);
            DeleteAllLogCmd = new RelayCommand(DeleteAllLog);
        }

        public ObservableCollection<EventLogDataContract> EventLogCollection
        {
            get { return _ppsClientViewModel.EventLogCollection; }
            set
            {
                _ppsClientViewModel.EventLogCollection = value;
                NotifyPropertyChanged(nameof(EventLogCollection));
            }
        }

        public ObservableCollection<EventDataContract> EventCollection
        {
            get { return _ppsClientViewModel.EventCollection; }
            set
            {
                _ppsClientViewModel.EventCollection = value;
                NotifyPropertyChanged(nameof(EventCollection));
            }
        }

        public EventDataContract SelectedEvent
        {
            get { return _selectedEvent; }
            set
            {
                if (_selectedEvent == value)
                    return;

                _selectedEvent = value;

                if (_selectedEvent == null)
                    return;

                NotifyPropertyChanged(nameof(SelectedEvent));
            }
        }

        public DateTime DateFrom
        {
            get { return _dateFrom; }
            set
            {
                _dateFrom = value;
                NotifyPropertyChanged(nameof(DateFrom));
            }
        }

        public DateTime DateTo
        {
            get { return _dateTo; }
            set
            {
                _dateTo = value;
                NotifyPropertyChanged(nameof(DateTo));
            }
        }

        public ICommand FilterLogCmd { get; private set; }

        public ICommand DeleteLogByFilterCmd { get; private set; }

        public ICommand DeleteAllLogCmd { get; private set; }

        private void _ppsClientViewModel_OnEmbeddedCollectionRefreshed(string collectionName)
        {
            NotifyPropertyChanged(collectionName);
        }

        private void FilterLog(object parameter)
        {
            try
            {
                using (var eventLogService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IEventLogService>())
                {
                    var channel = eventLogService.CreateChannel();
                    var result = channel.GetFilteredEventLog(DateFrom, DateTo, null);

                    if (result.BoolRes && result.SomeResult != null)
                        EventLogCollection = new ObservableCollection<EventLogDataContract>(result.SomeResult);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось прочитать лог событий - " + ex.Message);
            }
        }

        private void DeleteLogByFilter(object parameter)
        {
            try
            {
                using (var eventLogService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IEventLogService>())
                {
                    var channel = eventLogService.CreateChannel();
                    var result = channel.DeleteEventLog(DateFrom, DateTo, null);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_DEL_LOG,
                        string.Concat("Удаление лога с ", DateFrom, " по ", DateTo));
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось удалить лог событий - " + ex.Message);
            }
        }

        private void DeleteAllLog(object parameter)
        {
            try
            {
                using (var eventLogService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IEventLogService>())
                {
                    var channel = eventLogService.CreateChannel();
                    var result = channel.DeleteEventLog(DateTime.MinValue.AddDays(1), DateTime.MaxValue.AddDays(-1), null);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_DEL_LOG, "Удаление всего лога");
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось удалить лог событий - " + ex.Message);
            }
        }
    }
}