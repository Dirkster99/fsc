namespace FileSystemModels.Browse
{
	using FileSystemModels.Interfaces;
	using System;
	using System.Threading;

	/// <summary>
	/// Class models a request to browse to a certain location (path)
	/// in the file system. A controller should use this class to formulate
	/// a request and Id the corresponding result using the BrowsingEvent
	/// and RequestId property.
	/// </summary>
	public class BrowseRequest
	{
		#region ctors
		/// <summary>
		/// Parameterized class constructor.
		/// </summary>
		public BrowseRequest(IPathModel newLocation,
							 CancellationToken cancelToken = default(CancellationToken))
		  : this()
		{
			NewLocation = newLocation.Clone() as IPathModel;
			CancelTok = cancelToken;
		}

		/// <summary>
		/// Class constructor.
		/// </summary>
		protected BrowseRequest()
		{
			RequestId = Guid.NewGuid();
		}
		#endregion ctors

		#region properties
		/// <summary>
		/// Gets the new location (a path in the file system) to indicate
		/// the target of this browse request.
		/// </summary>
		public IPathModel NewLocation { get; }

		/// <summary>
		/// Gets the CancelationToken (if any) that can be used by the requesting
		/// process to cancel this request during its processing (on timeout or by user).
		/// </summary>
		public CancellationToken CancelTok { get; }

		/// <summary>
		/// Gets the Id that identifies this request among all other requests that may
		/// occur if multiple browse requests are initiated or if user interaction also
		/// causes additional browse processing...
		/// </summary>
		public Guid RequestId { get; }
		#endregion properties
	}
}
