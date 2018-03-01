namespace ExplorerTestLib.ViewModels
{
    using ExplorerTestLib.Interfaces;
    using FileListView.Interfaces;
    using FileSystemModels.Browse;
    using FileSystemModels.Events;
    using FileSystemModels.Interfaces;
    using FileSystemModels.Interfaces.Bookmark;
    using FileSystemModels.Models;
    using FilterControlsLib.Interfaces;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Class implements a folder/file view model class
    /// that can be used to dispaly filesystem related content in an ItemsControl.
    /// </summary>
    internal class ListControllerViewModel : ControllerBaseViewModel, IListControllerViewModel
    {
        #region fields
        protected readonly object _LockObject;

        private readonly SemaphoreSlim _SlowStuffSemaphore;
        #endregion

        #region constructor
        /// <summary>
        /// Custom class constructor
        /// </summary>
        /// <param name="onFileOpenMethod"></param>
        public ListControllerViewModel(System.EventHandler<FileOpenEventArgs> onFileOpenMethod)
          : this()
        {
            // Remove the standard constructor event that is fired when a user opens a file
            //FolderItemsView.OnFileOpen -= this.FolderItemsView_OnFileOpen;
            WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                .RemoveHandler(FolderItemsView, "OnFileOpen", FolderItemsView_OnFileOpen);

            // ...and establish a new link (if any)
            if (onFileOpenMethod != null)
            {
                //FolderItemsView.OnFileOpen += onFileOpenMethod;
                WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                    .AddHandler(FolderItemsView, "OnFileOpen", onFileOpenMethod);
            }
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public ListControllerViewModel()
        {
            _SlowStuffSemaphore = new SemaphoreSlim(1, 1);
            _LockObject = new object();

            FolderItemsView = FileListView.Factory.CreateFileListViewModel(new BrowseNavigation());
            FolderTextPath = FolderControlsLib.Factory.CreateFolderComboBoxVM();
            RecentFolders = FileSystemModels.Factory.CreateBookmarksViewModel();

            // This is fired when the user selects a new folder bookmark from the drop down button
            //RecentFolders.BrowseEvent += FolderTextPath_BrowseEvent;
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(RecentFolders, "BrowseEvent", FolderTextPath_BrowseEvent);

            // This is fired when the text path in the combobox changes to another existing folder
            //FolderTextPath.BrowseEvent += FolderTextPath_BrowseEvent;
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(FolderTextPath, "BrowseEvent", FolderTextPath_BrowseEvent);

            Filters = FilterControlsLib.Factory.CreateFilterComboBoxViewModel();
            //Filters.OnFilterChanged += FileViewFilter_Changed;
            WeakEventManager<IFilterComboBoxViewModel, FilterChangedEventArgs>
                .AddHandler(Filters, "OnFilterChanged", FileViewFilter_Changed);

            // This is fired when the current folder in the listview changes to another existing folder
            //FolderItemsView.BrowseEvent += FolderTextPath_BrowseEvent;
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(FolderItemsView, "BrowseEvent", FolderTextPath_BrowseEvent);

            // This is fired when the user requests to add a folder into the list of recently visited folders
            //FolderItemsView.BookmarkFolder.RequestEditBookmarkedFolders += this.FolderItemsView_RequestEditBookmarkedFolders;
            WeakEventManager<IEditBookmarks, EditBookmarkEvent>
                .AddHandler(FolderItemsView.BookmarkFolder, "RequestEditBookmarkedFolders", FolderItemsView_RequestEditBookmarkedFolders);

            // This event is fired when a user opens a file
            //FolderItemsView.OnFileOpen += this.FolderItemsView_OnFileOpen;
            WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                .AddHandler(FolderItemsView, "OnFileOpen", FolderItemsView_OnFileOpen);
        }
        #endregion constructor

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
                    FolderItemsView.SetExternalBrowsingState(true);
                    FolderTextPath.SetExternalBrowsingState(true);
                    SelectedFolder = itemPath.Path;
                }

                bool? browseResult = null;

                if (FolderTextPath != sender)
                {
                    // Navigate Folder ComboBox to this folder
                    browseResult = await FolderTextPath.NavigateToAsync(itemPath);
                }

                if (FolderItemsView != sender && browseResult != false)
                {
                    // Navigate Folder/File ListView to this folder
                    browseResult = await FolderItemsView.NavigateToAsync(itemPath);
                }

                if (browseResult == true)    // Log location into history of recent locations
                    NaviHistory.Forward(itemPath);
            }
            catch { }
            finally
            {
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
        /// A control has send an event that it has (been) browsing to a new location.
        /// Lets sync this with all other controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTextPath_BrowseEvent(object sender,
                                                FileSystemModels.Browse.BrowsingEventArgs e)
        {
            var itemPath = e.Location;

            SelectedFolder = itemPath.Path;

            if (e.IsBrowsing == false && e.Result == BrowseResult.Complete)
            {
                if (FolderTextPath != sender)
                {
                    // Navigate Folder ComboBox to this folder
                    FolderTextPath.NavigateTo(itemPath);
                }

                if (FolderItemsView != sender)
                {
                    // Navigate Folder/File ListView to this folder
                    FolderItemsView.NavigateTo(itemPath);
                }

                // Log location into history of recent locations
                NaviHistory.Forward(itemPath);
            }
            else
            {
                if (e.IsBrowsing == true)
                {
                    if (FolderTextPath != sender)
                    {
                        // Navigate Folder ComboBox to this folder
                        FolderTextPath.SetExternalBrowsingState(true);
                    }

                    if (FolderItemsView != sender)
                    {
                        // Navigate Folder/File ListView to this folder
                        FolderItemsView.SetExternalBrowsingState(true);
                    }
                }
            }
        }
        #endregion methods
    }
}
