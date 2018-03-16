namespace ExplorerTest
{
    using System.Windows;
    using ExplorerTestLib.ViewModels;
    using FileSystemModels;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            var appVM = new ApplicationViewModel();
            this.DataContext = appVM;

            var newPath = PathFactory.SysDefault;
            appVM.InitializeViewModel(newPath);
        }

        protected override void OnClosed(System.EventArgs e)
        {
            var app = this.DataContext as System.IDisposable;

            if (app != null)
                app.Dispose();

            base.OnClosed(e);
        }
    }
}
