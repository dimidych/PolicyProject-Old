using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class LoginViewModel : Notifier
    {
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private string _login;
        private LoginDataContract _selectedLogin;
        private UserDataContract _selectedUser;
        private GroupDataContract _selectedGroup;

        public LoginViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            _ppsClientViewModel.OnEmbeddedCollectionRefreshed += _ppsClientViewModel_OnEmbeddedCollectionRefreshed;
            AddLoginCmd = new RelayCommand(AddLogin);
            UpdateLoginCmd = new RelayCommand(UpdateLogin);
            DeleteLoginCmd = new RelayCommand(DeleteLogin);
        }

        public ObservableCollection<LoginDataContract> LoginCollection
        {
            get { return _ppsClientViewModel.LoginCollection; }
            set
            {
                _ppsClientViewModel.LoginCollection = value;
                NotifyPropertyChanged(nameof(LoginCollection));
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

        public ObservableCollection<GroupDataContract> GroupCollection
        {
            get { return _ppsClientViewModel.GroupCollection; }
            set
            {
                _ppsClientViewModel.GroupCollection = value;
                NotifyPropertyChanged(nameof(GroupCollection));
            }
        }

        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                NotifyPropertyChanged(nameof(Login));
            }
        }

        public LoginDataContract SelectedLogin
        {
            get { return _selectedLogin; }
            set
            {
                if (_selectedLogin == value)
                    return;

                _selectedLogin = value;

                if (_selectedLogin == null)
                    return;

                Login = _selectedLogin.Login;

                if (UserCollection != null && UserCollection.Any())
                    SelectedUser = UserCollection.FirstOrDefault(x => x.UserId == _selectedLogin.UserId) ??
                                   UserCollection.FirstOrDefault();

                if (GroupCollection != null && GroupCollection.Any())
                    SelectedGroup = GroupCollection.FirstOrDefault(x => x.GroupId == _selectedLogin.GroupId) ??
                                    GroupCollection.FirstOrDefault();

                NotifyPropertyChanged(nameof(SelectedLogin));
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

                NotifyPropertyChanged(nameof(SelectedUser));
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

                if (_selectedGroup == null)
                    return;

                NotifyPropertyChanged(nameof(SelectedGroup));
            }
        }

        public ICommand AddLoginCmd { get; private set; }

        public ICommand UpdateLoginCmd { get; private set; }

        public ICommand DeleteLoginCmd { get; private set; }

        private void _ppsClientViewModel_OnEmbeddedCollectionRefreshed(string collectionName)
        {
            NotifyPropertyChanged(collectionName);
        }

        private void AddLogin(object parameter)
        {
            try
            {
                if (string.IsNullOrEmpty(Login.Trim()) || SelectedUser == null || SelectedGroup == null)
                    throw new Exception("Не заполнены обязательные поля");

                var passwordContainer = parameter as IPasswordContainer;

                if (string.Equals(passwordContainer?.HashedPassword, Hasher.Hash(null)) ||
                    string.Equals(passwordContainer?.HashedElsePassword, Hasher.Hash(null)))
                    throw new Exception("Пароль не задан");

                if (!string.Equals(passwordContainer?.HashedPassword, passwordContainer?.HashedElsePassword))
                    throw new Exception("Пароли не совпадают");

                using (var loginService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<ILoginService>())
                {
                    var channel = loginService.CreateChannel();
                    var newLogin = new LoginDataContract
                    {
                        Login = Login.Trim(),
                        Password = passwordContainer?.HashedPassword,
                        GroupId = SelectedGroup.GroupId,
                        UserId = SelectedUser.UserId
                    };
                    var result = channel.AddLogin(newLogin);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateLoginCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_ADD_LOGIN, newLogin.Login);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось добавить логин - ", ex.Message));
            }
        }

        private void UpdateLogin(object parameter)
        {
            try
            {
                if (SelectedLogin == null)
                    throw new Exception("Логин не выбран");

                if (string.IsNullOrEmpty(Login.Trim()) || SelectedUser == null || SelectedGroup == null)
                    throw new Exception("Не заполнены обязательные поля");

                var passwordContainer = parameter as IPasswordContainer;

                if (!string.Equals(passwordContainer?.HashedPassword, Hasher.Hash(null)) &&
                    !string.Equals(passwordContainer?.HashedPassword, passwordContainer?.HashedElsePassword))
                    throw new Exception("Пароли не совпадают");

                using (var loginService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<ILoginService>())
                {
                    var channel = loginService.CreateChannel();
                    var changedLogin = new LoginDataContract
                    {
                        LoginId = SelectedLogin.LoginId,
                        Login = Login.Trim(),
                        Password = SelectedLogin.Password,
                        GroupId = SelectedGroup.GroupId,
                        UserId = SelectedUser.UserId
                    };

                    if (!string.Equals(passwordContainer?.HashedPassword, Hasher.Hash(null)))
                        changedLogin.Password = passwordContainer?.HashedPassword;

                    var result = channel.UpdateLogin(SelectedLogin, changedLogin);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateLoginCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_UPD_LOGIN, SelectedLogin.Login);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось изменить логин - ", ex.Message));
            }
        }

        private void DeleteLogin(object parameter)
        {
            try
            {
                if (SelectedLogin == null)
                    throw new Exception("Логин не выбран");

                using (var loginService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<ILoginService>())
                {
                    var channel = loginService.CreateChannel();
                    var result = channel.DeleteLogin(SelectedLogin);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdateLoginCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_DEL_LOGIN, SelectedLogin.Login);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось удалить логин - ", ex.Message));
            }
        }
    }
}