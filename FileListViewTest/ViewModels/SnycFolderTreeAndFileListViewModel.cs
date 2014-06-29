namespace FileListViewTest.ViewModels
{
  using FileListView.ViewModels;
  using FileListView.ViewModels.Interfaces;
  using FolderBrowser.ViewModels;
  using FolderBrowser.ViewModels.Interfaces;

  /// <summary>
  /// ViewModel to test synchronized display of folder treeview and filelistview.
  /// </summary>
  public class SnycFolderTreeAndFileListViewModel : Base.ViewModelBase
  {
    #region fields
    #endregion fields

    #region constructor
    /// <summary>
    /// Class constructor
    /// </summary>
    public SnycFolderTreeAndFileListViewModel()
    {
      // Construct synchronized folder and tree browser views
      this.SynchronizedFolderView = new FolderListViewModel();

      this.SynchronizedFolderView.AddRecentFolder(@"C:\temp\");
      this.SynchronizedFolderView.AddRecentFolder(@"C:\windows\test.txt");

      this.SynchronizedFolderView.AddFilter("Executeable files", "*.exe;*.bat");
      this.SynchronizedFolderView.AddFilter("Image files", "*.png;*.jpg;*.jpeg");
      this.SynchronizedFolderView.AddFilter("LaTex files", "*.tex");
      this.SynchronizedFolderView.AddFilter("Text files", "*.txt");
      this.SynchronizedFolderView.AddFilter("All Files", "*.*");

      this.SynchronizedTreeBrowser = new BrowserViewModel();
      this.SynchronizedTreeBrowser.SetSpecialFoldersVisibility(false);

      this.SynchronizedFolderView.AttachFolderBrowser(this.SynchronizedTreeBrowser);
    }
    #endregion constructor

    #region properties
    public IFolderListViewModel SynchronizedFolderView { get; set; }

    /// <summary>
    /// Gets the viewmodel that drives the folder picker control.
    /// </summary>
    public IBrowserViewModel SynchronizedTreeBrowser { get; private set; }
    #endregion properties

    #region methods
    #endregion methods
  }
}
