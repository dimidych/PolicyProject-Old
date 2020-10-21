using System.ComponentModel;
using PolicyProjectManagementService;

namespace PolicyProjectManagementClient
{
    public class SelectedPolicy : PolicyDataContract, INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _isGroupPolitics;
        private string _policyParam;

        public string PolicyParam
        {
            get { return _policyParam; }
            set
            {
                _policyParam = value;
                NotifyPropertyChanged(nameof(PolicyParam));
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                NotifyPropertyChanged(nameof(IsSelected));
            }
        }

        public bool IsGroupPolitics
        {
            get { return _isGroupPolitics; }
            set
            {
                _isGroupPolitics = value;
                NotifyPropertyChanged(nameof(IsGroupPolitics));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}