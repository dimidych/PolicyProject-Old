using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class PolicySetViewModel : Notifier
    {
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private bool _isUserPolicySet;
        private bool _isGroupPolicySet;
        private ObservableCollection<SelectedPolicy> _policyCollection;
        private LoginDataContract _selectedLogin;
        private GroupDataContract _selectedGroup;
        private Dictionary<int, int[]> _groupIdAndPolicyIdDct;

        public PolicySetViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            _ppsClientViewModel.OnEmbeddedCollectionRefreshed += _ppsClientViewModel_OnEmbeddedCollectionRefreshed;
            SavePolicySetCmd = new RelayCommand(SavePolicySet);
            GetGroupIdAndPolicyIdDct();
            UpdateSelectedPolicyCollection();
            IsUserPolicySet = true;

            if (LoginCollection != null && LoginCollection.Any())
                SelectedLogin = LoginCollection.FirstOrDefault();
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

        public ObservableCollection<GroupDataContract> GroupCollection
        {
            get { return _ppsClientViewModel.GroupCollection; }
            set
            {
                _ppsClientViewModel.GroupCollection = value;
                NotifyPropertyChanged(nameof(GroupCollection));
            }
        }

        public ObservableCollection<SelectedPolicy> PolicyCollection
        {
            get { return _policyCollection; }
            set
            {
                _policyCollection = value;
                NotifyPropertyChanged(nameof(PolicyCollection));
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

                CheckPolicy(_selectedLogin, null);
                NotifyPropertyChanged(nameof(SelectedLogin));
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

                CheckPolicy(null, _selectedGroup);
                NotifyPropertyChanged(nameof(SelectedGroup));
            }
        }

        public bool IsUserPolicySet
        {
            get { return _isUserPolicySet; }
            set
            {
                if (_isUserPolicySet == value)
                    return;

                _isUserPolicySet = value;
                IsGroupPolicySet = !_isUserPolicySet;
                NotifyPropertyChanged(nameof(IsUserPolicySet));
            }
        }

        public bool IsGroupPolicySet
        {
            get { return _isGroupPolicySet; }
            set
            {
                if (_isGroupPolicySet == value)
                    return;

                _isGroupPolicySet = value;
                IsUserPolicySet = !_isGroupPolicySet;
                NotifyPropertyChanged(nameof(IsGroupPolicySet));
            }
        }

        public ICommand SavePolicySetCmd { get; private set; }

        private void _ppsClientViewModel_OnEmbeddedCollectionRefreshed(string collectionName)
        {
            NotifyPropertyChanged(collectionName);

            if (collectionName == nameof(PolicyCollection))
                UpdateSelectedPolicyCollection();
        }

        private void GetGroupIdAndPolicyIdDct()
        {
            try
            {
                using (var policySetService =
                    _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IPolicySetService>())
                {
                    var channel = policySetService.CreateChannel();
                    var result = channel.GetGroupIdAndPolicyIdDct();

                    if (!result.BoolRes)
                        throw new Exception(result.ErrorRes);

                    _groupIdAndPolicyIdDct = result.SomeResult;
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Что-то пошло не так - ", ex.Message));
            }
        }

        private void CheckPolicy(LoginDataContract selectedLogin, GroupDataContract selectedGroup)
        {
            try
            {
                UpdateSelectedPolicyCollection();

                if (PolicyCollection == null || !PolicyCollection.Any())
                    return;

                foreach (var selectedPolicy in PolicyCollection)
                    selectedPolicy.IsSelected = selectedPolicy.IsGroupPolitics = false;

                using (var policySetService =
                    _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IPolicySetService>())
                {
                    var channel = policySetService.CreateChannel();
                    Result<PolicySetDataContract[]> result;

                    if (IsUserPolicySet && selectedLogin != null)
                        result = channel.GetPolicySetForLogin(selectedLogin.LoginId);
                    else if (IsGroupPolicySet && selectedGroup != null)
                        result = channel.GetPolicySetForGroup(selectedGroup.GroupId);
                    else
                        return;

                    if (result == null)
                        throw new Exception("Ошибка вызова службы");

                    if (!result.BoolRes || result.SomeResult == null)
                        throw new Exception(result.ErrorRes);

                    var policySetCollection = result.SomeResult;

                    foreach (var selectedPolicy in PolicyCollection)
                    {
                        var policySet = policySetCollection.FirstOrDefault(x => x.PolicyId == selectedPolicy.PolicyId);
                        selectedPolicy.IsSelected = policySet != null;

                        if (selectedPolicy.IsSelected && policySet != null)
                            selectedPolicy.PolicyParam = policySet.PolicyParam;
                    }

                    if (!IsUserPolicySet || _groupIdAndPolicyIdDct == null || !_groupIdAndPolicyIdDct.Any())
                        return;

                    var grpPoliciesForLogin = _groupIdAndPolicyIdDct[selectedLogin.GroupId];

                    if (grpPoliciesForLogin == null || !grpPoliciesForLogin.Any())
                        return;

                    foreach (var selectedPolicy in PolicyCollection)
                        selectedPolicy.IsGroupPolitics = grpPoliciesForLogin.Any(x => x == selectedPolicy.PolicyId);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось отметить политику - ", ex.Message));
            }
        }

        private void UpdateSelectedPolicyCollection()
        {
            try
            {
                if (_ppsClientViewModel.PolicyCollection == null || !_ppsClientViewModel.PolicyCollection.Any())
                    return;

                PolicyCollection = new ObservableCollection<SelectedPolicy>();

                foreach (var policy in _ppsClientViewModel.PolicyCollection)
                    PolicyCollection.Add(new SelectedPolicy
                    {
                        PolicyId = policy.PolicyId,
                        PolicyName = policy.PolicyName,
                        PlatformId = policy.PlatformId,
                        PolicyInstruction = policy.PolicyInstruction,
                        PlatformName = policy.PlatformName,
                        PolicyParam = policy.PolicyDefaultParam,
                        IsSelected = false
                    });
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось получить список политик - ", ex.Message));
            }
        }

        private void SavePolicySet(object parameter)
        {
            var loginListToUpdate = new List<long>();

            try
            {
                if (IsUserPolicySet && SelectedLogin == null)
                    throw new Exception("Пользователь не выбран");

                if (IsGroupPolicySet && SelectedGroup == null)
                    throw new Exception("Группа не выбрана");

                using (var policySetService =
                    _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IPolicySetService>())
                {
                    var channel = policySetService.CreateChannel();
                    var groupId = IsGroupPolicySet ? SelectedGroup.GroupId : SelectedLogin.GroupId;
                    var groupPolicySet = channel.GetPolicySetForGroup(groupId).SomeResult;
                    Result<object> res = null;

                    if (IsGroupPolicySet)
                    {
                        loginListToUpdate =
                            LoginCollection.Where(x => x.GroupId == groupId).Select(x => x.LoginId).ToList();

                        foreach (var selectedPolicy in PolicyCollection)
                        {
                            res = null;
                            var containedGroup =
                                groupPolicySet.FirstOrDefault(x => x.PolicyId == selectedPolicy.PolicyId);

                            if (selectedPolicy.IsSelected)
                            {
                                if (containedGroup == null)
                                    res = channel.AddPolicySet(new PolicySetDataContract
                                    {
                                        PolicyId = selectedPolicy.PolicyId,
                                        GroupId = groupId,
                                        PolicyParam = selectedPolicy.PolicyParam
                                    });
                                else
                                    res =
                                        channel.UpdatePolicySet(
                                            new PolicySetDataContract {PolicySetId = containedGroup.PolicySetId},
                                            new PolicySetDataContract
                                            {
                                                PolicyId = selectedPolicy.PolicyId,
                                                GroupId = groupId,
                                                PolicyParam = selectedPolicy.PolicyParam
                                            });
                            }
                            else
                            {
                                if (containedGroup != null)
                                    res = channel.DeletePolicySet(new PolicySetDataContract
                                    {
                                        PolicySetId = containedGroup.PolicySetId,
                                        PolicyId = selectedPolicy.PolicyId,
                                        GroupId = groupId
                                    });
                            }

                            if (res != null && !res.BoolRes)
                                _ppsClientViewModel.WriteLogMessage(
                                    string.Concat("Не удалось сохранить набор политик - ", res.ErrorRes));
                        }

                        GetGroupIdAndPolicyIdDct();
                        _ppsClientViewModel.AddEvent(EventConstants.EVENT_ASSIGN_lOGIN_POLICY,
                            string.Concat("Для группы ", SelectedGroup.GroupName));
                    }
                    else
                    {
                        loginListToUpdate.Add(SelectedLogin.LoginId);
                        var policySetForLoginInTbl =
                            channel.GetPolicySet(new PolicySetDataContract {LoginId = SelectedLogin.LoginId}).SomeResult;

                        foreach (var selectedPolicy in PolicyCollection)
                        {
                            res = null;
                            var containedGroup =
                                groupPolicySet.FirstOrDefault(x => x.PolicyId == selectedPolicy.PolicyId);
                            var containedInTbl =
                                policySetForLoginInTbl.FirstOrDefault(x => x.PolicyId == selectedPolicy.PolicyId);

                            if (selectedPolicy.IsSelected)
                            {
                                if (containedGroup == null)
                                {
                                    if (containedInTbl != null)
                                        res = channel.UpdatePolicySet(new PolicySetDataContract
                                            {
                                                PolicySetId = containedInTbl.PolicySetId
                                            },
                                            new PolicySetDataContract
                                            {
                                                PolicySetId = containedInTbl.PolicySetId,
                                                PolicyId = selectedPolicy.PolicyId,
                                                LoginId = SelectedLogin.LoginId,
                                                Selected = selectedPolicy.IsSelected,
                                                PolicyParam = selectedPolicy.PolicyParam
                                            }
                                        );
                                    else
                                        res = channel.AddPolicySet(new PolicySetDataContract
                                        {
                                            PolicyId = selectedPolicy.PolicyId,
                                            LoginId = SelectedLogin.LoginId,
                                            Selected = selectedPolicy.IsSelected,
                                            PolicyParam = selectedPolicy.PolicyParam
                                        });
                                }
                                else
                                {
                                    if (containedInTbl != null)
                                        res = channel.DeletePolicySet(new PolicySetDataContract
                                        {
                                            PolicySetId = containedInTbl.PolicySetId
                                        });
                                }
                            }
                            else
                            {
                                if (containedGroup == null)
                                {
                                    if (containedInTbl != null)
                                        res = channel.DeletePolicySet(new PolicySetDataContract
                                        {
                                            PolicySetId = containedInTbl.PolicySetId
                                        });
                                }
                                else
                                {
                                    if (containedInTbl != null)
                                        res = channel.UpdatePolicySet(new PolicySetDataContract
                                            {
                                                PolicySetId = containedInTbl.PolicySetId
                                            },
                                            new PolicySetDataContract
                                            {
                                                PolicySetId = containedInTbl.PolicySetId,
                                                PolicyId = selectedPolicy.PolicyId,
                                                LoginId = SelectedLogin.LoginId,
                                                Selected = selectedPolicy.IsSelected,
                                                PolicyParam = selectedPolicy.PolicyParam
                                            }
                                        );
                                    else
                                        res = channel.AddPolicySet(new PolicySetDataContract
                                        {
                                            PolicyId = selectedPolicy.PolicyId,
                                            LoginId = SelectedLogin.LoginId,
                                            Selected = selectedPolicy.IsSelected,
                                            PolicyParam = selectedPolicy.PolicyParam
                                        });
                                }
                            }

                            if (res != null && !res.BoolRes)
                                _ppsClientViewModel.WriteLogMessage(
                                    string.Concat("Не удалось сохранить набор политик - ", res.ErrorRes));
                        }

                        _ppsClientViewModel.AddEvent(EventConstants.EVENT_ASSIGN_lOGIN_POLICY,
                            string.Concat("Для логина ", SelectedLogin.Login));
                    }
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Не удалось сохранить набор политик - ", ex.Message));
            }
            finally
            {
                SetLoginDevicesForUpdate(loginListToUpdate);
            }
        }

        private void SetLoginDevicesForUpdate(List<long> loginIdsList)
        {
            try
            {
                using (var loginDevicesService =
                    _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<ILoginDevicesService>())
                {
                    var channel = loginDevicesService.CreateChannel();
                    var updateResult = channel.SetDevicesForUpdate(loginIdsList);

                    if (updateResult != null && !updateResult.BoolRes)
                        throw new Exception(updateResult.ErrorRes);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage(string.Concat("Ошибка обновления устройств - ", ex.Message));
            }
            finally
            {
                _ppsClientViewModel.GetDeviceCollection();
            }
        }
    }
}