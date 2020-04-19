namespace FileListView.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Threading;
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
		/// Defines the delimitor for multiple regular expression filter statements.
		/// eg: "*.txt;*.ini"
		/// </summary>
		public const char FilterSplitCharacter = ';';

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
		private IPathModel _CurrentFolder = null;

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
		/// Standard class constructor
		/// </summary>
		public FileListViewModel()
		{
			_ParsedFilter = this.GetParsedFilters(_FilterString);
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
				return _SelectedItem;
			}

			set
			{
				Logger.DebugFormat("Set SelectedItem '{0}' property", value);

				if (_SelectedItem != value)
				{
					_SelectedItem = value;
					RaisePropertyChanged(() => SelectedItem);
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
				return _ShowFolders;
			}

			protected set
			{
				Logger.DebugFormat("Set ShowFolders '{0}' property", value);

				if (_ShowFolders != value)
				{
					_ShowFolders = value;
					RaisePropertyChanged(() => ShowFolders);
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
				return _ShowHidden;
			}

			protected set
			{
				Logger.DebugFormat("Set ShowHidden '{0}' property", value);

				if (_ShowHidden != value)
				{
					_ShowHidden = value;
					RaisePropertyChanged(() => ShowHidden);
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
				return _ShowIcons;
			}

			protected set
			{
				Logger.DebugFormat("Set ShowIcons '{0}' property", value);

				if (_ShowIcons != value)
				{
					_ShowIcons = value;
					RaisePropertyChanged(() => ShowIcons);
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
				return _IsFiltered;
			}

			private set
			{
				Logger.DebugFormat("Set IsFiltered '{0}' property", value);

				if (_IsFiltered != value)
				{
					_IsFiltered = value;
					RaisePropertyChanged(() => IsFiltered);
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

				if (_CurrentFolder != null)
					return _CurrentFolder.Path;

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
								////                                try
								////                                {
								////                                    _CurrentFolder = PathFactory.Create(info.ItemPath);
								////                                }
								////                                catch
								////                                {
								////                                }

								var model = PathFactory.Create(info.ItemPath, info.ItemType);
								PopulateView(new BrowseRequest(model), true);
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
				if (_RefreshCommand == null)
					_RefreshCommand =
					  new RelayCommand<object>((p) => PopulateCurrentView(true));

				return _RefreshCommand;
			}
		}

		/// <summary>
		/// Toggles the visibiliy of folders in the folder/files listview.
		/// </summary>
		public ICommand ToggleIsFolderVisibleCommand
		{
			get
			{
				if (_ToggleIsFolderVisibleCommand == null)
					_ToggleIsFolderVisibleCommand =
					  new RelayCommand<object>((p) => SetIsFolderVisible(!ShowFolders));

				return _ToggleIsFolderVisibleCommand;
			}
		}

		/// <summary>
		/// Toggles the visibiliy of icons in the folder/files listview.
		/// </summary>
		public ICommand ToggleIsIconVisibleCommand
		{
			get
			{
				if (_ToggleIsIconVisibleCommand == null)
					_ToggleIsIconVisibleCommand =
					  new RelayCommand<object>((p) => SetShowIcons(!ShowIcons));

				return _ToggleIsIconVisibleCommand;
			}
		}

		/// <summary>
		/// Toggles the visibiliy of hidden files/folders in the folder/files listview.
		/// </summary>
		public ICommand ToggleIsHiddenVisibleCommand
		{
			get
			{
				if (_ToggleIsHiddenVisibleCommand == null)
					_ToggleIsHiddenVisibleCommand =
					  new RelayCommand<object>((p) => SetShowHidden(!ShowHidden));

				return _ToggleIsHiddenVisibleCommand;
			}
		}
		#region Windows Integration FileSystem Commands

		/// <summary>
		/// Gets a command that will open the folder in which an item is stored.
		/// The item (path to a file) is expected as <seealso cref="ILVItemViewModel"/> parameter.
		/// </summary>
		public ICommand OpenContainingFolderCommand
		{
			get
			{
				if (_OpenContainingFolderCommand == null)
					_OpenContainingFolderCommand = new RelayCommand<object>(
					  (p) =>
					  {
						  var path = p as ILVItemViewModel;

						  if (path == null)
							  return;

						  if (string.IsNullOrEmpty(path.ItemPath) == true)
							  return;

						  FileSystemCommands.OpenContainingFolder(path.ItemPath);
					  });

				return _OpenContainingFolderCommand;
			}
		}

		/// <summary>
		/// Gets a command that will open the selected item with the current default application
		/// in Windows. The selected item (path to a file) is expected as
		/// <seealso cref="ILVItemViewModel"/> parameter.
		/// (eg: Item is HTML file -> Open in Windows starts the web browser for viewing the HTML
		/// file if thats the currently associated Windows default application.
		/// </summary>
		public ICommand OpenInWindowsCommand
		{
			get
			{
				if (_OpenInWindowsCommand == null)
					_OpenInWindowsCommand = new RelayCommand<object>(
					  (p) =>
					  {
						  var path = p as ILVItemViewModel;

						  if (path == null)
							  return;

						  if (string.IsNullOrEmpty(path.ItemPath) == true)
							  return;

						  FileSystemCommands.OpenInWindows(path.ItemPath);
					  });

				return _OpenInWindowsCommand;
			}
		}

		/// <summary>
		/// Gets a command that will copy the path of an item into the Windows Clipboard.
		/// The item (path to a file) is expected as <seealso cref="ILVItemViewModel"/> parameter.
		/// </summary>
		public ICommand CopyPathCommand
		{
			get
			{
				if (_CopyPathCommand == null)
					_CopyPathCommand = new RelayCommand<object>(
					  (p) =>
					  {
						  var path = p as ILVItemViewModel;

						  if (path == null)
							  return;

						  if (string.IsNullOrEmpty(path.ItemPath) == true)
							  return;

						  FileListViewModel.CopyPathCommand_Executed(path.ItemPath);
					  });

				return _CopyPathCommand;
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
				if (_ToggleIsFilteredCommand == null)
					_ToggleIsFilteredCommand = new RelayCommand<object>(
					  (p) =>
					  {
						  SetIsFiltered(!this.IsFiltered);
					  });

				return _ToggleIsFilteredCommand;
			}
		}
		#endregion Windows Integration FileSystem Commands

		/// <summary>
		/// Renames the folder that is delivered in a Tuple parameter
		/// containing the new string and the <see cref="ILVItemViewModel"/> item
		/// who's rename method is to be called in this command.
		/// </summary>
		public ICommand RenameCommand
		{
			get
			{
				if (_RenameCommand == null)
					_RenameCommand = new RelayCommand<object>(it =>
					{
						var tuple = it as Tuple<string, object>;

						if (tuple != null)
						{
							var folderVM = tuple.Item2 as ILVItemViewModel;

							if (tuple.Item1 != null && folderVM != null)
								folderVM.RenameFileOrFolder(tuple.Item1);
						}
					});

				return _RenameCommand;
			}
		}

		/// <summary>
		/// Starts the rename folder process by that renames the folder
		/// that is represented by this viewmodel.
		/// 
		/// This command implements an event that triggers the actual rename
		/// process in the connected view.
		/// 
		/// The expected parameter is a <see cref="LVItemViewModel"/>
		/// that can be supplied as <see cref="ILVItemViewModel"/>.
		/// </summary>
		public ICommand StartRenameCommand
		{
			get
			{
				if (_StartRenameCommand == null)
					_StartRenameCommand = new RelayCommand<object>(it =>
					{
						var folder = it as LVItemViewModel;

						if (folder != null)
							folder.RequestEditMode(InplaceEditBoxLib.Events.RequestEditEvent.StartEditMode);
					});

				return _StartRenameCommand;
			}
		}

		/// <summary>
		/// Starts the create folder process by creating a new folder
		/// in the given location. The location is supplied as string
		/// 
		/// So, the string is the name of the new folder that is created underneath this folder.
		/// The new folder is created with a standard name:
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
				if (_CreateFolderCommand == null)
					_CreateFolderCommand = new RelayCommand<object>(it =>
					{
						var folder = it as string;
						CreateFolderCommandNewFolder(folder);
					});

				return _CreateFolderCommand;
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

				if (_Notification != value)
				{
					_Notification = value;
					RaisePropertyChanged(() => Notification);
				}
			}
		}
		#endregion properties

		#region methods
		/// <summary>
		/// Controller can start browser process if IsBrowsing = false
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		FinalBrowseResult INavigateable.NavigateTo(BrowseRequest request)
		{
			return PopulateView(request, false);
		}

		/// <summary>
		/// Controller can start browser process if IsBrowsing = false
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		async Task<FinalBrowseResult> INavigateable.NavigateToAsync(BrowseRequest request)
		{
			return await Task.Run(() => { return PopulateView(request, false); });
		}

		/// <summary>
		/// Sets the IsExternalBrowsing state and cleans up any running processings
		/// if any. This method should only be called by an external controll instance.
		/// </summary>
		/// <param name="isBrowsing"></param>
		void INavigateable.SetExternalBrowsingState(bool isBrowsing)
		{
			IsExternallyBrowsing = isBrowsing;
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

			string[] tempParsedFilter = GetParsedFilters(_FilterString);

			// Optimize nultiple requests for populating same view with unchanged filter away
			if (tempParsedFilter != _ParsedFilter)
			{
				_ParsedFilter = tempParsedFilter;
				PopulateCurrentView(false);
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

			ShowFolders = isFolderVisible;
			PopulateCurrentView(false);
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
			PopulateCurrentView(false);
		}

		/// <summary>
		/// Configure whether icons in listview should be shown or not.
		/// </summary>
		/// <param name="showIcons"></param>
		public void SetShowIcons(bool showIcons)
		{
			ShowIcons = showIcons;
			PopulateCurrentView(false);
		}

		/// <summary>
		/// Configure whether or not hidden files are shown in listview.
		/// </summary>
		/// <param name="showHiddenFiles"></param>
		public void SetShowHidden(bool showHiddenFiles)
		{
			ShowHidden = showHiddenFiles;
			PopulateCurrentView(false);
		}

		/// <summary>
		/// Generates a browse request based on the _CurrentFolder value
		/// (eg. Refresh current view), attempts to full view it, and returns
		/// the result indicator.
		/// </summary>
		protected FinalBrowseResult PopulateCurrentView(bool browseEvent,
														CancellationToken cancelToken = default(CancellationToken))
		{
			// This can happen when the viewmodel is configured at start-up
			// but the current folder is not configured, yet
			if (_CurrentFolder == null)
			{
				return new FinalBrowseResult(null, default(System.Guid), BrowseResult.InComplete);
			}

			var request = new BrowseRequest(_CurrentFolder, cancelToken);
			return PopulateView(request, browseEvent);
		}

		/// <summary>
		/// Fills the CurrentItems property for display in ItemsControl
		/// based view (ListBox, ListView etc.).
		/// 
		/// This method wraps a parameterized version of the same method 
		/// with a call that contains the standard data field.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="browseEvent">Defines whether a request should result
		/// in an explicit event relayed to a controller or not.</param>
		protected FinalBrowseResult PopulateView(BrowseRequest request,
												 bool browseEvent)
		{
			Logger.DebugFormat("PopulateView method");
			Logger.DebugFormat("Populating view for request id: {0} - '{1}'", request.RequestId, request.NewLocation.Path);

			// This can happen when the viewmodel is configured at start-up
			// but the current folder is not configured, yet
			if (request == null)
			{
				return FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);
			}

			IPathModel newPathToNavigateTo = request.NewLocation;

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

				DirectoryInfo cur = new DirectoryInfo(newPathToNavigateTo.Path);

				if (cur.Exists == false)
					return FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);

				CurrentItemClear();

				result = InternalPopulateView(_ParsedFilter, cur, ShowIcons);

				if (result == true)
				{
					SetCurrentLocation(newPathToNavigateTo);
					return FinalBrowseResult.FromRequest(request, BrowseResult.Complete);
				}

				return FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);
			}
			catch (Exception exp)
			{
				var bresult = FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);
				bresult.UnexpectedError = exp;
				return bresult;
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
		}

		/// <summary>
		/// Converts a filter string from "*.txt;*.tex" into a
		/// string array based format string[] filterString = { "*.txt", "*.tex" };
		/// </summary>
		/// <param name="inputFilterString"></param>
		protected string[] GetParsedFilters(string inputFilterString)
		{
			string[] filterString = { "*" };

			try
			{
				if (string.IsNullOrEmpty(inputFilterString) == false)
				{
					if (inputFilterString.Split(FilterSplitCharacter).Length > 1)
						filterString = inputFilterString.Split(FilterSplitCharacter);
					else
					{
						// Add asterix at front and beginning if user is too non-technical to type it.
						if (inputFilterString.Contains("*") == false)
							filterString = new string[] { "*" + inputFilterString + "*" };
						else
							filterString = new string[] { inputFilterString };
					}
				}
			}
			catch
			{
			}

			return filterString;
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

		private void SetCurrentLocation(IPathModel pmodel)
		{
			try
			{
				_CurrentFolder = pmodel.Clone() as IPathModel;
			}
			catch
			{
			}

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
			Logger.DebugFormat("InternalPopulateView method with filterString parameter");

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
