namespace FileListViewTest
{
  using System.Windows;
  using FileListViewTest.ViewModels;

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      this.InitializeComponent();

      this.DataContext = new ApplicationViewModel();
    }

    protected override void OnClosed(System.EventArgs e)
    {
      ApplicationViewModel app = this.DataContext as ApplicationViewModel;

      if (app != null)
        app.ApplicationClosed();

      base.OnClosed(e);
    }
  }
}
