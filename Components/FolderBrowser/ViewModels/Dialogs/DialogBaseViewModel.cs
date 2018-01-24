namespace FolderBrowser.Dialogs.ViewModels
{
    using FolderBrowser.Dialogs.Interfaces;
    using FolderBrowser.Interfaces;
    using FolderBrowser.ViewModels;
    using FsCore.ViewModels.Base;

    /// <summary>
    /// A base class for implementing a viewmodel that can drive dialogs
    /// or other such views that display a forlder browser or folder picker
    /// view for selecintg a folder in the Windows file system.
    /// </summary>
    internal class DialogBaseViewModel : ViewModelBase
    {
        #region fields
        private IBrowserViewModel mTreeBrowser = null;
        private IBookmarkedLocationsViewModel mBookmarkedLocations = null;
        #endregion fields

        #region constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        public DialogBaseViewModel(IBrowserViewModel treeBrowser = null,
                                   IBookmarkedLocationsViewModel recentLocations = null)
        {
            if (treeBrowser == null)
                TreeBrowser = new BrowserViewModel();
            else
                TreeBrowser = treeBrowser;

            ResetBookmarks(recentLocations);
        }
        #endregion constructor

        #region properties
        /// <summary>
        /// Gets the viewmodel that drives the folder picker control.
        /// </summary>
        public IBrowserViewModel TreeBrowser
        {
            get
            {
                return mTreeBrowser;
            }

            private set
            {
                if (mTreeBrowser != value)
                {
                    mTreeBrowser = value;
                    RaisePropertyChanged(() => TreeBrowser);
                }
            }
        }

        /// <summary>
        /// Gets the viewmodel that drives the folder bookmark drop down control.
        /// </summary>
        public IBookmarkedLocationsViewModel BookmarkedLocations
        {
            get
            {
                return mBookmarkedLocations;
            }

            private set
            {
                if (mBookmarkedLocations != value)
                {
                    mBookmarkedLocations = value;
                    RaisePropertyChanged(() => BookmarkedLocations);
                }
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// (Re-)Connects the Bookmark ViewModel with the
        /// <seealso cref="IBrowserViewModel.BrowsePath(string, bool)"/> method via private method.
        /// to enable user's path selection being input to folder browser.
        /// </summary>
        /// <param name="recentLocations"></param>
        protected void ResetBookmarks(IBookmarkedLocationsViewModel recentLocations)
        {
            if (this.BookmarkedLocations != null)
            {
                this.BookmarkedLocations.RequestChangeOfDirectory -= RecentLocations_RequestChangeOfDirectory;
                
                if (TreeBrowser != null)
                    TreeBrowser.BookmarkFolder.RequestEditBookmarkedFolders -= BookmarkFolder_RequestEditBookmarkedFolders;
            }

            if (recentLocations != null)
            {
                // The recentlocations drop down is optionanl
                // Its component and add/remove context menu accessibility in the treeview
                // is only shown if caller supplied this object
                this.BookmarkedLocations = recentLocations.CloneBookmark();
            }
            else
                this.BookmarkedLocations = FolderBrowser.FolderBrowserFactory.CreateReceentLocationsViewModel();

            if (this.BookmarkedLocations != null)
            {
                this.BookmarkedLocations.RequestChangeOfDirectory += RecentLocations_RequestChangeOfDirectory;
            }

            TreeBrowser.BookmarkFolder.RequestEditBookmarkedFolders += BookmarkFolder_RequestEditBookmarkedFolders;
        }

        private void RecentLocations_RequestChangeOfDirectory(object sender,
                                                              FolderBrowser.Events.FolderChangedEventArgs e)
        {
            TreeBrowser.BrowsePath(e.Folder.Path, false);
        }

        /// <summary>
        /// Removes or adds a folder bookmark if the event requests that.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BookmarkFolder_RequestEditBookmarkedFolders(object sender, BookmarkFolder.RecentFolderEvent e)
        {
            switch (e.Action)
            {
                case FolderBrowser.BookmarkFolder.RecentFolderEvent.RecentFolderAction.Remove:
                    BookmarkedLocations.RemoveFolder(e.Folder.Path);
                    break;

                case FolderBrowser.BookmarkFolder.RecentFolderEvent.RecentFolderAction.Add:
                    BookmarkedLocations.AddFolder(e.Folder.Path);
                    break;

                default:
                    throw new System.NotImplementedException(e.Action.ToString());
            }
        }
        #endregion methods
    }
}
