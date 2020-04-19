namespace FileSystemModels.Interfaces
{
	/// <summary>
	/// Define the properties and methods of a viewmodel for
	/// a file system item.
	/// </summary>
	public interface IListItemViewModel : IItem
	{
		#region properties
		/// <summary>
		/// Gets whether or not to show an Icon for this item or not.
		/// </summary>
		bool ShowIcon { get; }
		#endregion properties

		#region methods
		/// <summary>
		/// Determine whether a given path is an exeisting directory or not.
		/// </summary>
		/// <returns>true if this directory exists and otherwise false</returns>
		bool DirectoryPathExists();
		#endregion methods
	}
}
