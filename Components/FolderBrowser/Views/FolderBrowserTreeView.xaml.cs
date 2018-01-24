namespace FolderBrowser.Views
{
    using FolderBrowser.Interfaces;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for FolderBrowserTreeView.xaml
    /// </summary>
    public partial class FolderBrowserTreeView : UserControl
    {
        /// <summary>
        /// Standard class constructor
        /// </summary>
        public FolderBrowserTreeView()
        {
            InitializeComponent();
            Loaded += FolderBrowserTreeView_Loaded;
        }

        /// <summary>
        /// Initializes the folder browser viewmodel and view as soon as the view is loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderBrowserTreeView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= FolderBrowserTreeView_Loaded;

            var vm = DataContext as IBrowserViewModel;

            if (vm != null)
                vm.BrowsePath(vm.InitialPath);
        }
    }
}
