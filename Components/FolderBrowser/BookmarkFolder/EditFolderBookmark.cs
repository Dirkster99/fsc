namespace FolderBrowser.BookmarkFolder
{
    using FileSystemModels.Models;
    using FileSystemModels.Models.FSItems;
    using FolderBrowser.ViewModels;
    using FsCore.ViewModels;
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Class implements properties and methods required to message folder
    /// bookmark events to a related class which is responsible for viewing
    /// bookmarks (for example in a drop down button).
    /// </summary>
    internal class EditFolderBookmark : FolderBrowser.BookmarkFolder.IAddFolderBookmark
    {
        #region fields
        private ICommand mRecentFolderAddCommand;
        private ICommand mRecentFolderRemoveCommand;
        #endregion fields

        #region events
        /// <summary>
        /// Generate an event to remove or add a recent folder to a collection.
        /// </summary>
        public event EventHandler<RecentFolderEvent> RequestEditBookmarkedFolders;
        #endregion events

        #region properties
        /// <summary>
        /// Determine whether an Add/Remove command can execute or not.
        /// </summary>
        public bool RecentFolderCommandCanExecute
        {
            get
            {
                if (this.RequestEditBookmarkedFolders != null)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Implements a command that adds a removes a folder location.
        /// Expected parameter is of type FSItemVM.
        /// </summary>
        public ICommand RecentFolderRemoveCommand
        {
            get
            {
                if (this.mRecentFolderRemoveCommand == null)
                    this.mRecentFolderRemoveCommand = new RelayCommand<object>((p) => this.RecentFolderRemove_Executed(p),
                                                                               (p) => this.RecentFolderCommand_CanExecute(p));

                return this.mRecentFolderRemoveCommand;
            }
        }

        /// <summary>
        /// Implements a command that adds a recent folder location.
        /// Expected parameter is of type FSItemVM.
        /// </summary>
        public ICommand RecentFolderAddCommand
        {
            get
            {
                if (this.mRecentFolderAddCommand == null)
                    this.mRecentFolderAddCommand = new RelayCommand<object>((p) => this.RecentFolderAdd_Executed(p),
                                                                            (p) => this.RecentFolderCommand_CanExecute(p));

                return this.mRecentFolderAddCommand;
            }
        }
        #endregion properties

        #region methods
        private bool RecentFolderCommand_CanExecute(object param)
        {
            if (this.RequestEditBookmarkedFolders != null)
            {
                var item = param as FolderViewModel;

                if (item != null)
                    return true;
            }

            return false;
        }

        private void RecentFolderRemove_Executed(object param)
        {
            var item = param as FolderViewModel;

            if (item == null)
                return;

            // Tell client via event to get rid of this entry
            if (this.RequestEditBookmarkedFolders != null)
                this.RequestEditBookmarkedFolders(this,
                    new RecentFolderEvent(new PathModel(item.FolderPath, FSItemType.Folder),
                    RecentFolderEvent.RecentFolderAction.Remove));
        }

        private void RecentFolderAdd_Executed(object param)
        {
            var item = param as FolderViewModel;

            if (item == null)
                return;

            // Tell client via event to add this entry
            if (this.RequestEditBookmarkedFolders != null)
                this.RequestEditBookmarkedFolders(this,
                    new RecentFolderEvent(new PathModel(item.FolderPath, FSItemType.Folder),
                    RecentFolderEvent.RecentFolderAction.Add));
        }
        #endregion methods
    }
}
