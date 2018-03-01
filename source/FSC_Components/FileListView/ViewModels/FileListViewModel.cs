namespace FileListView.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using FileListView.Interfaces;
    using FileListView.ViewModels.Base;
    using FileSystemModels;
    using FileSystemModels.Browse;
    using FileSystemModels.Events;
    using FileSystemModels.Interfaces;
    using FileSystemModels.Interfaces.Bookmark;
    using FileSystemModels.Models;
    using FileSystemModels.Models.FSItems.Base;
    using FileSystemModels.Utils;
    using UserNotification.ViewModel;

    /// <summary>
    /// Class implements a list of file items viewmodel for a given directory.
    /// </summary>
    internal class FileListViewModel : Base.ViewModelBase, IFileListViewModel
    {
        #region fields
        /// <summary>
        /// Log4Net facility to log errors and warnings for this class.
        /// </summary>
        protected static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _FilterString = string.Empty;
        private string[] _ParsedFilter = null;

        private bool _ShowFolders = true;
        private bool _ShowHidden = true;
        private bool _ShowIcons = true;
        private bool _IsFiltered = false;
        private LVItemViewModel _SelectedItem;

        private readonly IBrowseNavigation _BrowseNavigation = null;
        private readonly ObservableCollection<ILVItemViewModel> _CurrentItems = null;

        private ICommand _NavigateDownCommand = null;
        private ICommand _RefreshCommand = null;
        private ICommand _ToggleIsFolderVisibleCommand = null;
        private ICommand _ToggleIsIconVisibleCommand = null;
        private ICommand _ToggleIsHiddenVisibleCommand = null;
        private ICommand _ToggleIsFilteredCommand = null;

        private ICommand _OpenContainingFolderCommand = null;
        private ICommand _OpenInWindowsCommand = null;
        private ICommand _CopyPathCommand = null;

        private ICommand _RenameCommand = null;
        private ICommand _StartRenameCommand = null;
        private ICommand _CreateFolderCommand = null;

        private SendNotificationViewModel _Notification;
        private bool _IsExternallyBrowsing;
        private bool _IsBrowsing;
        #endregion fields

        #region constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        public FileListViewModel(IBrowseNavigation browseNavigation)
          : this()
        {
            this._BrowseNavigation = browseNavigation;

            this._ParsedFilter = BrowseNavigation.GetParsedFilters(this._FilterString);
        }

        /// <summary>
        /// Standard class constructor
        /// </summary>
        protected FileListViewModel()
        {
            BookmarkFolder = new EditFolderBookmarks();
            Notification = new SendNotificationViewModel();
            _CurrentItems = new ObservableCollection<ILVItemViewModel>();
        }
        #endregion constructor

        #region Events
        /// <summary>
        /// Event is fired to indicate that user wishes to open a file via this viewmodel.
        /// </summary>
        public event EventHandler<FileOpenEventArgs> OnFileOpen;

        /// <summary>
        /// Indicates when the viewmodel starts heading off somewhere else
        /// and when its done browsing to a new location.
        /// </summary>
        public event EventHandler<BrowsingEventArgs> BrowseEvent;
        #endregion

        #region properties
        /// <summary>
        /// Can only be set by the control if user started browser process
        /// 
        /// Use IsBrowsing and IsExternallyBrowsing to lock the controls UI
        /// during browse operations or display appropriate progress bar(s).
        /// </summary>
        public bool IsBrowsing
        {
            get
            {
                return _IsBrowsing;
            }

            protected set
            {
                if (_IsBrowsing != value)
                {
                    _IsBrowsing = value;
                    RaisePropertyChanged(() => IsBrowsing);
                }
            }
        }

        /// <summary>
        /// This should only be set by the controller that started the browser process.
        /// 
        /// Use IsBrowsing and IsExternallyBrowsing to lock the controls UI
        /// during browse operations or display appropriate progress bar(s).
        /// </summary>
        public bool IsExternallyBrowsing
        {
            get
            {
                return _IsExternallyBrowsing;
            }

            protected set
            {
                if (_IsExternallyBrowsing != value)
                {
                    _IsExternallyBrowsing = value;
                    RaisePropertyChanged(() => IsExternallyBrowsing);
                }
            }
        }

        /// <summary>
        /// Expose properties to commands that work with the bookmarking of folders.
        /// </summary>
        public IEditBookmarks BookmarkFolder { get; }

        /// <summary>
        /// Gets/sets list of files and folders to be displayed in connected view.
        /// </summary>
        public IEnumerable<ILVItemViewModel> CurrentItems
        {
            get
            {
                return _CurrentItems;
            }
        }

        /// <summary>
        /// Get/set select item in filelist viemodel. This property is used to bind
        /// the selectitem of the listbox and enable the BringIntoView behaviour
        /// to scroll a selected item into view.
        /// </summary>
        public LVItemViewModel SelectedItem
        {
            get
            {
                return this._SelectedItem;
            }

            set
            {
                Logger.DebugFormat("Set SelectedItem '{0}' property", value);

                if (this._SelectedItem != value)
                {
                    this._SelectedItem = value;
                    this.RaisePropertyChanged(() => this.SelectedItem);
                }
            }
        }

        /// <summary>
        /// Gets/sets whether the list of folders and files should include folders or not.
        /// </summary>
        public bool ShowFolders
        {
            get
            {
                return this._ShowFolders;
            }

            protected set
            {
                Logger.DebugFormat("Set ShowFolders '{0}' property", value);

                if (this._ShowFolders != value)
                {
                    this._ShowFolders = value;
                    this.RaisePropertyChanged(() => this.ShowFolders);
                }
            }
        }

        /// <summary>
        /// Gets/sets whether the list of folders and files includes hidden folders or files.
        /// </summary>
        public bool ShowHidden
        {
            get
            {
                return this._ShowHidden;
            }

            protected set
            {
                Logger.DebugFormat("Set ShowHidden '{0}' property", value);

                if (this._ShowHidden != value)
                {
                    this._ShowHidden = value;
                    this.RaisePropertyChanged(() => this.ShowHidden);
                }
            }
        }

        /// <summary>
        /// Gets/sets whether the list of folders and files includes an icon or not.
        /// </summary>
        public bool ShowIcons
        {
            get
            {
                return this._ShowIcons;
            }

            protected set
            {
                Logger.DebugFormat("Set ShowIcons '{0}' property", value);

                if (this._ShowIcons != value)
                {
                    this._ShowIcons = value;
                    this.RaisePropertyChanged(() => this.ShowIcons);
                }
            }
        }

        /// <summary>
        /// Gets whether the list of folders and files is filtered or not.
        /// </summary>
        public bool IsFiltered
        {
            get
            {
                return this._IsFiltered;
            }

            private set
            {
                Logger.DebugFormat("Set IsFiltered '{0}' property", value);

                if (this._IsFiltered != value)
                {
                    this._IsFiltered = value;
                    this.RaisePropertyChanged(() => this.IsFiltered);
                }
            }
        }

        /// <summary>
        /// Gets the current path this viewmodel assigned to look at.
        /// This property is not updated (must be polled) so its not
        /// a good idea to bind to it.
        /// </summary>
        public string CurrentFolder
        {
            get
            {
                Logger.DebugFormat("get CurrentFolder property");

                if (_BrowseNavigation != null)
                {
                    if (_BrowseNavigation.CurrentFolder != null)
                        return _BrowseNavigation.CurrentFolder.Path;
                }

                return null;
            }
        }

        #region commands
        /// <summary>
        /// Browse into a given a path.
        /// </summary>
        public ICommand NavigateDownCommand
        {
            get
            {
                if (this._NavigateDownCommand == null)
                    this._NavigateDownCommand = new RelayCommand<object>((p) =>
                    {
                        var info = p as LVItemViewModel;

                        if (info == null)
                            return;

                        try
                        {
                            if (info.ItemType == FSItemType.Folder || info.ItemType == FSItemType.LogicalDrive)
                            {
                                _BrowseNavigation.BrowseDown(info.ItemType, info.ItemPath);
                                var model = PathFactory.Create(info.ItemPath, info.ItemType);
                                PopulateView(model);
                            }
                            else
                            {
                                if (this.OnFileOpen != null && info.ItemType == FSItemType.File)
                                    this.OnFileOpen(this, new FileOpenEventArgs() { FileName = info.ItemPath });
                            }
                        }
                        catch
                        {
                        }
                    },
                    (p) =>
                    {
                        return (p as LVItemViewModel) != null;
                    });

                return this._NavigateDownCommand;
            }
        }

        /// <summary>
        /// Gets the command that updates the currently viewed
        /// list of directory items (files and sub-directories).
        /// </summary>
        public ICommand RefreshCommand
        {
            get
            {
                if (this._RefreshCommand == null)
                    this._RefreshCommand = new RelayCommand<object>((p) => this.PopulateView());

                return this._RefreshCommand;
            }
        }

        /// <summary>
        /// Toggles the visibiliy of folders in the folder/files listview.
        /// </summary>
        public ICommand ToggleIsFolderVisibleCommand
        {
            get
            {
                if (this._ToggleIsFolderVisibleCommand == null)
                    this._ToggleIsFolderVisibleCommand = new RelayCommand<object>((p) => this.ToggleIsFolderVisible_Executed());

                return this._ToggleIsFolderVisibleCommand;
            }
        }

        /// <summary>
        /// Toggles the visibiliy of icons in the folder/files listview.
        /// </summary>
        public ICommand ToggleIsIconVisibleCommand
        {
            get
            {
                if (this._ToggleIsIconVisibleCommand == null)
                    this._ToggleIsIconVisibleCommand = new RelayCommand<object>((p) => this.ToggleIsIconVisible_Executed());

                return this._ToggleIsIconVisibleCommand;
            }
        }

        /// <summary>
        /// Toggles the visibiliy of hidden files/folders in the folder/files listview.
        /// </summary>
        public ICommand ToggleIsHiddenVisibleCommand
        {
            get
            {
                if (this._ToggleIsHiddenVisibleCommand == null)
                    this._ToggleIsHiddenVisibleCommand = new RelayCommand<object>((p) => this.ToggleIsHiddenVisible_Executed());

                return this._ToggleIsHiddenVisibleCommand;
            }
        }
        #region Windows Integration FileSystem Commands

        /// <summary>
        /// Gets a command that will open the folder in which an item is stored.
        /// The item (path to a file) is expected as <seealso cref="FSItemViewModel"/> parameter.
        /// </summary>
        public ICommand OpenContainingFolderCommand
        {
            get
            {
                if (this._OpenContainingFolderCommand == null)
                    this._OpenContainingFolderCommand = new RelayCommand<object>(
                      (p) =>
                      {
                          var path = p as LVItemViewModel;

                          if (path == null)
                              return;

                          if (string.IsNullOrEmpty(path.ItemPath) == true)
                              return;

                          FileSystemCommands.OpenContainingFolder(path.ItemPath);
                      });

                return this._OpenContainingFolderCommand;
            }
        }

        /// <summary>
        /// Gets a command that will open the selected item with the current default application
        /// in Windows. The selected item (path to a file) is expected as <seealso cref="FSItemViewModel"/> parameter.
        /// (eg: Item is HTML file -> Open in Windows starts the web browser for viewing the HTML
        /// file if thats the currently associated Windows default application.
        /// </summary>
        public ICommand OpenInWindowsCommand
        {
            get
            {
                if (this._OpenInWindowsCommand == null)
                    this._OpenInWindowsCommand = new RelayCommand<object>(
                      (p) =>
                      {
                          var path = p as LVItemViewModel;

                          if (path == null)
                              return;

                          if (string.IsNullOrEmpty(path.ItemPath) == true)
                              return;

                          FileSystemCommands.OpenInWindows(path.ItemPath);
                      });

                return this._OpenInWindowsCommand;
            }
        }

        /// <summary>
        /// Gets a command that will copy the path of an item into the Windows Clipboard.
        /// The item (path to a file) is expected as <seealso cref="FSItemViewModel"/> parameter.
        /// </summary>
        public ICommand CopyPathCommand
        {
            get
            {
                if (this._CopyPathCommand == null)
                    this._CopyPathCommand = new RelayCommand<object>(
                      (p) =>
                      {
                          var path = p as LVItemViewModel;

                          if (path == null)
                              return;

                          if (string.IsNullOrEmpty(path.ItemPath) == true)
                              return;

                          FileListViewModel.CopyPathCommand_Executed(path.ItemPath);
                      });

                return this._CopyPathCommand;
            }
        }

        /// <summary>
        /// Toggles whether a file filter is currently applied on a list
        /// of files or not.
        /// </summary>
        public ICommand ToggleIsFilteredCommand
        {
            get
            {
                if (this._ToggleIsFilteredCommand == null)
                    this._ToggleIsFilteredCommand = new RelayCommand<object>(
                      (p) =>
                      {
                          this.SetIsFiltered(!this.IsFiltered);
                      });

                return this._ToggleIsFilteredCommand;
            }
        }
        #endregion Windows Integration FileSystem Commands

        /// <summary>
        /// Renames the folder that is represented by this viewmodel.
        /// This command should be called directly by the implementing view
        /// since the new name of the folder is delivered as string.
        /// </summary>
        public ICommand RenameCommand
        {
            get
            {
                if (this._RenameCommand == null)
                    this._RenameCommand = new RelayCommand<object>(it =>
                    {
                        var tuple = it as Tuple<string, object>;

                        if (tuple != null)
                        {
                            var folderVM = tuple.Item2 as LVItemViewModel;

                            if (tuple.Item1 != null && folderVM != null)
                                folderVM.RenameFileOrFolder(tuple.Item1);
                        }
                    });

                return this._RenameCommand;
            }
        }

        /// <summary>
        /// Starts the rename folder process by that renames the folder
        /// that is represented by this viewmodel.
        /// 
        /// This command implements an event that triggers the actual rename
        /// process in the connected view.
        /// </summary>
        public ICommand StartRenameCommand
        {
            get
            {
                if (this._StartRenameCommand == null)
                    this._StartRenameCommand = new RelayCommand<object>(it =>
                    {
                        var folder = it as LVItemViewModel;

                        if (folder != null)
                            folder.RequestEditMode(InplaceEditBoxLib.Events.RequestEditEvent.StartEditMode);
                    });

                return this._StartRenameCommand;
            }
        }

        /// <summary>
        /// Starts the create folder process by creating a new folder
        /// in the given location. The location is supplied as <seealso cref="System.Windows.Input.ICommandSource.CommandParameter"/>
        /// which is a <seealso cref="IFolderViewModel"/> item. So, the <seealso cref="IFolderViewModel"/> item
        /// is the parent of the new folder and the new folder is created with a standard name:
        /// 'New Folder n'. The new folder n is selected and in rename mode such that users can edit
        /// the name of the new folder right away.
        /// 
        /// This command implements an event that triggers the actual rename
        /// process in the connected view.
        /// </summary>
        public ICommand CreateFolderCommand
        {
            get
            {
                if (this._CreateFolderCommand == null)
                    this._CreateFolderCommand = new RelayCommand<object>(it =>
                    {
                        var folder = it as string;
                        this.CreateFolderCommandNewFolder(folder);
                    });

                return this._CreateFolderCommand;
            }
        }
        #endregion commands

        /// <summary>
        /// Gets a property that can be bound to the Notification dependency property
        /// of the <seealso cref="UserNotification.View.NotifyableContentControl"/>.
        /// Application developers can invoke the ShowNotification method to show a
        /// short pop-up message to the user. The pop-up message is shown in the
        /// vicinity of the content control that contains the real control (eg: ListBox)
        /// to which this notfication is related to.
        /// </summary>
        public SendNotificationViewModel Notification
        {
            get
            {
                return _Notification;
            }

            set
            {
                Logger.DebugFormat("Set Notification '{0}' property", value);

                if (this._Notification != value)
                {
                    this._Notification = value;
                    this.RaisePropertyChanged(() => this.Notification);
                }
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Controller can start browser process if IsBrowsing = false
        /// </summary>
        /// <param name="newPath"></param>
        /// <returns></returns>
        bool INavigateable.NavigateTo(IPathModel newPath)
        {
            return PopulateView(newPath, false);
        }

        /// <summary>
        /// Controller can start browser process if IsBrowsing = false
        /// </summary>
        /// <param name="newPath"></param>
        /// <returns></returns>
        async Task<bool> INavigateable.NavigateToAsync(IPathModel newPath)
        {
            return await Task.Run(() => { return PopulateView(newPath, false); });
        }

        /// <summary>
        /// Sets the IsExternalBrowsing state and cleans up any running processings
        /// if any. This method should only be called by an external controll instance.
        /// </summary>
        /// <param name="isBrowsing"></param>
        public void SetExternalBrowsingState(bool isBrowsing)
        {
            IsBrowsing = isBrowsing;
        }

        /// <summary>
        /// Applies a filter string (which can contain multiple
        /// alternative regular expression filter items) and updates
        /// the current display.
        /// </summary>
        /// <param name="filterText"></param>
        public void ApplyFilter(string filterText)
        {
            Logger.DebugFormat("ApplyFilter method with '{0}'", filterText);

            _FilterString = filterText;

            string[] tempParsedFilter = BrowseNavigation.GetParsedFilters(_FilterString);

            // Optimize nultiple requests for populating same view with unchanged filter away
            if (tempParsedFilter != this._ParsedFilter)
            {
                this._ParsedFilter = tempParsedFilter;
                this.PopulateView();
            }
        }

        /// <summary>
        /// Call this method to determine whether folders are part of the list of
        /// files and folders or not (list only files without folders).
        /// </summary>
        /// <param name="isFolderVisible"></param>
        public void SetIsFolderVisible(bool isFolderVisible)
        {
            Logger.DebugFormat("SetIsFolderVisible method with '{0}'", isFolderVisible);

            this.ShowFolders = isFolderVisible;
            this.PopulateView();
        }

        /// <summary>
        /// Call this method to determine whether folders are part of the list of
        /// files and folders or not (list only files without folders).
        /// </summary>
        /// <param name="isFiltered"></param>
        public void SetIsFiltered(bool isFiltered)
        {
            Logger.DebugFormat("SetIsFiltered method with '{0}'", isFiltered);

            this.IsFiltered = isFiltered;
            this.PopulateView();
        }

        /// <summary>
        /// Configure whether icons in listview should be shown or not.
        /// </summary>
        /// <param name="showIcons"></param>
        public void SetShowIcons(bool showIcons)
        {
            ShowIcons = showIcons;
        }

        /// <summary>
        /// Configure whether or not hidden files are shown in listview.
        /// </summary>
        /// <param name="showHiddenFiles"></param>
        public void SetShowHidden(bool showHiddenFiles)
        {
            ShowHidden = showHiddenFiles;
        }

        /// <summary>
        /// Fills the CurrentItems property for display in ItemsControl
        /// based view (ListBox, ListView etc.).
        /// 
        /// This method wraps a parameterized version of the same method 
        /// with a call that contains the standard data field.
        /// </summary>
        protected bool PopulateView(IPathModel newPathToNavigateTo = null,
                                    bool browseEvent = true)
        {
            Logger.DebugFormat("PopulateView method");

            bool result = false;
            IsBrowsing = true;
            try
            {
                if (newPathToNavigateTo != null && browseEvent == true)
                {
                    if (this.BrowseEvent != null)
                        this.BrowseEvent(this,
                                         new BrowsingEventArgs(newPathToNavigateTo, true));
                }

                if (newPathToNavigateTo != null)
                    SetCurrentLocation(newPathToNavigateTo.Path, false);

                CurrentItemClear();

                if (_BrowseNavigation.IsCurrentPathDirectory() == false)
                    return false;

                DirectoryInfo cur = _BrowseNavigation.GetDirectoryInfoOnCurrentFolder();

                if (cur.Exists == false)
                    return false;

                result = InternalPopulateView(_ParsedFilter, cur, ShowIcons);

                if (result == true)
                    SetCurrentLocation(newPathToNavigateTo.Path, true);

                return result;
            }
            catch
            {
            }
            finally
            {
                if (newPathToNavigateTo != null && browseEvent == true)
                {
                    if (this.BrowseEvent != null)
                        this.BrowseEvent(this,
                                         new BrowsingEventArgs(newPathToNavigateTo, false,
                                                              (result == true ? BrowseResult.Complete :
                                                                                BrowseResult.InComplete)));
                }

                IsBrowsing = false;
            }

            return result;
        }

        #region FileSystem Commands
        /// <summary>
        /// A hyperlink has been clicked. Start a web browser and let it browse to where this points to...
        /// </summary>
        /// <param name="sFileName"></param>
        private static void CopyPathCommand_Executed(string sFileName)
        {
            if (string.IsNullOrEmpty(sFileName) == true)
                return;

            try
            {
                System.Windows.Clipboard.SetText(sFileName);
            }
            catch
            {
            }
        }
        #endregion FileSystem Commands

        private void SetCurrentLocation(string path, bool bHistory)
        {
            _BrowseNavigation.SetCurrentFolder(path, bHistory);

            RaisePropertyChanged(() => CurrentFolder);
        }

        /// <summary>
        /// Fills the CurrentItems property for display in ItemsControl
        /// based view (ListBox, ListView etc.)
        /// 
        /// This version is parameterized since the filterstring can be parsed
        /// seperately and does not need to b parsed each time when this method
        /// executes.
        /// </summary>
        private bool InternalPopulateView(string[] filterString
                                        , DirectoryInfo cur
                                        , bool showIcons)
        {
            Logger.DebugFormat("PopulateView method with filterString parameter");

            try
            {
                // Retrieve and add (filtered) list of directories
                if (this.ShowFolders)
                {
                    string[] directoryFilter = null;

                    //// if (filterString != null)
                    ////  directoryFilter = new ArrayList(filterString).ToArray() as string[];
                    directoryFilter = null;

                    foreach (DirectoryInfo dir in cur.SelectDirectoriesByFilter(directoryFilter))
                    {
                        if (dir.Attributes.HasFlag(FileAttributes.Hidden) == true)
                        {
                            if (this.ShowHidden == false)
                            {
                                if ((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                                    continue;
                            }
                        }

                        var info = new LVItemViewModel(dir.FullName,
                                                       FSItemType.Folder, dir.Name, showIcons);

                        CurrentItemAdd(info);
                    }
                }

                if (this.IsFiltered == false) // Do not apply the filter if it is not enabled
                    filterString = null;

                // Retrieve and add (filtered) list of files in current directory
                foreach (FileInfo f in cur.SelectFilesByFilter(filterString))
                {
                    if (this.ShowHidden == false)
                    {
                        if (f.Attributes.HasFlag(FileAttributes.Hidden) == true)
                        {
                            if ((f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                                continue;
                        }
                    }

                    var info = new LVItemViewModel(f.FullName,
                                                   FSItemType.File, f.Name, showIcons);

                    CurrentItemAdd(info);
                }

                return true;
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// Clears the collection of current file/folder items and makes sure
        /// the operation is performed on the dispatcher thread.
        /// </summary>
        private void CurrentItemClear()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _CurrentItems.Clear();
            });
        }

        /// <summary>
        /// Adds another item into the collection of file/folder items
        /// and ensures the operation is performed on the dispatcher thread.
        /// </summary>
        private void CurrentItemAdd(LVItemViewModel item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _CurrentItems.Add(item);
            });
        }

        private void ToggleIsFolderVisible_Executed()
        {
            this.ShowFolders = !this.ShowFolders;
            this.PopulateView();
        }

        private void ToggleIsIconVisible_Executed()
        {
            this.ShowIcons = !this.ShowIcons;
            this.PopulateView();
        }

        private void ToggleIsHiddenVisible_Executed()
        {
            this.ShowHidden = !this.ShowHidden;
            this.PopulateView();
        }

        /// <summary>
        /// Create a new folder underneath the given parent folder. This method creates
        /// the folder with a standard name (eg 'New folder n') on disk and selects it
        /// in editing mode to give users a chance for renaming it right away.
        /// </summary>
        /// <param name="parentFolder"></param>
        private void CreateFolderCommandNewFolder(string parentFolder)
        {
            Logger.DebugFormat("CreateFolderCommandNewFolder method with '{0}'", parentFolder);

            if (parentFolder == null)
                return;

            LVItemViewModel newSubFolder = this.CreateNewDirectory(parentFolder);

            if (newSubFolder != null)
            {
                this.SelectedItem = newSubFolder;

                // Do this with low priority (thanks for that tip to Joseph Leung)
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                {
                    newSubFolder.RequestEditMode(InplaceEditBoxLib.Events.RequestEditEvent.StartEditMode);
                });
            }
        }

        /// <summary>
        /// Creates a new folder with a standard name (eg: 'New folder n').
        /// </summary>
        /// <returns></returns>
        private LVItemViewModel CreateNewDirectory(string parentFolder)
        {
            Logger.DebugFormat("CreateNewDirectory method with '{0}'", parentFolder);

            try
            {
                var model = PathFactory.Create(parentFolder, FSItemType.Folder);
                var newSubFolder = PathFactory.CreateDir(model);

                if (newSubFolder != null)
                {
                    var newFolderVM = new LVItemViewModel(newSubFolder.Path, newSubFolder.PathType, newSubFolder.Name);

                    _CurrentItems.Add(newFolderVM);

                    return newFolderVM;
                }
            }
            catch (Exception exp)
            {
                Logger.Error(string.Format("Creating new folder underneath '{0}' was not succesful.", parentFolder), exp);
                this.Notification.ShowNotification(FileSystemModels.Local.Strings.STR_CREATE_FOLDER_ERROR_TITLE,
                                                   exp.Message);
            }

            return null;
        }
        #endregion methods
    }
}
