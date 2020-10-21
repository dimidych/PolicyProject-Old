using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class GroupsViewModel : Notifier
    {
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private string _groupName;
        private GroupDataContract _selectedGroup;

        public GroupsViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            _ppsClientViewModel.OnEmbeddedCollectionRefreshed += _ppsClientViewModel_OnEmbeddedCollectionRefreshed;
            AddGroupCmd = new RelayCommand(AddGroup);
            UpdateGroupCmd = new RelayCommand(UpdateGroup);
            DeleteGroupCmd = new RelayCommand(DeleteGroup);
        }

        public string GroupName
        {
            get { return _groupName; }
            set
            {
                _groupName = value;
                NotifyPropertyChanged(nameof(GroupName));
            }
        }

        public ObservableCollection<GroupDataContract> GroupCollection
        {
            get { return _ppsClientViewModel.GroupCollection; }
            set
            {
                _ppsClientViewModel.GroupCollection = value;
                NotifyPropertyChanged(nameof(GroupCollection));
            }
        }

        public GroupDataContract SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (_selectedGroup == value)
                    return;

                _selectedGroup = value;

                if(_selectedGroup == null)
                    return;

                GroupName = _selectedGroup.GroupName;
                NotifyPropertyChanged(nameof(SelectedGroup));
            }
        }

        public ICommand AddGroupCmd { get; private set; }

        public ICommand UpdateGroupCmd { get; private set; }

        public ICommand DeleteGroupCmd { get; private set; }

        private void _ppsClientViewModel_OnEmbeddedCollectionRefreshed(string collectionName)
        {
            NotifyPropertyChanged(collectionName);
        }

        private void AddGroup(object parameter)
        {
            try
            {
                using (var groupService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IGroupService>())
                {
                    var channel = groupService.CreateChannel();
                    var result = channel.AddGroup(new GroupDataContract {GroupName = GroupName});

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateGroupCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_ADD_GROUP, GroupName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось добавить группу - " + ex.Message);
            }
        }

        private void UpdateGroup(object parameter)
        {
            try
            {
                using (var groupService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IGroupService>())
                {
                    var channel = groupService.CreateChannel();
                    var result = channel.UpdateGroup(SelectedGroup, new GroupDataContract {GroupName = GroupName});

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateGroupCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_UPD_GROUP, GroupName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось изменить группу - " + ex.Message);
            }
        }

        private void DeleteGroup(object parameter)
        {
            try
            {
                using (var groupService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IGroupService>())
                {
                    var channel = groupService.CreateChannel();
                    var result = channel.DeleteGroup(SelectedGroup);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateGroupCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_DEL_GROUP, GroupName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось удалить группу - " + ex.Message);
            }
        }
    }
}