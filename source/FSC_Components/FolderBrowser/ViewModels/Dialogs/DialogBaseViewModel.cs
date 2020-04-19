namespace FolderBrowser.Dialogs.ViewModels
{
	using FileSystemModels.Browse;
	using FileSystemModels.Events;
	using FileSystemModels.Interfaces.Bookmark;
	using FileSystemModels.ViewModels.Base;
	using FolderBrowser.Interfaces;
	using FolderBrowser.ViewModels;
	using System.Windows;

	/// <summary>
	/// A base class for implementing a viewmodel that can drive dialogs
	/// or other such views that display a forlder browser or folder picker
	/// view for selecintg a folder in the Windows file system.
	/// </summary>
	internal class DialogBaseViewModel : ViewModelBase
	{
		#region fields
		private IBrowserViewModel _TreeBrowser = null;
		private IBookmarksViewModel _BookmarkedLocations = null;
		#endregion fields

		#region constructors
		/// <summary>
		/// Class constructor
		/// </summary>
		public DialogBaseViewModel(IBrowserViewModel treeBrowser = null,
								   IBookmarksViewModel recentLocations = null)
		{
			if (treeBrowser == null)
				TreeBrowser = new BrowserViewModel();
			else
				TreeBrowser = treeBrowser;

			ResetBookmarks(recentLocations);
		}
		#endregion constructors

		#region properties
		/// <summary>
		/// Gets the viewmodel that drives the folder picker control.
		/// </summary>
		public IBrowserViewModel TreeBrowser
		{
			get
			{
				return _TreeBrowser;
			}

			private set
			{
				if (_TreeBrowser != value)
				{
					_TreeBrowser = value;
					RaisePropertyChanged(() => TreeBrowser);
				}
			}
		}

		/// <summary>
		/// Gets the viewmodel that drives the folder bookmark drop down control.
		/// </summary>
		public IBookmarksViewModel BookmarkedLocations
		{
			get
			{
				return _BookmarkedLocations;
			}

			private set
			{
				if (_BookmarkedLocations != value)
				{
					_BookmarkedLocations = value;
					RaisePropertyChanged(() => BookmarkedLocations);
				}
			}
		}
		#endregion properties

		#region methods
		/// <summary>
		/// (Re-)Connects the Bookmark ViewModel with the
		/// <seealso cref="IBrowserViewModel"/>.BrowsePath(string, bool) method via private method.
		/// to enable user's path selection being input to folder browser.
		/// </summary>
		/// <param name="recentLocations"></param>
		protected void ResetBookmarks(IBookmarksViewModel recentLocations)
		{
			if (BookmarkedLocations != null)
			{
				//BookmarkedLocations.BrowseEvent -= RecentLocations_RequestChangeOfDirectory;
				WeakEventManager<ICanNavigate, BrowsingEventArgs>
					.RemoveHandler(BookmarkedLocations, "BrowseEvent", RecentLocations_RequestChangeOfDirectory);

				if (TreeBrowser != null)
				{
					//TreeBrowser.BookmarkFolder.RequestEditBookmarkedFolders -= BookmarkFolder_RequestEditBookmarkedFolders;
					WeakEventManager<IEditBookmarks, EditBookmarkEvent>
						.RemoveHandler(TreeBrowser.BookmarkFolder, "RequestEditBookmarkedFolders", BookmarkFolder_RequestEditBookmarkedFolders);
				}
			}

			if (recentLocations != null)
			{
				// The recentlocations drop down is optionanl
				// Its component and add/remove context menu accessibility in the treeview
				// is only shown if caller supplied this object
				BookmarkedLocations = recentLocations.CloneBookmark();
			}
			else
				BookmarkedLocations = FileSystemModels.Factory.CreateBookmarksViewModel();

			if (BookmarkedLocations != null)
			{
				//BookmarkedLocations.BrowseEvent += RecentLocations_RequestChangeOfDirectory;
				WeakEventManager<ICanNavigate, BrowsingEventArgs>
					.AddHandler(BookmarkedLocations, "BrowseEvent", RecentLocations_RequestChangeOfDirectory);
			}

			//TreeBrowser.BookmarkFolder.RequestEditBookmarkedFolders += BookmarkFolder_RequestEditBookmarkedFolders;
			WeakEventManager<IEditBookmarks, EditBookmarkEvent>
				.AddHandler(TreeBrowser.BookmarkFolder, "RequestEditBookmarkedFolders", BookmarkFolder_RequestEditBookmarkedFolders);
		}

		private void RecentLocations_RequestChangeOfDirectory(object sender,
															  BrowsingEventArgs e)
		{
			if (e.IsBrowsing == false && e.Result == BrowseResult.Complete)
			{
				// XXX Todo Keep task reference, support cancel, and remove on end?
				var t = TreeBrowser.NavigateToAsync(new BrowseRequest(e.Location));
			}
		}

		/// <summary>
		/// Removes or adds a folder bookmark if the event requests that.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void BookmarkFolder_RequestEditBookmarkedFolders(object sender, EditBookmarkEvent e)
		{
			switch (e.Action)
			{
				case EditBookmarkEvent.RecentFolderAction.Remove:
					BookmarkedLocations.RemoveFolder(e.Folder.Path);
					break;

				case EditBookmarkEvent.RecentFolderAction.Add:
					BookmarkedLocations.AddFolder(e.Folder.Path);
					break;

				default:
					throw new System.NotImplementedException(e.Action.ToString());
			}
		}
		#endregion methods
	}
}
