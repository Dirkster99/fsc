namespace ExplorerTest
{
    using System.Windows;
    using ExplorerTestLib.ViewModels;
    using FileSystemModels;
    using FileSystemModels.Models.FSItems.Base;

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

        protected override void OnClosed(System.EventArgs e)
        {
            ApplicationViewModel app = this.DataContext as ApplicationViewModel;

            if (app != null)
                app.ApplicationClosed();

            base.OnClosed(e);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;

            var appVM = new ApplicationViewModel();
            this.DataContext = appVM;

            var newPath = PathFactory.SysDefault;
            appVM.InitializeViewModel(newPath);
        }
    }
}
