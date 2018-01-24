namespace TestFolderBrowser
{
  using System.Windows;
  using FolderBrowser.ViewModels;

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      this.InitializeComponent();
    }

    /// <summary>
    /// Use a button click event to demo the folder browser dialog...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      var dlg = new FolderBrowser.Views.FolderBrowserDialog();

      var dlgViewModel = new DialogViewModel(new BrowserViewModel());
      dlgViewModel.TreeBrowser.SetSelectedFolder(@"C:\");

      dlg.DataContext = dlgViewModel;

      bool? bResult = dlg.ShowDialog();

      if (dlgViewModel.DialogCloseResult == true || bResult == true)
        System.Windows.MessageBox.Show("OPening path:" + dlgViewModel.TreeBrowser.SelectedFolder);
    }
  }
}
