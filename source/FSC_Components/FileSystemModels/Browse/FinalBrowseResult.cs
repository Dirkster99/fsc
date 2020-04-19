namespace FileSystemModels.Browse
{
	using FileSystemModels.Interfaces;
	using System;

	/// <summary>
	/// Class models the final result of the process for browsing to a location (path)
	/// in the file system. A controller should use this class to measure success or
	/// failure of a previous <seealso cref="BrowseRequest"/>.
	///
	/// An object of this class can be used to match a previous request with the
	/// indicated result (for logging or debugging purposes).
	/// </summary>
	public class FinalBrowseResult
	{
		#region ctors
		/// <summary>
		/// Parameterized class constructor.
		/// </summary>
		public FinalBrowseResult(IPathModel requestedLocation,
								 Guid requestId = default(System.Guid),
								 BrowseResult result = BrowseResult.Unknown
								 )
		  : this()
		{
			RequestedLocation = requestedLocation;
			Result = result;
			RequestId = requestId;
		}

		/// <summary>
		/// Class constructor.
		/// </summary>
		protected FinalBrowseResult()
		{
			Result = BrowseResult.Unknown;
			RequestedLocation = null;
			RequestId = default(System.Guid);
		}
		#endregion ctors

		#region properties
		/// <summary>
		/// Gets a descriptive browse result that can be used to classify
		/// success or failure and sub-class different types of failures.
		/// </summary>
		public BrowseResult Result { get; }

		/// <summary>
		/// Gets the new location (a path in the file system) to indicate
		/// the target of this browse request.
		/// </summary>
		public IPathModel RequestedLocation { get; }

		/// <summary>
		/// Gets the Id that identifies this request among all other requests that may
		/// occur if multiple browse requests are initiated or if user interaction also
		/// causes additional browse processing...
		/// </summary>
		public Guid RequestId { get; }

		/// <summary>
		/// Gets/sets an exception object that can be used to communicate exceptional
		/// results back to the requesting instance.
		/// </summary>
		public System.Exception UnexpectedError { get; set; }
		#endregion properties

		/// <summary>
		/// Short-cut to convert a given <seealso cref="BrowseRequest"/> into a final
		/// result receipt to support simple closure of requests being full-filled or not etc ...
		/// </summary>
		/// <param name="request"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public static FinalBrowseResult FromRequest(BrowseRequest request, BrowseResult result)
		{
			if (request != null)
				return new FinalBrowseResult(request.NewLocation, request.RequestId, result);

			return new FinalBrowseResult(null, default(System.Guid), result);
		}
	}
}
