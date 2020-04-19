namespace FileListView
{
	using FileListView.Interfaces;
	using FileListView.ViewModels;
	using FileSystemModels.Interfaces;
	using FileSystemModels.Models.FSItems.Base;

	/// <summary>
	/// Implements factory methods that create library objects that are accessible
	/// through interfaces but are otherwise invisible for the outside world.
	/// </summary>
	public sealed class Factory
	{
		private Factory() { }

		/// <summary>
		/// Creates a viewmodel object that implements the <see cref="IFileListViewModel"/>
		/// interface to drive a file listview.
		/// </summary>
		/// <returns>A new instance of a file list viewmodel that can be used to drive
		/// a listview that shows folders and files in the file system.</returns>
		public static IFileListViewModel CreateFileListViewModel()
		{
			return new FileListViewModel();
		}

		/// <summary>
		/// Creates a viewmodel object that implements the <see cref="ILVItemViewModel"/>
		/// interface to drive one item that can be part of a file listview.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="displayName"></param>
		/// <returns></returns>
		public static ILVItemViewModel CreateItem(
			  string path
			, FSItemType type
			, string displayName)
		{
			return new LVItemViewModel(path, type, displayName);
		}
	}
}
