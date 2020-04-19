namespace FolderBrowser.ViewModels.Messages
{
	using System.ComponentModel;

	/// <summary>
	/// Defines an interface for a viewmodel class that can be
	/// used to pop-up messages in a UI (without using messageboxes).
	/// </summary>
	public interface IDisplayMessageViewModel : ISetMessageDisplay, INotifyPropertyChanged
	{
		/// <summary>
		/// Gets/sets whether an error message is currently available.
		/// </summary>
		bool IsErrorMessageAvailable { get; set; }

		/// <summary>
		/// Gets/sets the massage.
		/// </summary>
		string Message { get; set; }
	}

	/// <summary>
	/// Defines an interface that allows a client to set a message for display.
	/// </summary>
	public interface ISetMessageDisplay
	{
		/// <summary>
		/// Sets the message to be displayed.
		/// </summary>
		/// <param name="Message"></param>
		void SetMessage(string Message);
	}
}
