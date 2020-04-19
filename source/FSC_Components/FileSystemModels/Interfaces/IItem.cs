namespace FileSystemModels.Interfaces
{
	using FileSystemModels.Models.FSItems.Base;

	/// <summary>
	/// Define the basic properties and methods of a viewmodel for
	/// a file system item.
	/// </summary>
	public interface IItem
	{
		/// <summary>
		/// Gets the type (folder, file) of this item
		/// </summary>
		FSItemType ItemType { get; }

		/// <summary>
		/// Gets the path to this item
		/// </summary>
		string ItemPath { get; }

		/// <summary>
		/// Gets the name (without the path) of this item.
		/// </summary>
		string ItemName { get; }

		/// <summary>
		/// Gets a folder item string for display purposes.
		/// This string can evaluete to 'C:\ (Windows)' for drives,
		/// if the 'C:\' drive was named 'Windows'.
		/// </summary>
		string ItemDisplayString { get; }

		/// <summary>
		/// Gets whether this folder is currently expanded or not.
		/// </summary>
		bool IsExpanded { get; }
	}
}
