namespace FolderBrowser.BookmarkFolder
{
    using System;
    using System.Windows.Input;

    public interface IAddFolderBookmark
    {
        ICommand RecentFolderAddCommand { get; }
        event EventHandler<RecentFolderEvent> RequestEditBookmarkedFolders;
    }
}
