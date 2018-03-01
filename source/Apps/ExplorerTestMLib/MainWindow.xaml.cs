namespace ExplorerTestMLib
{
    using ExplorerTestMLib.ViewModels;
    using FileSystemModels;
    using FileSystemModels.Models.FSItems.Base;
    using Settings.UserProfile;
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MWindowLib.SimpleMetroWindow
                                     , IViewSize  // Implements saving and loading/repositioning of Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WeakEventManager<Window, EventArgs>.AddHandler(this, "Loaded", MainWindow_Loaded);
        }

        private void MainWindow_Loaded(object sender, EventArgs e)
        {
            var viewModel = this.DataContext as AppViewModel;

            var newPath = PathFactory.SysDefault;
            viewModel.Demo.InitializeViewModel(newPath);
        }
    }
}
