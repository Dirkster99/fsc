namespace InplaceEditBoxLib.ViewModels
{
  using System.Windows.Media.Imaging;
  using InplaceEditBoxLib.Events;
  using InplaceEditBoxLib.Interfaces;
  using UserNotification.Events;

  /// <summary>
  /// Implements a viewmodel class that can be used as base of a viewmodel
  /// that drives the <seealso cref="InplaceEditBoxLib.Views.EditBox"/> view.
  /// </summary>
  public class EditInPlaceViewModel : Base.ViewModelBase, IEditBox
  {
    #region fields
    /// <summary>
    /// Log4net logger facility for this class.
    /// </summary>
    protected static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private bool mIsReadOnly;
    #endregion fields

    #region events
    /// <summary>
    /// Expose an event that is triggered when the viewmodel tells its view:
    /// Here is another notification message please show it to the user.
    /// </summary>
    public event UserNotification.Events.ShowNotificationEventHandler ShowNotificationMessage;

    /// <summary>
    /// Expose an event that is triggered when the viewmodel requests its view to
    /// start the editing mode for rename this item.
    /// </summary>
    public event InplaceEditBoxLib.Events.RequestEditEventHandler RequestEdit;
    #endregion events

    #region constructors
    /// <summary>
    /// Class constructor
    /// </summary>
    public EditInPlaceViewModel()
    {
      this.IsReadOnly = false;
    }
    #endregion constructors

    #region properties
    /// <summary>
    /// Gets/sets whether this folder is readonly (can be renamed) or not.
    /// A drive can, for example, not be renamed and is therefore, readonly
    /// on this context.
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        return this.mIsReadOnly;
      }

      protected set
      {
        if (this.mIsReadOnly != value)
        {
          this.mIsReadOnly = value;

          this.RaisePropertyChanged(() => this.IsReadOnly);
        }
      }
    }
    #endregion properties

    #region methods
    /// <summary>
    /// Call this method to request of start editing mode for renaming this item.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>Returns true if event was successfully send (listener is attached), otherwise false</returns>
    public bool RequestEditMode(RequestEditEvent request)
    {
      if (this.RequestEdit != null)
      {
        this.RequestEdit(this, new RequestEdit(request));
        return true;
      }

      return false;
    }

    /// <summary>
    /// Shows a pop-notification message with the given title and text.
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="imageIcon"></param>
    /// <returns>true if the event was succesfully fired.</returns>
    public bool ShowNotification(string title, string message,
                                 BitmapImage imageIcon = null)
    {
      if (this.ShowNotificationMessage != null)
      {
        this.ShowNotificationMessage(this, new ShowNotificationEvent
        (
          title,
          message,
          imageIcon
        ));

        return true;
      }

      return false;
    }
    #endregion methods
  }
}
