namespace FileSystemModels.Browse
{
	/// <summary>
	/// Determines the state of a browsing process. for instance, browsing a
	/// viewmodel (and it attached view(s)) from 'C:\' to 'G:\' may involve
	/// a:
	/// - start state == (Unknown)
	/// 
	/// and a state of completion:
	/// - error   == Incomplete or
	/// - success == Complete
	/// </summary>
	public enum BrowseResult
	{
		/// <summary>
		/// Result is unknown since browse task is currently running
		/// or completed with unknown result ...
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Browse Process OK - destination does exist and is accessible
		/// </summary>
		Complete = 1,

		/// <summary>
		/// Error indicator - destination does not exist or is not accessible
		/// </summary>
		InComplete = 2
	}
}
