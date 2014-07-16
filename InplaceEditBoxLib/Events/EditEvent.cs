namespace InplaceEditBoxLib.Events
{
  using System;

  /// <summary>
  /// Determine the type of edit event that can be reuqested from the view.
  /// </summary>
  public enum RequestEditEvent
  {
    /// <summary>
    /// Start the editing mode for renaming the item represented by the viewmodel
    /// that send this message to its view.
    /// </summary>
    StartEditMode,

    /// <summary>
    /// Unknown type of event should never occur if this enum is used correctly.
    /// </summary>
    Unknown
  }

  /// <summary>
  /// Event handler delegation method to be used when handling <seealso cref="RequestEdit"/> events.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="e"></param>
  public delegate void RequestEditEventHandler(object sender, RequestEdit e);

  /// <summary>
  /// Implements an event that can be send from viewmodel to view
  /// to request certain edit modes.
  /// </summary>
  public class RequestEdit : EventArgs
  {
    #region constructors
    /// <summary>
    /// Parameterized class constructor
    /// </summary>
    /// <param name="eventRequest"></param>
    public RequestEdit(RequestEditEvent eventRequest) : this()
    {
      this.Request = eventRequest;
    }

    /// <summary>
    /// Class Constructor
    /// </summary>
    protected RequestEdit()
    {
      this.Request = RequestEditEvent.Unknown;
    }
    #endregion constructors

    /// <summary>
    /// Gets the type of editing event that was requested by the viewmodel.
    /// </summary>
    public RequestEditEvent Request { get; private set; }
  }
}
