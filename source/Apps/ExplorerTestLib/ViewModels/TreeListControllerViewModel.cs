namespace ExplorerTestLib.ViewModels
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using ExplorerTestLib.Interfaces;
    using FileListView.Interfaces;
    using FileSystemModels.Browse;
    using FileSystemModels.Events;
    using FileSystemModels.Interfaces;
    using FileSystemModels.Interfaces.Bookmark;
    using FileSystemModels.Models;
    using FilterControlsLib.Interfaces;
    using FolderBrowser.Interfaces;

    /// <summary>
    /// Class implements a tree/folder/file view model class
    /// that can be used to dispaly filesystem related content in a view or dialog.
    /// 
    /// Common Sample dialogs are file pickers for load/save etc.
    /// </summary>
    internal class TreeListControllerViewModel : ControllerBaseViewModel, ITreeListControllerViewModel
    {
        #region fields
        protected readonly object _LockObject;

        private readonly SemaphoreSlim _SlowStuffSemaphore;
        #endregion fields

        #region constructor
        /// <summary>
        /// Custom class constructor
        /// </summary>
        /// <param name="onFileOpenMethod"></param>
        public TreeListControllerViewModel(System.EventHandler<FileOpenEventArgs> onFileOpenMethod)
          : this()
        {
            // Remove the standard constructor event that is fired when a user opens a file
            //this.FolderItemsView.OnFileOpen -= this.FolderItemsView_OnFileOpen;
            WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                .RemoveHandler(FolderItemsView, "OnFileOpen", FolderItemsView_OnFileOpen);

            // ...and establish a new link (if any)
            if (onFileOpenMethod != null)
            {
                //this.FolderItemsView.OnFileOpen += onFileOpenMethod;
                WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                    .AddHandler(FolderItemsView, "OnFileOpen", onFileOpenMethod);
            }
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public TreeListControllerViewModel()
        {
            _SlowStuffSemaphore = new SemaphoreSlim(1, 1);
            _LockObject = new object();

            FolderItemsView = FileListView.Factory.CreateFileListViewModel(new BrowseNavigation());
            FolderTextPath = FolderControlsLib.Factory.CreateFolderComboBoxVM();
            RecentFolders = FileSystemModels.Factory.CreateBookmarksViewModel();
            TreeBrowser = FolderBrowser.FolderBrowserFactory.CreateBrowserViewModel(false);

            // This is fired when the user selects a new folder bookmark from the drop down button
            //RecentFolders.BrowseEvent += FolderTextPath_BrowseEvent;
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(RecentFolders, "BrowseEvent", FolderTextPath_BrowseEvent);

            // This is fired when the text path in the combobox changes to another existing folder
            //FolderTextPath.BrowseEvent += FolderTextPath_BrowseEvent;
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(FolderTextPath, "BrowseEvent", FolderTextPath_BrowseEvent);

            Filters = FilterControlsLib.Factory.CreateFilterComboBoxViewModel();
            //Filters.OnFilterChanged += this.FileViewFilter_Changed;
            WeakEventManager<IFilterComboBoxViewModel, FilterChangedEventArgs>
                .AddHandler(Filters, "OnFilterChanged", FileViewFilter_Changed);

            // This is fired when the current folder in the listview changes to another existing folder
            //this.FolderItemsView.BrowseEvent += FolderTextPath_BrowseEvent;
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(FolderItemsView, "BrowseEvent", FolderTextPath_BrowseEvent);

            // Event fires when the user requests to add a folder into the list of recently visited folders
            //FolderItemsView.BookmarkFolder.RequestEditBookmarkedFolders += this.FolderItemsView_RequestEditBookmarkedFolders;
            WeakEventManager<IEditBookmarks, EditBookmarkEvent>
                .AddHandler(FolderItemsView.BookmarkFolder, "RequestEditBookmarkedFolders", FolderItemsView_RequestEditBookmarkedFolders);

            // This event is fired when a user opens a file
            //this.FolderItemsView.OnFileOpen += this.FolderItemsView_OnFileOpen;
            WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                .AddHandler(FolderItemsView, "OnFileOpen", FolderItemsView_OnFileOpen);

            //TreeBrowser.BrowseEvent += FolderTextPath_BrowseEvent;
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(TreeBrowser, "BrowseEvent", FolderTextPath_BrowseEvent);

            // Event fires when the user requests to add a folder into the list of recently visited folders
            //TreeBrowser.BookmarkFolder.RequestEditBookmarkedFolders += FolderItemsView_RequestEditBookmarkedFolders;
            WeakEventManager<IEditBookmarks, EditBookmarkEvent>
                .AddHandler(TreeBrowser.BookmarkFolder, "RequestEditBookmarkedFolders", FolderItemsView_RequestEditBookmarkedFolders);
        }
        #endregion constructor

        #region properties
        /// <summary>
        /// Gets the viewmodel that drives the folder picker control.
        /// </summary>
        public IBrowserViewModel TreeBrowser { get; }
        #endregion properties

        #region methods
        /// <summary>
        /// Master controller interface method to navigate all views
        /// to the folder indicated in <paramref name="folder"/>
        /// - updates all related viewmodels.
        /// </summary>
        /// <param name="itemPath"></param>
        /// <param name="requestor"</param>
        public override void NavigateToFolder(IPathModel itemPath)
        {
            // XXX Todo Keep task reference, support cancel, and remove on end?
            var t = NavigateToFolderAsync(itemPath, null);
        }

        /// <summary>
        /// Master controler interface method to navigate all views
        /// to the folder indicated in <paramref name="folder"/>
        /// - updates all related viewmodels.
        /// </summary>
        /// <param name="itemPath"></param>
        /// <param name="requestor"</param>
        private async Task NavigateToFolderAsync(IPathModel itemPath, object sender)
        {
            // Make sure the task always processes the last input but is not started twice
            await _SlowStuffSemaphore.WaitAsync();
            try
            {
                lock (_LockObject)
                {
                    TreeBrowser.SetExternalBrowsingState(true);
                    FolderItemsView.SetExternalBrowsingState(true);
                    FolderTextPath.SetExternalBrowsingState(true);
                }

                bool? browseResult = null;

                // Navigate TreeView to this file system location
                if (TreeBrowser != sender)
                    browseResult = await TreeBrowser.NavigateToAsync(itemPath);

                // Navigate Folder ComboBox to this folder
                if (FolderTextPath != sender && browseResult != false)
                    browseResult = await FolderTextPath.NavigateToAsync(itemPath);

                // Navigate Folder/File ListView to this folder
                if (FolderItemsView != sender && browseResult != false)
                    browseResult = await FolderItemsView.NavigateToAsync(itemPath);

                if (browseResult == true)
                {
                    SelectedFolder = itemPath.Path;

                    // Log location into history of recent locations
                    NaviHistory.Forward(itemPath);
                }
            }
            catch { }
            finally
            {
                TreeBrowser.SetExternalBrowsingState(true);
                FolderItemsView.SetExternalBrowsingState(false);
                FolderTextPath.SetExternalBrowsingState(false);

                _SlowStuffSemaphore.Release();
            }
        }

        /// <summary>
        /// Executes when the file open event is fired and class was constructed with statndard constructor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void FolderItemsView_OnFileOpen(object sender,
                                                           FileOpenEventArgs e)
        {
            MessageBox.Show("File Open:" + e.FileName);
        }

        /// <summary>
        /// One of the controls has changed its location in the filesystem.
        /// This method is invoked to synchronize this change with all other controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTextPath_BrowseEvent(object sender,
                                                FileSystemModels.Browse.BrowsingEventArgs e)
        {
            var location = e.Location;

            SelectedFolder = location.Path;

            if (e.IsBrowsing == false && e.Result == BrowseResult.Complete)
            {
                // XXX Todo Keep task reference, support cancel, and remove on end?
                var t = NavigateToFolderAsync(location, sender);
            }
            else
            {
                if (e.IsBrowsing == true)
                {
                    // The sender has messaged: "I am chnaging location..."
                    // So, we set this property to tell the others:
                    // 1) Don't change your location now (eg.: Disable UI)
                    // 2) We'll be back to tell you the location when we know it
                    if (TreeBrowser != sender)
                        TreeBrowser.SetExternalBrowsingState(true);

                    if (FolderTextPath != sender)
                        FolderTextPath.SetExternalBrowsingState(true);

                    if (FolderItemsView != sender)
                        FolderItemsView.SetExternalBrowsingState(true);
                }
            }
        }
        #endregion methods
    }
}
