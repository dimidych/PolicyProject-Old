using System.Windows;

namespace PolicyProjectManagementClient
{
    public partial class StartWindow : Window, IPasswordContainer
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        public string HashedPassword { get; private set; }

        string IPasswordContainer.HashedElsePassword { get; }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            HashedPassword = Hasher.Hash(PwdBox.SecurePassword);
            PwdBox.Clear();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility != Visibility.Collapsed)
                return;

            Hide();
            var mainWnd=new MainWindow();
            mainWnd.Closed += MainWnd_Closed;
            mainWnd.Show();
        }

        private void MainWnd_Closed(object sender, System.EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
