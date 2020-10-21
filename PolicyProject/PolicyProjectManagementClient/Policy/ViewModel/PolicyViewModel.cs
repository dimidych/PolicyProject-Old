using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using NotifyHelper;
using PolicyProjectManagementService;
using RelayCmd;

namespace PolicyProjectManagementClient
{
    public class PolicyViewModel : Notifier
    {
        private readonly IPpsClientViewModel _ppsClientViewModel;
        private PolicyDataContract _selectedPolicy;
        private PlatformDataContract _selectedPlatform;
        private string _policyName;
        private string _policyDefaultParam;
        private string _policyInstruction;

        public PolicyViewModel(IPpsClientViewModel ppsClientViewModel)
        {
            _ppsClientViewModel = ppsClientViewModel;
            _ppsClientViewModel.OnEmbeddedCollectionRefreshed += _ppsClientViewModel_OnEmbeddedCollectionRefreshed;
            AddPolicyCmd = new RelayCommand(AddPolicy);
            UpdatePolicyCmd = new RelayCommand(UpdatePolicy);
            DeletePolicyCmd = new RelayCommand(DeletePolicy);
        }

        public ObservableCollection<PolicyDataContract> PolicyCollection
        {
            get { return _ppsClientViewModel.PolicyCollection; }
            set
            {
                _ppsClientViewModel.PolicyCollection = value;
                NotifyPropertyChanged(nameof(PolicyCollection));
            }
        }

        public ObservableCollection<PlatformDataContract> PlatformCollection
        {
            get { return _ppsClientViewModel.PlatformCollection; }
            set
            {
                _ppsClientViewModel.PlatformCollection = value;
                NotifyPropertyChanged(nameof(PlatformCollection));
            }
        }

        public PolicyDataContract SelectedPolicy
        {
            get { return _selectedPolicy; }
            set
            {
                if (_selectedPolicy == value)
                    return;

                _selectedPolicy = value;

                if (_selectedPolicy == null)
                    return;

                PolicyName = _selectedPolicy.PolicyName;
                PolicyInstruction = _selectedPolicy.PolicyInstruction;
                PolicyDefaultParam = _selectedPolicy.PolicyDefaultParam;

                if (PlatformCollection != null && PlatformCollection.Any())
                    SelectedPlatform =
                        PlatformCollection.FirstOrDefault(x => x.PlatformId == _selectedPolicy.PlatformId) ??
                        PlatformCollection.FirstOrDefault();

                NotifyPropertyChanged(nameof(SelectedPolicy));
            }
        }

        public PlatformDataContract SelectedPlatform
        {
            get { return _selectedPlatform; }
            set
            {
                if (_selectedPlatform == value)
                    return;

                _selectedPlatform = value;

                if (_selectedPlatform == null)
                    return;

                NotifyPropertyChanged(nameof(SelectedPlatform));
            }
        }

        public string PolicyName
        {
            get { return _policyName; }
            set
            {
                _policyName = value;
                NotifyPropertyChanged(nameof(PolicyName));
            }
        }

        public string PolicyDefaultParam
        {
            get { return _policyDefaultParam; }
            set
            {
                _policyDefaultParam = value;
                NotifyPropertyChanged(nameof(PolicyDefaultParam));
            }
        }

        public string PolicyInstruction
        {
            get { return _policyInstruction; }
            set
            {
                _policyInstruction = value;
                NotifyPropertyChanged(nameof(PolicyInstruction));
            }
        }

        public ICommand AddPolicyCmd { get; private set; }

        public ICommand UpdatePolicyCmd { get; private set; }

        public ICommand DeletePolicyCmd { get; private set; }

        private void _ppsClientViewModel_OnEmbeddedCollectionRefreshed(string collectionName)
        {
            NotifyPropertyChanged(collectionName);
        }

        private void AddPolicy(object param)
        {
            try
            {
                if (string.IsNullOrEmpty(PolicyName.Trim()) || string.IsNullOrEmpty(PolicyInstruction.Trim()) ||
                    SelectedPlatform == null)
                    throw new Exception("Не заполнены обязательные поля");

                using (var policyService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IPolicyService>())
                {
                    var channel = policyService.CreateChannel();
                    var newPolicy = new PolicyDataContract
                    {
                        PlatformId = SelectedPlatform.PlatformId,
                        PolicyInstruction = PolicyInstruction.Trim(),
                        PolicyName = PolicyName.Trim(),
                        PolicyDefaultParam = PolicyDefaultParam.Trim()
                    };
                    var result = channel.AddPolicy(newPolicy);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdatePolicyCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_ADD_POLICY, newPolicy.PolicyName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось добавить политику - " + ex.Message);
            }
        }

        private void UpdatePolicy(object param)
        {
            try
            {
                if (SelectedPolicy == null)
                    throw new Exception("Политика не выбрана");

                if (string.IsNullOrEmpty(PolicyName.Trim()) || SelectedPlatform == null)
                    throw new Exception("Не заполнены обязательные поля");

                using (var policyService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IPolicyService>())
                {
                    var channel = policyService.CreateChannel();
                    var policyName = PolicyName;
                    var result = channel.UpdatePolicy(SelectedPolicy, new PolicyDataContract
                    {
                        PolicyId = SelectedPolicy.PolicyId,
                        PlatformId = SelectedPlatform.PlatformId,
                        PolicyInstruction = PolicyInstruction.Trim(),
                        PolicyName = PolicyName.Trim(),
                        PolicyDefaultParam = PolicyDefaultParam.Trim()
                    });

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdatePolicyCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_UPD_POLICY, policyName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось изменить политику - " + ex.Message);
            }
        }

        private void DeletePolicy(object param)
        {
            try
            {
                if (SelectedPolicy == null)
                    throw new Exception("Политика не выбрана");

                using (var policyService = _ppsClientViewModel.ServiceProxy.GetPpsChannelFactory<IPolicyService>())
                {
                    var channel = policyService.CreateChannel();
                    var result = channel.DeletePolicy(SelectedPolicy);

                    if (!result.BoolRes || !string.IsNullOrEmpty(result.ErrorRes))
                        throw new Exception(result.ErrorRes);

                    _ppsClientViewModel.UpdatePolicyCollection();
                    _ppsClientViewModel.AddEvent(EventConstants.EVENT_DEL_POLICY, SelectedPolicy.PolicyName);
                }
            }
            catch (Exception ex)
            {
                _ppsClientViewModel.WriteLogMessage("Не удалось удалить политику - " + ex.Message);
            }
        }
    }
}