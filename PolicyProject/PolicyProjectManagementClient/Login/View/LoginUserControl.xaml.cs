using System.Windows.Controls;

namespace PolicyProjectManagementClient
{
    public partial class LoginUserControl : UserControl, IPasswordContainer
    {
        public string HashedPassword { get; private set; }

        public string HashedElsePassword { get; private set; }

        public LoginUserControl()
        {
            InitializeComponent();
        }

        private void ClearPassword()
        {
            PwdBox.Clear();
            ElsePwdBox.Clear();
        }

        private void SetPassword()
        {
            HashedPassword = Hasher.Hash(PwdBox.SecurePassword);
            HashedElsePassword = Hasher.Hash(ElsePwdBox.SecurePassword);
        }

        private void BtnAddLogin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetPassword();
            ClearPassword();
        }

        private void BtnUpdateLogin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SetPassword();
            ClearPassword();
        }
    }
}