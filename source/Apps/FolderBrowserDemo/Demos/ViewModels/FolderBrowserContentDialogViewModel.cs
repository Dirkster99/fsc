namespace FsContentDialogDemo.Demos.ViewModels
{
    using MWindowInterfacesLib.Events;
    using MWindowInterfacesLib.Interfaces;
    using System;
    using System.Windows.Input;

    public class FolderBrowserContentDialogViewModel : FsContentDialogDemo.ViewModels.Base.ViewModelBase,
                                                       IBaseMetroDialogFrameViewModel<int>
    {
        #region fields
        private const string _titleDefault = "Select a folder";
        private string _title;
        private readonly object _FolderBrowser;

        private bool? _DialogCloseResult;

        private ICommand _CloseCommand;
        private bool _IsEnabledClose = true;
        private string _CloseButtonText;

        private string _CancelButtonText;
        private ICommand _OKCommand;
        #endregion fields

        #region constructors
        public FolderBrowserContentDialogViewModel(
              object folderBrowser
            , string title = null)
            : this()
        {
            Title = title;
            _FolderBrowser = folderBrowser;
        }

        protected FolderBrowserContentDialogViewModel()
        {
            _FolderBrowser = null;
            _title = _titleDefault;

            _DialogCloseResult = null;

            _CloseButtonText = "_OK";
            _CancelButtonText = "_Cancel";
        }
        #endregion constructors

        #region events
        /// <summary>
        /// This event can be used to react to the fact that a dialog has just been closed.
        /// </summary>
        public event EventHandler<DialogStateChangedEventArgs> DialogClosed;
        #endregion events

        #region properties
        /// <summary>
        /// Gets all properties of the hosted folder browser
        /// viewmodel displayed in the FolderBrowser view portion.
        /// </summary>
        public object FolderBrowser
        {
            get
            {
                return _FolderBrowser;
            }
        }

        /// <summary>
        /// Gets/sets the title (if any) of the dialog to be displayed.
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                if (_title != value)
                {
                    // Make sure that title can never be empty or null
                    if (string.IsNullOrEmpty(value) == true)
                        _title = _titleDefault;
                    else
                        _title = value;

                    RaisePropertyChanged(() => this.Title);
                }
            }
        }

        /// <summary>
        /// Use this property to determine whether the dialog can be closed
        /// without picking a choice (e.g. OK or Cancel) or not.
        /// </summary>
        public bool DialogCanCloseViaChrome { get { return true; } }

        /// <summary>
        /// Use this property to tell the view that the viewmodel would like to close now.
        /// </summary>
        public bool? DialogCloseResult
        {
            get
            {
                return _DialogCloseResult;
            }

            set
            {
                if (_DialogCloseResult != value)
                {
                    _DialogCloseResult = value;

                    RaisePropertyChanged(() => this.DialogCloseResult);
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
        public ICommand CloseCommand
        {
            get
            {
                if (this._CloseCommand == null)
                {
                    this._CloseCommand = new FsContentDialogDemo.ViewModels.Base.RelayCommand<object>((p) =>
                    {
                        Result = DialogIntResults.CANCEL; // OK Button

                        SendDialogStateChangedEvent();

                        DialogCloseResult = true;
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
        public bool CloseWindowButtonVisibility { get { return (true); } }

        /// <summary>
        /// Get the resulting button (that has been clicked
        /// by the user) or result event when working with the dialog.
        /// </summary>
        public int Result { get; private set; }

        /// <summary>
        /// Gets the default value for the result datatype.
        /// </summary>
        public int DefaultResult { get; }

        /// <summary>
        /// Gets property to determine dialog result when user closes it
        /// via F4 or Window Close (X) button when using window chrome.
        /// </summary>
        public int DefaultCloseResult { get; }

        public bool IsEnabledClose
        {
            get { return _IsEnabledClose; }
            protected set
            {
                if (_IsEnabledClose != value)
                {
                    _IsEnabledClose = value;
                    RaisePropertyChanged(() => this.IsEnabledClose);
                }
            }
        }

        public string CloseButtonText
        {
            get { return _CloseButtonText; }
            set
            {
                if (_CloseButtonText != value)
                {
                    _CloseButtonText = value;
                    RaisePropertyChanged(() => this.CloseButtonText);
                }
            }
        }

        public string CancelButtonText
        {
            get { return _CancelButtonText; }
            set
            {
                if (_CancelButtonText != value)
                {
                    _CancelButtonText = value;
                    RaisePropertyChanged(() => this.CancelButtonText);
                }
            }
        }

        public bool IsCancelable
        {
            get { return true; }
        }

        public ICommand OKCommand
        {
            get
            {
                if (this._OKCommand == null)
                {
                    this._OKCommand = new FsContentDialogDemo.ViewModels.Base.RelayCommand<object>((p) =>
                    {
                        Result = DialogIntResults.OK; // Cancel Button clicked

                        SendDialogStateChangedEvent();

                        DialogCloseResult = true;
                    },
                    (p) =>
                    {
                        return this.DialogCanCloseViaChrome;
                    });
                }

                return this._OKCommand;
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
