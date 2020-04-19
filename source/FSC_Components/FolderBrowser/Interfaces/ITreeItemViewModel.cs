namespace FolderBrowser.Interfaces
{
	using FileSystemModels.Interfaces;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Windows.Media.Imaging;

	/// <summary>
	/// Defines an interface to an object in the tree hierarchy that can have
	/// child items (e.g.: folders) and a parent item (drive or folder).
	/// </summary>
	public interface ITreeItemViewModel : IItem, IParent
	{
		#region properties
		/// <summary>
		/// Gets a collection of sub-folders (if any) that are contained within this folder.
		/// </summary>
		IEnumerable<ITreeItemViewModel> Folders { get; }

		/// <summary>
		/// Determine whether child is a dummy (must be evaluated and replaced
		/// with real data) or not.
		/// </summary>
		bool HasDummyChild { get; }

		/// <summary>
		/// Get/set whether this folder is currently selected or not.
		/// </summary>
		bool IsSelected { get; }
		#endregion properties

		#region methods
		/// <summary>
		/// Adds the folder item into the collection of sub-folders
		/// of this folder.
		/// </summary>
		/// <param name="item"></param>
		void ChildAdd(ITreeItemViewModel item);

		/// <summary>
		/// Remove all sub-folders from a given folder.
		/// </summary>
		void ChildrenClear();

		/// <summary>
		/// Load all sub-folders into the Folders collection.
		/// </summary>
		void ChildrenLoad();

		/// <summary>
		/// Load all sub-folders into the Folders collection.
		/// </summary>
		Task<int> ChildrenLoadAsync();

		/// <summary>
		/// Attempts to find an item with the given name in the list of child items
		/// below this item and returns it or null.
		/// </summary>
		/// <param name="folderName"></param>
		/// <returns></returns>
		ITreeItemViewModel ChildTryGet(string folderName);

		/// <summary>
		/// Renames a child below this item.
		/// </summary>
		/// <param name="oldName"></param>
		/// <param name="newName"></param>
		/// <returns></returns>
		bool ChildRename(string oldName, string newName);

		/// <summary>
		/// Create a new folder with a standard name
		/// 'New folder n' underneath this folder.
		/// </summary>
		/// <returns>a viewmodel of the newly created directory or null</returns>
		ITreeItemViewModel CreateNewDirectory();

		/// <summary>
		/// Rename the name of the folder into a new name.
		/// </summary>
		/// <param name="newFolderName"></param>
		void Rename(string newFolderName);

		/// <summary>
		/// Shows a pop-notification message with the given title and text.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="imageIcon"></param>
		/// <returns>true if the event was succesfully fired.</returns>
		bool ShowNotification(string title, string message,
							  BitmapImage imageIcon = null);
		#endregion methods
	}
}
