namespace FileListView.Interfaces
{
	using FileSystemModels.Interfaces;
	using InplaceEditBoxLib.Interfaces;
	using System.ComponentModel;

	/// <summary>
	/// Defines the properties and members of an item view model that is
	/// designed for usage in list views or similar controls.
	/// </summary>
	public interface ILVItemViewModel : IListItemViewModel, IEditBox, INotifyPropertyChanged
	{
		/// <summary>
		/// Renames this item with the indicated name.
		/// 
		/// This includes renaming the item in the file system.
		/// </summary>
		/// <param name="newFolderName"></param>
		void RenameFileOrFolder(string newFolderName);
	}
}