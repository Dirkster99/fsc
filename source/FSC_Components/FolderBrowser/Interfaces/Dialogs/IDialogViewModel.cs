namespace FolderBrowser.Dialogs.Interfaces
{
	using FileSystemModels.Interfaces.Bookmark;
	using FolderBrowser.Interfaces;
	using System.ComponentModel;

	/// <summary>
	/// Defines an interface to a viewmodel that can be used to drive a dialog.
	/// </summary>
	public interface IDialogViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Gets a property that can be set by the viewmodel to indicate for the
		/// view that the dialog should now be closed.
		/// </summary>
		bool? DialogCloseResult { get; }

		/// <summary>
		/// Gets the viewmodel that drives the folder picker control.
		/// </summary>
		IBrowserViewModel TreeBrowser { get; }

		/// <summary>
		/// Gets the viewmodel that drives the folder bookmark drop down control.
		/// </summary>
		IBookmarksViewModel BookmarkedLocations { get; }
	}
}
