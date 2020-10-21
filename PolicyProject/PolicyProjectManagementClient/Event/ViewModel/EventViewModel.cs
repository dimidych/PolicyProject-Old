using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class EventViewModel : Notifier
    {
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private string _eventName;
        private EventDataContract _selectedEvent;

        public EventViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            _ppsClientViewModel.OnEmbeddedCollectionRefreshed += _ppsClientViewModel_OnEmbeddedCollectionRefreshed;
            AddEventCmd = new RelayCommand(AddEvent);
            UpdateEventCmd = new RelayCommand(UpdateEvent);
            DeleteEventCmd = new RelayCommand(DeleteEvent);
        }

        public string EventName
        {
            get { return _eventName; }
            set
            {
                _eventName = value;
                NotifyPropertyChanged(nameof(EventName));
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

                EventName = _selectedEvent.EventName;
                NotifyPropertyChanged(nameof(SelectedEvent));
            }
        }

        public ICommand AddEventCmd { get; private set; }

        public ICommand UpdateEventCmd { get; private set; }

        public ICommand DeleteEventCmd { get; private set; }

        private void _ppsClientViewModel_OnEmbeddedCollectionRefreshed(string collectionName)
        {
            NotifyPropertyChanged(collectionName);
        }

        private void AddEvent(object parameter)
        {
            try
            {
                using (var eventService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IEventService>())
                {
                    var channel = eventService.CreateChannel();
                    var result = channel.AddEvent(new EventDataContract {EventName = EventName});

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateEventCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_ADD_EVENT, EventName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось добавить событие - " + ex.Message);
            }
        }

        private void UpdateEvent(object parameter)
        {
            try
            {
                using (var eventService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IEventService>())
                {
                    var channel = eventService.CreateChannel();
                    var result = channel.UpdateEvent(SelectedEvent, new EventDataContract {EventName = EventName});

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateEventCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_UPD_EVENT, EventName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось изменить событие - " + ex.Message);
            }
        }

        private void DeleteEvent(object parameter)
        {
            try
            {
                using (var eventService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IEventService>())
                {
                    var channel = eventService.CreateChannel();
                    var result = channel.DeleteEvent(SelectedEvent);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateEventCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_DEL_EVENT, EventName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось удалить событие - " + ex.Message);
            }
        }
    }
}