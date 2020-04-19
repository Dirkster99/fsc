namespace FileListView.Interfaces
{
	using System;
	using FileSystemModels.Events;

	/// <summary>
	/// implements an interface that can be used to request event based actions, such as,
	/// opening a file system item (in the client application or windows).
	/// </summary>
	public interface IFileOpenEventSource
	{
		/// <summary>
		/// Event is fired to indicate that user wishes to open a file via this viewmodel.
		/// </summary>
		event EventHandler<FileOpenEventArgs> OnFileOpen;
	}
}
