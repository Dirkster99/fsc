namespace FsContentDialogDemo.Demos.ViewModels
{
    using FileSystemModels;
    using FileSystemModels.Browse;
    using FileSystemModels.Interfaces.Bookmark;
    using FolderBrowser;
    using FolderBrowser.Dialogs.Interfaces;
    using FsContentDialogDemo.ViewModels.Base;
    using System.Linq;
    using System.Windows.Input;

    public class DemoViewModel : MWindowDialogLib.ViewModels.Base.BaseViewModel
    {
        #region private fields
        private string _Path;
        private ICommand _ShowConententDialogCommand;
        private IBookmarksViewModel _BookmarkedLocation;
        private ICommand mSelectFolderCommand;
        private IDropDownViewModel mDropDownBrowser = null;
        #endregion private fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public DemoViewModel()
        {
            _Path = @"C:\Program Files\Microsoft SQL Server\90\Shared\Resources\1028";
            BookmarkedLocations = ConstructBookmarks();

            DropDownBrowser = InitializeDropDownBrowser(Path);

            BookmarkedLocations.BrowseEvent += BookmarkedLocations_RequestChangeOfDirectory;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets a bookmark folder property to manage bookmarked folders.
        /// </summary>
        public IBookmarksViewModel BookmarkedLocations
        {
            get
            {
                return _BookmarkedLocation;
            }

            private set
            {
                if (_BookmarkedLocation != value)
                {
                    _BookmarkedLocation = value;
                    RaisePropertyChanged(() => BookmarkedLocations);
                }
            }
        }

        /// <summary>
        /// Gets/sezs a string representing a path to demo ...
        /// </summary>
        public string Path
        {
            get { return _Path; }
            set
            {
                if (_Path != value)
                {
                    _Path = value;
                    RaisePropertyChanged(() => this.Path);
                }
            }
        }

        /// <summary>
        /// Gets a command to demo the content folder browser dialog...
        /// </summary>
        public ICommand ShowConententDialogCommand
        {
            get
            {
                if (_ShowConententDialogCommand == null)
                {
                    _ShowConententDialogCommand = new RelayCommand<object>(async (p) =>
                    {
                        var initialPath = p as string;

                        if (string.IsNullOrEmpty(initialPath) == true)
                            initialPath = this.GetDefaultPath(FsContentDialogDemo.ViewModels.AppLifeCycleViewModel.MyDocumentsUserDir);

                        FolderBrowserControler DlgDemo = new FolderBrowserControler(
                            initialPath, this.BookmarkedLocations);

                        var path = await DlgDemo.ShowContentDialogFromVM(this, true);

                        if (string.IsNullOrEmpty(path) == false)
                            this.Path = path;

                        CloneBookMarks(DlgDemo.BookmarkedLocations);
                    },
                    (p) => { return true; });
                }

                return _ShowConententDialogCommand;
            }
        }

        /// <summary>
        /// Gets a command to demo the folder modal browser dialog...
        /// </summary>
        public ICommand SelectFolderCommand
        {
            get
            {
                if (mSelectFolderCommand == null)
                {
                    mSelectFolderCommand = new RelayCommand<object>((p) =>
                    {
                        // See Loaded event in FolderBrowserTreeView_Loaded methid to understand initial load
                        var treeBrowser = FolderBrowserFactory.CreateBrowserViewModel();

                        // Switch updates to view off by default to speed up load of view
                        // Loading the view will kick-off the browsing via View.Loaded Event
                        // and that in turn will switch on view updates when browser is done...
                        treeBrowser.UpdateView = false;

                        var initialPath = p as string;

                        if (initialPath != null)
                            treeBrowser.InitialPath = initialPath;
                        else
                            treeBrowser.InitialPath = this.Path;

                        treeBrowser.SetSpecialFoldersVisibility(true);

                        var dlgVM = FolderBrowserFactory.CreateDialogViewModel(treeBrowser,
                                                                               BookmarkedLocations);

                        var dlg = new Demos.Views.FolderBrowserDialog();
                        dlg.DataContext = dlgVM;

                        bool? bResult = dlg.ShowDialog();

                        if (dlgVM.DialogCloseResult == true || bResult == true)
                            Path = dlgVM.TreeBrowser.SelectedFolder;

                        CloneBookMarks(dlgVM.BookmarkedLocations);
                    });
                }

                return mSelectFolderCommand;
            }
        }

        /// <summary>
        /// Gets a viewmodel object that can be used to drive a folder browser
        /// displayed inside a drop down button element.
        /// </summary>
        public IDropDownViewModel DropDownBrowser
        {
            get
            {
                return mDropDownBrowser;
            }

            private set
            {
                if (mDropDownBrowser != value)
                {
                    mDropDownBrowser = value;
                    RaisePropertyChanged(() => DropDownBrowser);
                }
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Get a source or distination file path as default path
        /// </summary>
        /// <param name="defaultPath"></param>
        /// <returns></returns>
        private string GetDefaultPath(string defaultPath = @"C:\")
        {
            // Insert Appplication Specific default paths here ...

            return defaultPath;
        }

        /// <summary>
        /// Constructs a few initial entries for
        /// the recent folder collection that implements folder bookmarks.
        /// </summary>
        /// <returns></returns>
        private IBookmarksViewModel ConstructBookmarks()
        {
            IBookmarksViewModel ret = FileSystemModels.Factory.CreateBookmarksViewModel();

            ret.AddFolder(@"C:\Windows");
            ret.AddFolder(@"C:\Users");
            ret.AddFolder(@"C:\Program Files");

            ret.SelectedItem = ret.DropDownItems.First();

            return ret;
        }

        /// <summary>
        /// Method is invoked when the user clicks the drop down and has selected an item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BookmarkedLocations_RequestChangeOfDirectory(object sender, BrowsingEventArgs e)
        {
            if (e.IsBrowsing == false && e.Result == BrowseResult.Complete)
                this.Path = e.Location.Path;
        }

        /// <summary>
        /// Method configures a drop down element to show a
        /// folder picker dialog on opening up.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IDropDownViewModel InitializeDropDownBrowser(string path)
        {
            // See Loaded event in FolderBrowserTreeView_Loaded method to understand initial load
            var treeBrowser = FolderBrowserFactory.CreateBrowserViewModel();

            // Switch updates to view of by default to speed up load of view
            // Loading the view will kick-off the browsing via View.Loaded Event
            // and that in turn will switch on view updates ...
            treeBrowser.UpdateView = false;

            if (string.IsNullOrEmpty(path) == false)
                treeBrowser.InitialPath = path;
            else
                treeBrowser.InitialPath = this.Path;

            treeBrowser.SetSpecialFoldersVisibility(true);

            var dlgVM = FolderBrowserFactory.CreateDropDownViewModel(
                treeBrowser, BookmarkedLocations, this.DropDownClosedResult);

            dlgVM.UpdateInitialPath = this.UpdateCurrentPath;
            dlgVM.UpdateInitialBookmarks = this.UpdateBookmarks;

            dlgVM.ButtonLabel = "Select a Folder";

            return dlgVM;
        }

        private string UpdateCurrentPath()
        {
            return this.Path;
        }

        private IBookmarksViewModel UpdateBookmarks()
        {
            return this.BookmarkedLocations;
        }

        /// <summary>
        /// Method is invoked when drop element is closed.
        /// </summary>
        /// <param name="bookmarks"></param>
        /// <param name="selectedPath"></param>
        /// <param name="result"></param>
        private void DropDownClosedResult(IBookmarksViewModel bookmarks,
                                          string selectedPath,
                                          FolderBrowser.Dialogs.Interfaces.Result result)
        {
            if (result == FolderBrowser.Dialogs.Interfaces.Result.OK)
            {
                CloneBookMarks(bookmarks);

                if (string.IsNullOrEmpty(selectedPath) == false)
                    this.Path = selectedPath;
                else
                    this.Path = PathFactory.SysDefault.Path;
            }
        }

        private void CloneBookMarks(IBookmarksViewModel bookmarkedLocations)
        {
            if (bookmarkedLocations == null)
                return;

            BookmarkedLocations.BrowseEvent -= BookmarkedLocations_RequestChangeOfDirectory;

            this.BookmarkedLocations = bookmarkedLocations.CloneBookmark();

            BookmarkedLocations.BrowseEvent += BookmarkedLocations_RequestChangeOfDirectory;
        }
        #endregion methods
    }
}
