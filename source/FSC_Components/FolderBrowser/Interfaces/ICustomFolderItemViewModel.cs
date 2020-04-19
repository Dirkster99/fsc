namespace FolderBrowser.Interfaces
{
	using System;
	using System.ComponentModel;

	/// <summary>
	/// Defines an interface to a viewmodel item that represents a
	/// Special Folder (Music, Video, Desktop) in the Windows files system.
	/// </summary>
	public interface ICustomFolderItemViewModel : INotifyPropertyChanged
	{
		/// <summary>
		/// Gets the file system path (if any) of this item.
		/// </summary>
		string Path { get; }

		/// <summary>
		/// Gets the Special Folder enumeration of this item.
		/// </summary>
		Environment.SpecialFolder SpecialFolder { get; }
	}
}
