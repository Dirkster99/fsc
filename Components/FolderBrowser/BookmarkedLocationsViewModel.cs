namespace FolderBrowser.ViewModels
{
    using FileSystemModels.Models;
    using FileSystemModels.Models.FSItems;
    using FileSystemModels.Utils;
    using FolderBrowser.Dialogs.Interfaces;
    using FolderBrowser.Events;
    using FolderBrowser.FileSystem.Interfaces;
    using FolderBrowser.FileSystem.ViewModels;
    using FsCore.ViewModels;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Windows.Input;

    /// <summary>
    /// Implement viewmodel for management of recent folder locations.
    /// </summary>
    internal class BookmarkedLocationsViewModel : FsCore.ViewModels.Base.ViewModelBase, IBookmarkedLocationsViewModel
    {
        #region fields
        private IFSItemViewModel mSelectedItem;

        private object mLockObject = new object();

        private RelayCommand<object> mChangeOfDirectoryCommand;
        private RelayCommand<object> mRemoveFolderBookmark;
        private bool mIsOpen;
        #endregion fields

        #region constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        public BookmarkedLocationsViewModel()
        {
            this.DropDownItems = new ObservableCollection<IFSItemViewModel>();
            this.IsOpen = false;
        }

        /// <summary>
        /// Copy class constructor
        /// </summary>
        /// <param name="copyThis"></param>
        public BookmarkedLocationsViewModel(BookmarkedLocationsViewModel copyThis)
            : this()
        {
            if (copyThis == null)
                return;

            ////this.IsOpen = copyThis.IsOpen; this could magically open other drop downs :-)

            foreach (var item in copyThis.DropDownItems)
                DropDownItems.Add(new FSItemViewModel(item as FSItemViewModel));

            // Select quivalent item in target collection
            if (copyThis.SelectedItem != null)
            {
                string fullPath = copyThis.SelectedItem.FullPath;
                var result = DropDownItems.SingleOrDefault(item => fullPath == item.FullPath);

                if (result != null)
                    SelectedItem = result;
            }
        }
        #endregion constructor

        #region events
        /// <summary>
        /// Event is fired whenever a change of the current directory is requested.
        /// </summary>
        public event EventHandler<FolderChangedEventArgs> RequestChangeOfDirectory;
        #endregion events

        #region properties
        /// <summary>
        /// Request a change of current directory to the directory
        /// stated in <seealso cref="FSItemViewModel"/> in CommandParameter.
        /// </summary>
        public ICommand ChangeOfDirectoryCommand
        {
            get
            {
                if (this.mChangeOfDirectoryCommand == null)
                    this.mChangeOfDirectoryCommand = new RelayCommand<object>((p) =>
                    {
                        var param = p as FSItemViewModel;

                        if (param != null)
                            this.ChangeOfDirectoryCommand_Executed(param);
                    });

                return this.mChangeOfDirectoryCommand;
            }
        }

        /// <summary>
        /// Command removes a folder bookmark from the list of
        /// currently bookmarked folders. Required command parameter
        /// is of type <seealso cref="FSItemViewModel"/>.
        /// </summary>
        public ICommand RemoveFolderBookmark
        {
            get
            {
                if (this.mRemoveFolderBookmark == null)
                    this.mRemoveFolderBookmark = new RelayCommand<object>((p) =>
                    {
                        var param = p as FSItemViewModel;

                        if (param != null)
                            this.RemoveFolderBookmark_Executed(param);
                    });

                return this.mRemoveFolderBookmark;
            }
        }

        /// <summary>
        /// <inheritedoc />
        /// </summary>
        public ObservableCollection<IFSItemViewModel> DropDownItems { get; private set; }

        /// <summary>
        /// Gets/set the selected item of the RecentLocations property.
        /// 
        /// This should be bound by the view (ItemsControl) to find the SelectedItem here.
        /// </summary>
        public IFSItemViewModel SelectedItem
        {
            get
            {
                return this.mSelectedItem;
            }

            set
            {
                if (this.mSelectedItem != value)
                {
                    this.mSelectedItem = value;
                    this.RaisePropertyChanged(() => this.SelectedItem);
                }
            }
        }

        /// <summary>
        /// <inheritedoc />
        /// </summary>
        public bool IsOpen
        {
            get
            {
                return this.mIsOpen;
            }

            set
            {
                if (this.mIsOpen != value)
                {
                    this.mIsOpen = value;
                    this.RaisePropertyChanged(() => this.IsOpen);
                }
            }
        }
        #endregion properties

        #region methods
        /// <summary>
        /// Gets a data copy of the current object. Object specific fields, like events
        /// and their handlers are not copied.
        /// </summary>
        /// <returns></returns>
        public IBookmarkedLocationsViewModel CloneBookmark()
        {
            return new BookmarkedLocationsViewModel(this);
        }

        /// <summary>
        /// Gets a data copy of the current object. Object specific fields, like events
        /// and their handlers are not copied.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return CloneBookmark();
        }

        /// <summary>
        /// Add a recent folder location into the collection of recent folders.
        /// This collection can then be used in the folder combobox drop down
        /// list to store user specific customized folder short-cuts.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="selectNewItem"></param>
        public void AddFolder(string folderPath,
                                      bool selectNewItem = false)
        {
            lock (this.mLockObject)
            {
                if ((folderPath = PathModel.ExtractDirectoryRoot(folderPath)) == null)
                    return;

                // select this path if its already there
                var results = this.DropDownItems.Where<IFSItemViewModel>(folder => string.Compare(folder.FullPath, folderPath, true) == 0);

                // Do not add this twice
                if (results != null)
                {
                    if (results.Count() != 0)
                    {
                        if (selectNewItem == true)
                            this.SelectedItem = results.First();

                        return;
                    }
                }

                var folderVM = this.CreateFSItemVMFromString(folderPath);

                this.DropDownItems.Add(folderVM);

                if (selectNewItem == true)
                    this.SelectedItem = folderVM;
            }
        }

        /// <summary>
        /// Remove a recent folder location from the collection of recent folders.
        /// This collection can then be used in the folder combobox drop down
        /// list to store user specific customized folder short-cuts.
        /// </summary>
        /// <param name="folderPath"></param>
        public void RemoveFolder(PathModel folderPath)
        {
            lock (this.mLockObject)
            {
                if (folderPath == null)
                    return;

                // Find all items that satisfy the query match and remove them
                // (This statement requires a Linq extension method to work)
                // See FileSystemModels.Utils for more details
                this.DropDownItems.Remove(i => string.Compare(folderPath.Path, i.FullPath, true) == 0);
            }
        }

        /// <summary>
        /// Removes all data items from the current collection of recent folders.
        /// </summary>
        public void ClearFolderCollection()
        {
            if (this.DropDownItems != null)
                this.DropDownItems.Clear();
        }

        /// <summary>
        /// Removes all data items from the current collection of recent folders.
        /// </summary>
        public void RemoveFolder(string path)
        {
            try
            {
                this.RemoveFolder(new PathModel(path, FSItemType.Folder));
            }
            catch
            {
            }
        }

        private void ChangeOfDirectoryCommand_Executed(FSItemViewModel path)
        {
            if (path == null)
                return;

            this.IsOpen = false;
            this.SelectedItem = path;

            if (this.RequestChangeOfDirectory != null)
                this.RequestChangeOfDirectory(this, new FolderChangedEventArgs(new PathModel(path.FullPath, FSItemType.Folder)));
        }

        private FSItemViewModel CreateFSItemVMFromString(string folderPath)
        {
            ////folderPath = System.IO.Path.GetDirectoryName(folderPath);

            string displayName = string.Empty;

            try
            {
                displayName = System.IO.Path.GetFileName(folderPath);
            }
            catch
            {
                displayName = folderPath;
            }

            if (displayName.Trim() == string.Empty)
                displayName = folderPath;

            return new FSItemViewModel(folderPath, FSItemType.Folder, displayName, true);
        }

        /// <summary>
        /// Method removes a folder bookmark from the list of currently bookmarked folders.
        /// </summary>
        /// <param name="param"></param>
        private void RemoveFolderBookmark_Executed(FSItemViewModel param)
        {
            this.RemoveFolder(param.GetModel);
        }
        #endregion methods
    }
}
