namespace FileListViewTest.ViewModels
{
  using System.Collections.Generic;
  using System.Windows.Controls;
  using System.Windows.Input;
  using FileListView.ViewModels;
  using FileListView.ViewModels.Interfaces;
  using FileListViewTest.Command;
  using FolderBrowser.ViewModels;
  using FolderBrowser.ViewModels.Interfaces;

  /// <summary>
  /// Class implements an application viewmodel that manages the test application.
  /// </summary>
  public class ApplicationViewModel : FileListViewTest.ViewModels.Base.ViewModelBase
  {
    #region fields
    private RelayCommand<object> mAddRecentFolder;
    private RelayCommand<object> mRemoveRecentFolder;
    #endregion fields

    #region constructor
    /// <summary>
    /// Class constructor
    /// </summary>
    public ApplicationViewModel()
    {
      this.FolderView = new FolderListViewModel();

      this.FolderView.AddRecentFolder(@"C:\temp\");
      this.FolderView.AddRecentFolder(@"C:\windows\test.txt", true);
      
      this.FolderView.AddFilter("Executeable files", "*.exe;*.bat");
      this.FolderView.AddFilter("Image files", "*.png;*.jpg;*.jpeg");
      this.FolderView.AddFilter("LaTex files", "*.tex");
      this.FolderView.AddFilter("Text files", "*.txt");
      this.FolderView.AddFilter("All Files", "*.*");

      this.SyncFolderTreeFileListTest = new SnycFolderTreeAndFileListViewModel();

      this.SyncFolderTreeFileListTest.SynchronizedFolderView.NavigateToFolder(@"G:\Myfiles\");
    }
    #endregion constructor

    #region properties
    /// <summary>
    /// Gets a test viewmodel that contains a synchronized treeview
    /// and listview to work with file system items.
    /// </summary>
    public SnycFolderTreeAndFileListViewModel SyncFolderTreeFileListTest { get; private set; }

    /// <summary>
    /// Expose a viewmodel that controls the combobox folder drop down
    /// and the fodler/file list view.
    /// </summary>
    public IFolderListViewModel FolderView { get; set; }

    #region Commands for test case without folderBrowser
    /// <summary>
    /// Add a folder to the list of recent folders.
    /// </summary>
    public ICommand AddRecentFolder
    {
      get
      {
        if (this.mAddRecentFolder == null)
          this.mAddRecentFolder = new RelayCommand<object>((p) =>
          {
            this.AddRecentFolder_Executed(p);
          });

        return this.mAddRecentFolder;
      }
    }

    /// <summary>
    /// Remove a folder from the list of recent folders.
    /// </summary>
    public ICommand RemoveRecentFolder
    {
      get
      {
        if (this.mRemoveRecentFolder == null)
          this.mRemoveRecentFolder = new RelayCommand<object>(
               (p) => this.RemoveRecentFolder_Executed(p),
               (p) => this.FolderView.SelectedRecentLocation != null);

        return this.mRemoveRecentFolder;
      }
    }
    #endregion Commands for test case without folderBrowser
    #endregion properties

    #region methods
    /// <summary>
    /// Free resources (if any) when application exits.
    /// </summary>
    internal void ApplicationClosed()
    {
      if (this.FolderView != null)
      {
        this.FolderView.DetachFolderBrowser();
      }
    }

    private void AddRecentFolder_Executed(object p)
    {
      string path;
      FolderListViewModel vm;

      this.ResolveParameterList(p, out path, out vm);

      if (vm == null)
        return;

      var dlg = new FolderBrowser.Views.FolderBrowserDialog();

      var dlgViewModel = new FolderBrowser.ViewModels.DialogViewModel(new BrowserViewModel());
      path = (string.IsNullOrEmpty(path) == true ? @"C:\" : path);
      dlgViewModel.TreeBrowser.SetSelectedFolder( path);

      dlg.DataContext = dlgViewModel;

      bool? bResult = dlg.ShowDialog();

      if (dlgViewModel.DialogCloseResult == true || bResult == true)
        vm.AddRecentFolder(dlgViewModel.TreeBrowser.SelectedFolder, true);
    }

    private void RemoveRecentFolder_Executed(object p)
    {
      string path;
      FolderListViewModel vm;

      this.ResolveParameterList(p, out path, out vm);

      if (vm == null || path == null)
        return;

      vm.RemoveRecentFolder(path);
    }

    /// <summary>
    /// Resolves the parameterlist retrieved from a multibinding command parameter
    /// which has packed parameters via List<object> container into 1 object.
    /// </summary>
    /// <param name="p"></param>
    /// <param name="path"></param>
    /// <param name="vm"></param>
    private void ResolveParameterList(object p,
                                      out string path, out FolderListViewModel vm)
    {
      path = null;
      vm = null;

      var l = p as List<object>;

      if (l == null)
        return;

      foreach (var item in l)
      {
        if (item is FSItemViewModel)
        {
          var pathItem = item as FSItemViewModel;

          if (pathItem != null)
            path = pathItem.FullPath;
        }
        else
          if (item is FolderListViewModel)
          {
            var vmItem = item as FolderListViewModel;

            if (vmItem != null)
              vm = item as FolderListViewModel;
          }
      }

      if (path == null)
        path = @"C:\";
    }
    #endregion methods
  }
}
