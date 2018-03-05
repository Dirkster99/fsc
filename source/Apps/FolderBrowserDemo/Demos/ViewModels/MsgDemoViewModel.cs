namespace FsContentDialogDemo.Demos.ViewModels
{
    using FsContentDialogDemo.ViewModels.Base;
    using MWindowDialogLib.ViewModels;
    using MWindowInterfacesLib.Events;
    using MWindowInterfacesLib.Interfaces;
    using System;
    using System.Windows.Input;

    /// <summary>
    /// This demo viewmodel implements a subset of the MsgBoxViewModel in order to drive
    /// custom dialogs in a very similar fashion. The difference here is that we use an
    /// int return value to indicate the value that a user has selected when clicking a
    /// button in a custom dialog.
    /// </summary>
    public class MsgDemoViewModel : DialogResultViewModel<int>, IBaseMetroDialogFrameViewModel<int>
    {
        #region fields
        private string _Message = string.Empty;
        private string _Title = string.Empty;

        private ICommand _CloseCommand;
        private bool? _DialogCloseResult = null;
        private bool _DialogCanCloseViaChrome = true;
        private bool _CloseWindowButtonVisibility = true;
        #endregion fields

        #region events
        /// <summary>
        /// This event can be used to react to the fact that a dialog has just been closed.
        /// </summary>
        public event EventHandler<DialogStateChangedEventArgs> DialogClosed;
        #endregion events

        #region properties
        #region IBaseMetroDialogFrameViewModel properties
        /// <summary>
        /// Title of message shown to the user (this is usally the Window title)
        /// </summary>
        public string Title
        {
            get
            {
                return _Title;
            }

            set
            {
                if (_Title != value)
                {
                    _Title = value;
                    RaisePropertyChanged(() => this.Title);
                }
            }
        }

        /// <summary>
        /// Use this property to determine whether the dialog can be closed
        /// without picking a choice (e.g. OK or Cancel) or not.
        /// </summary>
        public bool DialogCanCloseViaChrome
        {
            get
            {
                return _DialogCanCloseViaChrome;
            }
            set
            {
                if (_DialogCanCloseViaChrome != value)
                {
                    _DialogCanCloseViaChrome = value;
                    RaisePropertyChanged(() => this.DialogCanCloseViaChrome);
                }
            }
        }

        /// <summary>
        /// This can be used to close the attached view via bound ViewModel
        /// if WaitForKey is used to block the calling thread.
        /// 
        /// Source: http://stackoverflow.com/questions/501886/wpf-mvvm-newbie-how-should-the-viewmodel-close-the-form
        /// </summary>
        public bool? DialogCloseResult
        {
            get
            {
                return _DialogCloseResult;
            }

            protected set
            {
                if (_DialogCloseResult != value)
                {
                    _DialogCloseResult = value;
                    RaisePropertyChanged(() => DialogCloseResult);
                }
            }
        }

        /// <summary>
        /// Gets the close command that is invoked to close this dialog.
        /// The close command is invoked when the user clicks:
        /// 1) the dialogs (x) button or
        /// 2) a button that is, for example, labelled |Close|
        /// 
        /// - if any of the above is visible
        /// - if |Close| button is bound
        /// </summary>
        public virtual ICommand CloseCommand
        {
            get
            {
                if (this._CloseCommand == null)
                {
                    this._CloseCommand = new RelayCommand<object>((p) =>
                    {
                        SendDialogStateChangedEvent();

                        this.DialogCloseResult = true;
                    },
                    (p) =>
                    {
                        return this.DialogCanCloseViaChrome;
                    });
                }

                return this._CloseCommand;
            }
        }

        /// <summary>
        /// Determines whether the dialog's close (x) button is visible or not.
        /// </summary>
        public bool CloseWindowButtonVisibility
        {
            get { return _CloseWindowButtonVisibility; }

            set
            {
                if (_CloseWindowButtonVisibility != value)
                {
                    _CloseWindowButtonVisibility = value;
                    RaisePropertyChanged(() => this.CloseWindowButtonVisibility);
                }
            }
        }
        #endregion IBaseMetroDialogFrameViewModel properties

        /// <summary>
        /// Message content that tells the user what the problem is
        /// (why is it a problem, how can it be fixed,
        ///  and clicking which button will do what resolution [if any] etc...).
        /// </summary>
        public string Message
        {
            get
            {
                return _Message;
            }

            set
            {
                if (_Message != value)
                {
                    _Message = value;
                    RaisePropertyChanged(() => this.Message);
                }
            }
        }
        #endregion properties

        #region methods
        protected void SendDialogStateChangedEvent()
        {
            if (DialogClosed != null)
                DialogClosed(this, new DialogStateChangedEventArgs());
        }
        #endregion methods
    }
}
