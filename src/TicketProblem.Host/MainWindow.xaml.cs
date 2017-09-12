namespace TicketProblem.Host
{
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainViewModel();
        }

        private void LaunchOnGitHub(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ElijahReva/ticket-problem");
        }
    }
}
