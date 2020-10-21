using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class UsersViewModel : Notifier
    {
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private string _firstName;
        private string _lastName;
        private string _middleName;
        private UserDataContract _selectedUser;

        public UsersViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            _ppsClientViewModel.OnEmbeddedCollectionRefreshed += _ppsClientViewModel_OnEmbeddedCollectionRefreshed;
            AddUserCmd = new RelayCommand(AddUser);
            UpdateUserCmd = new RelayCommand(UpdateUser);
            DeleteUserCmd = new RelayCommand(DeleteUser);
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                NotifyPropertyChanged(nameof(LastName));
            }
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                NotifyPropertyChanged(nameof(FirstName));
            }
        }

        public string MiddleName
        {
            get { return _middleName; }
            set
            {
                _middleName = value;
                NotifyPropertyChanged(nameof(MiddleName));
            }
        }

        public ObservableCollection<UserDataContract> UserCollection
        {
            get { return _ppsClientViewModel.UserCollection; }
            set
            {
                _ppsClientViewModel.UserCollection = value;
                NotifyPropertyChanged(nameof(UserCollection));
            }
        }

        public UserDataContract SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (_selectedUser == value)
                    return;

                _selectedUser = value;

                if (_selectedUser == null)
                    return;

                LastName = _selectedUser.UserLastName;
                FirstName = _selectedUser.UserFirstName;
                MiddleName = _selectedUser.UserMiddleName;
                NotifyPropertyChanged(nameof(SelectedUser));
            }
        }

        public ICommand AddUserCmd { get; private set; }

        public ICommand UpdateUserCmd { get; private set; }

        public ICommand DeleteUserCmd { get; private set; }

        private void _ppsClientViewModel_OnEmbeddedCollectionRefreshed(string collectionName)
        {
            NotifyPropertyChanged(collectionName);
        }

        private void AddUser(object parameter)
        {
            try
            {
                if (string.IsNullOrEmpty(FirstName.Trim()))
                    throw new Exception("Необходимо заполнить хотя бы фамилию");

                using (var userService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IUserService>())
                {
                    var channel = userService.CreateChannel();
                    var newUser = new UserDataContract
                    {
                        UserFirstName = FirstName,
                        UserLastName = LastName,
                        UserMiddleName = MiddleName
                    };
                    var result = channel.AddUser(newUser);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateUserCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_ADD_USER, newUser.UserName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось добавить пользователя - " + ex.Message);
            }
        }

        private void UpdateUser(object parameter)
        {
            try
            {
                if (SelectedUser == null)
                    throw new Exception("Необходимо выбрать пользователя");

                if (string.IsNullOrEmpty(FirstName.Trim()))
                    throw new Exception("Необходимо заполнить хотя бы фамилию");

                using (var userService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IUserService>())
                {
                    var channel = userService.CreateChannel();
                    var result = channel.UpdateUser(SelectedUser,
                        new UserDataContract
                        {
                            UserId = SelectedUser.UserId,
                            UserLastName = LastName,
                            UserFirstName = FirstName,
                            UserMiddleName = MiddleName
                        });

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateUserCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_UPD_USER, SelectedUser.UserName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось изменить пользователя - " + ex.Message);
            }
        }

        private void DeleteUser(object parameter)
        {
            try
            {
                if (SelectedUser == null)
                    throw new Exception("Необходимо выбрать пользователя");

                using (var userService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IUserService>())
                {
                    var channel = userService.CreateChannel();
                    var result = channel.DeleteUser(SelectedUser);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateUserCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_DEL_USER, SelectedUser.UserName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось удалить пользователя - " + ex.Message);
            }
        }
    }
}