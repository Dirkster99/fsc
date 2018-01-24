namespace FolderBrowser.ViewModels
{
    using FileSystemModels;
    using FileSystemModels.Models;
    using FileSystemModels.Models.FSItems;
    using FolderBrowser.BookmarkFolder;
    using FolderBrowser.Events;
    using FolderBrowser.Interfaces;
    using FolderBrowser.ViewModels.Messages;
    using FsCore.Collections;
    using FsCore.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using WPFProcessingLib;
    using WPFProcessingLib.Interfaces;

    /// <summary>
    /// A browser viewmodel is about managing activities and properties related
    /// to displaying a treeview that repesents folders in the file system.
    /// 
    /// This viewmodel is almost equivalent to the backend code needed to drive
    /// the Treeview that shows the items in the UI.
    /// </summary>
    internal class BrowserViewModel : FsCore.ViewModels.Base.ViewModelBase, IBrowserViewModel
    {
        #region fields
        private string mSelectedFolder;
        private bool mIsSpecialFoldersVisisble;

        private ICommand mExpandCommand;
        private ICommand mFolderSelectedCommand = null;
        private ICommand mSelectedFolderChangedCommand;
        private ICommand mOpenInWindowsCommand = null;
        private ICommand mCopyPathCommand = null;
        private ICommand mRenameCommand;
        private ICommand mStartRenameCommand;
        private ICommand mCreateFolderCommand;
        private ICommand mCancelBrowsingCommand;
        private ICommand mRefreshViewCommand;

        private bool mIsBrowsing = true;
        private IProcessViewModel mProcessor = null;
        private readonly IProcessViewModel mExpandProcessor = null;

        private bool mIsExpanding = false;

        private string mInitalPath;
        private bool _UpdateView;
        private bool _IsBrowseViewEnabled;
        #endregion fields

        #region constructor
        /// <summary>
        /// Standard class constructor
        /// </summary>
        public BrowserViewModel()
        {
            DisplayMessage = new DisplayMessageViewModel();
            BookmarkFolder = new EditFolderBookmark();
            InitializeSpecialFolders();

            mOpenInWindowsCommand = null;
            mCopyPathCommand = null;

            Folders = new ObservableSortedDictionary<string, IFolderViewModel>();

            mExpandProcessor = ProcessFactory.CreateProcessViewModel();
            mProcessor = ProcessFactory.CreateProcessViewModel();

            InitialPath = string.Empty;

            _UpdateView = true;
            _IsBrowseViewEnabled = true;
        }
        #endregion constructor

        #region browsing events
        /// <summary>
        /// Indicates when the viewmodel starts heading off somewhere else
        /// and when its done browsing to a new location.
        /// </summary>
        public event EventHandler<BrowsingChangedEventArgs> BrowsingChanged;
        #endregion browsing events

        #region properties
        /// <summary>
        /// This property determines whether the control
        /// is to be updated right now or not. Switching off updates at times
        /// can save performance when browsing long and deep paths with multiple
        /// levels - so we:
        /// 1) Switch off view updates
        /// 2) Browse the structure to a target
        /// 3) Switch on updates and update view at current/new location.
        /// </summary>
        public bool UpdateView
        {
            get
            {
                return _UpdateView;
            }

            set
            {
                if (_UpdateView != value)
                {
                    _UpdateView = value;
                    RaisePropertyChanged(() => UpdateView);
                }
            }
        }

        public bool IsBrowseViewEnabled
        {
            get
            {
                return _IsBrowseViewEnabled;
            }

            set
            {
                if (_IsBrowseViewEnabled != value)
                {
                    _IsBrowseViewEnabled = value;
                    RaisePropertyChanged(() => IsBrowseViewEnabled);
                }
            }
        }

        /// <summary>
        /// Gets whether the tree browser is currently processing
        /// a request for brwosing to a known location.
        /// </summary>
        public bool IsBrowsing
        {
            get
            {
                return mIsBrowsing;
            }

            private set
            {
                if (mIsBrowsing != value)
                {
                    mIsBrowsing = value;
                    RaisePropertyChanged(() => IsBrowsing);
                }
            }
        }

        /// <summary>
        /// Gets the list of drives and folders for display in treeview structure control.
        /// </summary>
        public ObservableSortedDictionary<string, IFolderViewModel> Folders
        {
            get;
            private set;
        }

        /// <summary>
        /// Get/set currently selected folder.
        /// </summary>
        public string SelectedFolder
        {
            get
            {
                return this.mSelectedFolder;
            }

            set
            {
                if (this.mSelectedFolder != value)
                {
                    this.mSelectedFolder = value;
                    this.RaisePropertyChanged(() => this.SelectedFolder);
                }
            }
        }

        /// <summary>
        /// Gets a command to cancel the current browsing process.
        /// </summary>
        public ICommand CancelBrowsingCommand
        {
            get
            {
                if (mCancelBrowsingCommand == null)
                {
                    mCancelBrowsingCommand = new RelayCommand<object>((p) =>
                    {
                        if (mProcessor != null)
                        {
                            if (mProcessor.IsCancelable == true)
                                mProcessor.Cancel();
                        }
                    },
                    (p) =>
                    {
                        if (IsBrowsing == true)
                        {
                            if (mProcessor.IsCancelable == true)
                                return mProcessor.IsProcessing;
                        }

                        return false;
                    });
                }

                return mCancelBrowsingCommand;
            }
        }

        /// <summary>
        /// Gets a command that will open the selected item with the current default application
        /// in Windows. The selected item (path to a file) is expected as FSItemVM parameter.
        /// (eg: Item is HTML file -> Open in Windows starts the web browser for viewing the HTML
        /// file if thats the currently associated Windows default application.
        /// </summary>
        public ICommand OpenInWindowsCommand
        {
            get
            {
                if (mOpenInWindowsCommand == null)
                    mOpenInWindowsCommand = new RelayCommand<object>(
                      (p) =>
                      {
                          var vm = p as FolderViewModel;

                          if (vm == null)
                              return;

                          if (string.IsNullOrEmpty(vm.FolderPath) == true)
                              return;

                          FileSystemCommands.OpenContainingFolder(vm.FolderPath);
                      });

                return mOpenInWindowsCommand;
            }
        }

        /// <summary>
        /// Gets a command that will copy the path of an item into the Windows Clipboard.
        /// The item (path to a file) is expected as FSItemVM parameter.
        /// </summary>
        public ICommand CopyPathCommand
        {
            get
            {
                if (mCopyPathCommand == null)
                    mCopyPathCommand = new RelayCommand<object>(
                      (p) =>
                      {
                          var vm = p as FolderViewModel;

                          if (vm == null)
                              return;

                          if (string.IsNullOrEmpty(vm.FolderPath) == true)
                              return;

                          FileSystemCommands.CopyPath(vm.FolderPath);
                      });

                return mCopyPathCommand;
            }
        }

        /// <summary>
        /// Gets a command that executes when the selected item in the treeview has changed.
        /// This updates a text property to inform other attached dependencies property controls
        /// about this change in selection state.
        /// </summary>
        public ICommand SelectedFolderChangedCommand
        {
            get
            {
                if (mSelectedFolderChangedCommand == null)
                {
                    mSelectedFolderChangedCommand = new RelayCommand<object>((p) =>
                    {
                        if (p is KeyValuePair<string, IFolderViewModel> == false)
                            return;

                        var item = (KeyValuePair<string, IFolderViewModel>)p;

                        SelectedFolder = item.Value.FolderPath;
                    });
                }

                return mSelectedFolderChangedCommand;
            }
        }

        /// <summary>
        /// Gets a command that executes when a user expands a tree view item node in the treeview.
        /// </summary>
        public ICommand ExpandCommand
        {
            get
            {
                if (mExpandCommand == null)
                {
                    mExpandCommand = new RelayCommand<object>((p) =>
                    {
                        var expandedItem = p as IFolderViewModel;

                        if (expandedItem != null && mIsExpanding == false)
                        {
                            if (expandedItem.ChildFolderIsDummy == true)
                                ExpandDummyFolder(expandedItem);
                        }
                    });
                }

                return mExpandCommand;
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
                if (this.mStartRenameCommand == null)
                    this.mStartRenameCommand = new RelayCommand<object>(it =>
                    {
                        var folder = it as FolderViewModel;

                        if (folder != null)
                            folder.RequestEditMode(InplaceEditBoxLib.Events.RequestEditEvent.StartEditMode);
                    },
                    (it) =>
                    {
                        var folder = it as FolderViewModel;

                        if (folder != null)
                        {
                            if (folder.IsReadOnly == true)
                                return false;
                        }

                        return true;
                    });

                return this.mStartRenameCommand;
            }
        }

        /// <summary>
        /// Renames the folder that is represented by this viewmodel.
        /// This command should be called directly by the implementing view
        /// since the new name of the folder is delivered as string.
        /// </summary>
        public ICommand RenameCommand
        {
            get
            {
                if (this.mRenameCommand == null)
                    this.mRenameCommand = new RelayCommand<object>(it =>
                    {
                        var tuple = it as Tuple<string, object>;

                        if (tuple != null)
                        {
                            var folderVM = tuple.Item2 as FolderViewModel;

                            if (tuple.Item1 != null && folderVM != null)
                            {
                                folderVM.RenameFolder(tuple.Item1);
                                this.SelectedFolder = folderVM.FolderPath;
                            }
                        }
                    });

                return this.mRenameCommand;
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
                if (this.mCreateFolderCommand == null)
                    this.mCreateFolderCommand = new RelayCommand<object>(async it =>
                    {
                        var folder = it as FolderViewModel;

                        if (folder == null)
                            return;

                        if (folder.IsExpanded == false)
                        {
                            folder.IsExpanded = true;

                            // Refresh child items if this has been expanded for the 1st time
                            if (folder.ChildFolderIsDummy == true)
                            {
                                var x = await RequeryChildItems(folder);
                            }
                        }

                        this.CreateFolderCommandNewFolder(folder);
                    });

                return this.mCreateFolderCommand;
            }
        }

        /// <summary>
        /// Get/set command to select the current folder.
        /// 
        /// This binding can be used for browsing to a certain folder
        /// e.g. Users Document folder.
        /// 
        /// Expected parameter: string containing a path to be browsed to.
        /// </summary>
        public ICommand FolderSelectedCommand
        {
            get
            {
                if (this.mFolderSelectedCommand == null)
                {
                    this.mFolderSelectedCommand = new RelayCommand<object>(p =>
                    {
                        try
                        {
                            string path = p as string;

                            if (string.IsNullOrEmpty(path) == true)
                            {
                                // Tell subscribers that we started browsing this directory
                                if (this.BrowsingChanged != null)
                                    this.BrowsingChanged(this, new BrowsingChangedEventArgs(new PathModel(path, FSItemType.Folder), false));

                                var item = p as FolderViewModel;

                                if (item != null)
                                    path = item.FolderPath;

                                if (string.IsNullOrEmpty(path) == true)
                                    return;
                            }

                            if (IsBrowsing == true)
                                return;

                            //// Set new path if it changed in comparison to old path
                            //// this.SetSelectedFolder(PathModel.NormalizePath(path), true);

                            mProcessor.StartCancelableProcess(cts =>
                            {
                                try
                                {
                                    IsBrowseViewEnabled = UpdateView = false;
                                    BrowsePath(path, false, cts);
                                }
                                finally
                                {
                                    IsBrowseViewEnabled = UpdateView = true;
                                }

                            }, ProcessinishedEvent, "This process is already running.");
                        }
                        catch
                        {
                        }
                    }, (p) =>
                        {
                            return ! IsBrowsing;
                        });
                }

                return this.mFolderSelectedCommand;
            }
        }

        /// <summary>
        /// Gets a command that will reload the folder view up to the
        /// selected path that is expected as <seealso cref="FolderViewModel"/>
        /// in the CommandParameter.
        /// 
        /// This command is particularly useful when users create/delete a folder
        /// and want to update the treeview accordingly.
        /// </summary>
        public ICommand RefreshViewCommand
        {
            get
            {
                if (this.mRefreshViewCommand == null)
                {
                    this.mRefreshViewCommand = new RelayCommand<object>(p =>
                    {
                        try
                        {
                            var item = p as FolderViewModel;

                            if (item == null)
                                return;

                            if (string.IsNullOrEmpty(item.FolderPath) == true)
                                return;

                            if (IsBrowsing == true)
                                return;

                            BrowsePath(item.FolderPath);
                        }
                        catch
                        {
                        }
                    }, (p) =>
                        {
                            return ! IsBrowsing;
                        });
                }

                return this.mRefreshViewCommand;
            }
        }

        /// <summary>
        /// Expand folder for the very first time (using the process background viewmodel).
        /// </summary>
        /// <param name="expandedItem"></param>
        private void ExpandDummyFolder(IFolderViewModel expandedItem)
        {
            if (expandedItem != null && mIsExpanding == false)
            {
                if (expandedItem.ChildFolderIsDummy == true)
                {
                    mIsExpanding = true;

                    mExpandProcessor.StartProcess(() =>
                    {
                        expandedItem.ClearFolders();                    // Requery sub-folders of this item
                        (expandedItem as FolderViewModel).LoadFolders();

                        ////expandedItem.IsSelected = true;
                        ////SelectedFolder = expandedItem.FolderPath;

                    }, ExpandProcessinishedEvent, "This process is already running.");
                }
            }
        }

        /// <summary>
        /// Methid executes when expand method is finished processing.
        /// </summary>
        /// <param name="processWasSuccessful"></param>
        /// <param name="exp"></param>
        /// <param name="caption"></param>
        private void ExpandProcessinishedEvent(bool processWasSuccessful, Exception exp, string caption)
        {
            mIsExpanding = false;
        }

        /// <summary>
        /// Requery all child items - this can be useful when we
        /// expand a folder for the very first time. Here we use task library with
        /// async to enable synchronization. This is for parts of other commands
        /// such as New Folder command which requires expansion of sub-folder
        /// items before actual New Folder command can execute.
        /// </summary>
        /// <param name="expandedItem"></param>
        /// <returns></returns>
        private async Task<bool> RequeryChildItems(FolderViewModel expandedItem)
        {
            await Task.Run(() => 
            {
                expandedItem.ClearFolders();  // Requery sub-folders of this item
                expandedItem.LoadFolders();
            });

            return true;
        }

        /// <summary>
        /// Gets a property to an object that is used to pop-up UI messages when errors occur.
        /// </summary>
        public IDisplayMessageViewModel DisplayMessage { get; private set; }

        /// <summary>
        /// Expose properties to commands that work with the bookmarking of folders.
        /// </summary>
        public IAddFolderBookmark BookmarkFolder { get; private set; }

        #region SpecialFolders property
        /// <summary>
        /// Gets a list of Special Windows Standard folders for display in view.
        /// </summary>
        public ObservableCollection<ICustomFolderItemViewModel> SpecialFolders { get; private set; }

        /// <summary>
        /// Gets whether the browser view should show a special folder control or not
        /// (A special folder control lets users browse to folders like 'My Documents'
        /// with a mouse click).
        /// </summary>
        public bool IsSpecialFoldersVisisble
        {
            get
            {
                return mIsSpecialFoldersVisisble;
            }

            private set
            {
                if (mIsSpecialFoldersVisisble != value)
                {
                    mIsSpecialFoldersVisisble = value;
                    RaisePropertyChanged(() => IsSpecialFoldersVisisble);
                }
            }
        }
        #endregion SpecialFolders property

        /// <summary>
        /// Get/set property to indicate the initial path when control
        /// starts up via Loading. The control attempts to change the
        /// current directory to the indicated directory if the
        /// ... method is called in the Loaded event of the
        /// <seealso cref="FolderBrowserDialog"/>.
        /// </summary>
        public string InitialPath
        {
            get
            {
                return mInitalPath;

            }

            set
            {
                if (mInitalPath != value)
                {
                    mInitalPath = value;
                    RaisePropertyChanged(() => InitialPath);
                }
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Call this method from the OnLoad method of the view
        /// in order to initialize a location as soon as the view
        /// is visible.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ResetBrowserStatus"></param>
        public void BrowsePath(
            string path,
            bool ResetBrowserStatus = true)
        {

            // Tell subscribers that we started browsing this directory
            if (this.BrowsingChanged != null)
                this.BrowsingChanged(this, new BrowsingChangedEventArgs(new PathModel(path, FSItemType.Folder), false));

            mProcessor.StartCancelableProcess(cts =>
            {
                try
                {
                    IsBrowseViewEnabled = UpdateView = false;
                    
                    ClearFoldersCollections();

                    if (cts != null)
                        cts.Token.ThrowIfCancellationRequested();

                    SetInitialDrives(cts);

                    if (cts != null)
                        cts.Token.ThrowIfCancellationRequested();

                    BrowsePath(path, ResetBrowserStatus, cts);
                }
                finally
                {
                    // Make sure that view updates at the end of browsing process
                    IsBrowseViewEnabled = UpdateView = true;
                }

            }, ProcessinishedEvent, "This process is already running.");
        }

        /// <summary>
        /// Determines whether the list of Windows special folder shortcut
        /// buttons (Music, Video etc) is visible or not.
        /// </summary>
        /// <param name="visible"></param>
        public void SetSpecialFoldersVisibility(bool visible)
        {
            this.IsSpecialFoldersVisisble = visible;
        }

        private bool BrowsePath(string path,
                                 bool ResetBrowserStatus,
                                 CancellationTokenSource cts = null)
        {
            if (ResetBrowserStatus == true)
                ClearBrowserStates();

            DisplayMessage.IsErrorMessageAvailable = false;

            if (System.IO.Directory.Exists(path) == false)
            {
                DisplayMessage.IsErrorMessageAvailable = true;
                DisplayMessage.Message = string.Format(FileSystemModels.Local.Strings.STR_ERROR_FOLDER_DOES_NOT_EXIST, path);
                return false;
            }

            SelectDirectory(new PathModel(path, FSItemType.Folder), cts);

            return true;
        }

        private void AddFolder(string name, IFolderViewModel folder)
        {
           Application.Current.Dispatcher.Invoke(() =>
           {
                Folders.Add(name.ToLower(), folder);
            });
        }

        private void InitializeSpecialFolders()
        {
            SpecialFolders = new ObservableCollection<ICustomFolderItemViewModel>();

            SpecialFolders.Add(new CustomFolderItemViewModel(Environment.SpecialFolder.Desktop));
            SpecialFolders.Add(new CustomFolderItemViewModel(Environment.SpecialFolder.MyDocuments));
            SpecialFolders.Add(new CustomFolderItemViewModel(Environment.SpecialFolder.MyMusic));
            SpecialFolders.Add(new CustomFolderItemViewModel(Environment.SpecialFolder.MyPictures));
            SpecialFolders.Add(new CustomFolderItemViewModel(Environment.SpecialFolder.MyVideos));
        }

        private FolderViewModel CreateFolderItem(PathModel model)
        {
            var f = new FolderViewModel(model);

            return f;
        }

        /// <summary>
        /// Initialize the treeview with a set of local drives
        /// currently available on the computer.
        /// </summary>
        private void SetInitialDrives(CancellationTokenSource cts = null)
        {
            foreach (var item in DriveModel.GetLogicalDrives())
            {
                if (cts != null)
                    cts.Token.ThrowIfCancellationRequested();

                var vmItem = CreateFolderItem(item.Model);

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    Folders.Add(item.Model.Name.ToLower(), vmItem);
                }));
            }
        }

        private void ClearFoldersCollections()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Folders.Clear();
            });
        }

        private void SelectDirectory(PathModel path,
                                     CancellationTokenSource cts = null)
        {
            string[] dirs = PathModel.GetDirectories(path.Path);
            List<IFolderViewModel> PathItems = new List<IFolderViewModel>();

            if (dirs == null)
                return;

            IFolderViewModel root;

            // Find drive in which we will have to insert this
            if (Folders.TryGetValue(dirs[0].ToLower(), out root) == false)
            {
                // Looks like this is a new drive - lets create it then ...
                root = CreateFolderItem(new PathModel(dirs[0], FSItemType.LogicalDrive));

                AddFolder(root.FolderName, root);
            }

            PathItems.Add(root);
            var folderItem = MergeFolders(root, dirs, dirs[0], PathItems);
            ////PathItems[PathItems.Count - 1].IsArmed = false;

            for (int i = 0; i < PathItems.Count; i++)
            {
                if (cts != null)
                    cts.Token.ThrowIfCancellationRequested();

                var folder1 = (PathItems[i] as FolderViewModel);
                folder1.IsExpanded = true;
            }

            ////PathItems[PathItems.Count - 1].IsExpanded = true;
            var folder = PathItems[PathItems.Count - 1] as FolderViewModel;
            folder.IsSelected = true;
            SelectedFolder = PathItems[PathItems.Count - 1].FolderPath;
        }

        private IFolderViewModel MergeFolders(IFolderViewModel root,
                                              string[] dirs,
                                              string accumulatedPath,
                                              List<IFolderViewModel> PathItems)
        {
            IFolderViewModel nextRoot = null;

            int i = 1;
            for (; i < dirs.Length; i++)
            {
                accumulatedPath = accumulatedPath + "/" + dirs[i];

                if (root.Folders.TryGetValue(dirs[i].ToLower(), out nextRoot) == false)
                {
                    (root as FolderViewModel).LoadFolders();     // Refresh children of this node

                    // Find Folder in which we will have to insert this
                    if (root.Folders.TryGetValue(dirs[i].ToLower(), out nextRoot) == false)
                        return root;
                }

                PathItems.Add(nextRoot);
                root = nextRoot;
            }

            return root;
        }

        /// <summary>
        /// Create a new folder underneath the given parent folder. This method creates
        /// the folder with a standard name (eg 'New folder n') on disk and selects it
        /// in editing mode to give users a chance for renaming it right away.
        /// </summary>
        /// <param name="parentFolder"></param>
        private void CreateFolderCommandNewFolder(FolderViewModel parentFolder)
        {
            if (parentFolder == null)
                return;

            // Cast this to access internal methods and setters
            var newSubFolder = parentFolder.CreateNewDirectory() as FolderViewModel;

            ////this.SelectedFolder = newSubFolder.FolderPath;
            ////this.SetSelectedFolder(newSubFolder.FolderPath, true);

            if (newSubFolder != null)
            {
                // Do this with low priority (thanks for that tip to Joseph Leung)
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, (Action)delegate
                {
                    newSubFolder.IsSelected = true;
                    newSubFolder.RequestEditMode(InplaceEditBoxLib.Events.RequestEditEvent.StartEditMode);
                });
            }
        }

        /// <summary>
        /// Get all child entries for a given path entry
        /// </summary>
        /// <param name="childFolders"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static IFolderViewModel Expand(ObservableSortedDictionary<string, IFolderViewModel> childFolders, string path)
        {
            if (string.IsNullOrEmpty(path) || childFolders.Count == 0)
            {
                return null;
            }

            string folderName = path;
            if (path.Contains('/') || path.Contains('\\'))
            {
                int idx = path.IndexOfAny(new char[] { '/', '\\' });
                folderName = path.Substring(0, idx);
                path = path.Substring(idx + 1);
            }
            else
            {
                path = null;
            }

            // bugfix: Folder names on windows are case insensitiv
            //var results = childFolders.Where<IFolderViewModel>(folder => string.Compare(folder.FolderName, folderName, true) == 0);
            IFolderViewModel results;
            childFolders.TryGetValue(folderName.ToLower(), out results);
            if (results != null)
            {
                IFolderViewModel fvm = results;

                var folder = fvm as FolderViewModel;  // Cast this to access internal setter
                folder.IsExpanded = true;

                var retVal = BrowserViewModel.Expand(fvm.Folders, path);
                if (retVal != null)
                {
                    return retVal;
                }
                else
                {
                    return fvm;
                }
            }

            return null;
        }

        /// <summary>
        /// Clear states of browser control (hide error message and other things that may not apply now)
        /// </summary>
        private void ClearBrowserStates()
        {
            DisplayMessage.Message = string.Empty;
            DisplayMessage.IsErrorMessageAvailable = false;
        }

        private void ProcessinishedEvent(bool processWasSuccessful, Exception exp, string caption)
        {
            IsBrowsing = false;

            // Tell subscribers that we finished browsing this directory
            if (this.BrowsingChanged != null)
                this.BrowsingChanged(this, new BrowsingChangedEventArgs( new PathModel(this.SelectedFolder, FSItemType.Folder), false));
        }
        #endregion methods
    }
}
