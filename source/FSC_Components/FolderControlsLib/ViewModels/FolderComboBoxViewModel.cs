namespace FolderControlsLib.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Windows.Input;
	using FolderControlsLib.Interfaces;
	using FolderControlsLib.ViewModels.Base;
	using FileSystemModels;
	using FileSystemModels.Interfaces;
	using System.Linq;
	using FileSystemModels.Browse;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Threading;
	using FileSystemModels.ViewModels.Collections.Generics;

	/// <summary>
	/// Class implements a viewmodel that can be used for a
	/// combobox that can be used to browse to different folder locations.
	/// </summary>
	internal class FolderComboBoxViewModel : Base.ViewModelBase, IFolderComboBoxViewModel
	{
		#region fields
		protected static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly SortableObservableDictionaryCollection<IFolderItemViewModel> _CurrentItems;

		private IFolderItemViewModel _SelectedItem = null;

		private ICommand _SelectionChanged = null;
		private string _SelectedRecentLocation = string.Empty;

		private readonly SemaphoreSlim _SlowStuffSemaphore;

		private bool _IsBrowsing;
		private bool _IsExternallyBrowsing;
		#endregion fields

		#region constructors
		/// <summary>
		/// Class constructor
		/// </summary>
		public FolderComboBoxViewModel()
		{
			_CurrentItems = new SortableObservableDictionaryCollection<IFolderItemViewModel>();
			_SlowStuffSemaphore = new SemaphoreSlim(1, 1);
		}
		#endregion constructors

		#region Events
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
		/// Expose a collection of file system items (folders and hard disks and ...) that
		/// can be selected and navigated to in this viewmodel.
		/// </summary>
		public IEnumerable<IFolderItemViewModel> CurrentItems
		{
			get
			{
				return _CurrentItems;
			}
		}

		/// <summary>
		/// Gets/sets the currently selected file system viewmodel item.
		/// </summary>
		public IFolderItemViewModel SelectedItem
		{
			get
			{
				return _SelectedItem;
			}

			protected set
			{
				if (_SelectedItem != value)
				{
					logger.DebugFormat("SelectedItem changed to '{0}' -> '{1}'", _SelectedItem, value);

					_SelectedItem = value;
					RaisePropertyChanged(() => SelectedItem);

					RaisePropertyChanged(() => CurrentFolder);
					RaisePropertyChanged(() => CurrentFolderToolTip);
				}
			}
		}

		/// <summary>
		/// Get/sets viewmodel data pointing at the path
		/// of the currently selected folder.
		/// </summary>
		public string CurrentFolder
		{
			get
			{
				if (_SelectedItem != null)
					return _SelectedItem.ItemPath;

				return string.Empty;
			}
		}

		/// <summary>
		/// Gets a string that can be displayed as a tooltip for the
		/// viewmodel data pointing at the path of the currently selected folder.
		/// </summary>
		public string CurrentFolderToolTip
		{
			get
			{
				if (string.IsNullOrEmpty(this.CurrentFolder) == false)
					return string.Format("{0}\n{1}", this.CurrentFolder,
													 FileSystemModels.Local.Strings.SelectLocationCommand_TT);
				else
					return FileSystemModels.Local.Strings.SelectLocationCommand_TT;
			}
		}

		#region commands
		/// <summary>
		/// Gets a command that should be invoked when the combobox view tells
		/// the viewmodel that the current path selection has changed
		/// (via selection changed event or keyup events).
		/// 
		/// The parameter p can be an array of objects
		/// containing objects of the FSItemVM type or
		/// p can also be string.
		/// 
		/// Each parameter item that adheres to the above types results in
		/// a OnCurrentPathChanged event being fired with the folder path
		/// as parameter.
		/// </summary>
		public ICommand SelectionChanged
		{
			get
			{
				if (_SelectionChanged == null)
					_SelectionChanged = new RelayCommand<object>(
						async (p) => await this.SelectionChanged_ExecutedAsync(p));

				return _SelectionChanged;
			}
		}
		#endregion commands
		#endregion properties

		#region methods
		/// <summary>
		/// Controller can start browser process if IsBrowsing = false
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		FinalBrowseResult INavigateable.NavigateTo(BrowseRequest request)
		{
			return PopulateView(request);
		}

		/// <summary>
		/// Controller can start browser process if IsBrowsing = false
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		async Task<FinalBrowseResult> INavigateable.NavigateToAsync(BrowseRequest request)
		{
			return await Task.Run(() => { return PopulateView(request); });
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
		/// Can be invoked to refresh the currently visible set of data.
		/// </summary>
		public FinalBrowseResult PopulateView(BrowseRequest request)
		{
			IPathModel newPath = request.NewLocation;

			// Make sure the task always processes the last input but is not started twice
			_SlowStuffSemaphore.WaitAsync();
			try
			{
				if (request.CancelTok != null)
					request.CancelTok.ThrowIfCancellationRequested();

				// Initialize view with current path
				if (_CurrentItems.Count() == 0)
				{
					CurrentItemsClear();
					SelectedItem = InitializeView(newPath);
				}
				else
				{
					var match = _CurrentItems.TryGet(newPath.Path);
					if (match != null)
					{
						SelectedItem = match;
					}
					else
					{
						var folderItem = new FolderItemViewModel(newPath, newPath.Name);
						SelectedItem = CurrentItemsAdd(folderItem);
					}
				}

				// Force a selection on to the control when there is no selected item, yet
				// Select last item in the list (hoping this is what we want...)
				if (_CurrentItems.Count > 0 && SelectedItem == null)
				{
					SelectedItem = _CurrentItems.Last();
				}

				return FinalBrowseResult.FromRequest(request, BrowseResult.Complete);
			}
			catch (Exception exp)
			{
				//// Console.WriteLine("{0} -> {1}", exp.Message, exp.StackTrace);
				var result = FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);
				result.UnexpectedError = exp;
				return result;
			}
			finally
			{
				_SlowStuffSemaphore.Release();
			}
		}

		private IFolderItemViewModel InitializeView(IPathModel newPath)
		{
			string pathroot = string.Empty;
			IFolderItemViewModel selectedItem = null;
			string[] dirs;

			if (newPath == null)
			{
				if (string.IsNullOrEmpty(CurrentFolder) == false)
				{
					return null; // No parameter available at this time ...
				}
				else
				{
					try
					{
						newPath = PathFactory.Create(CurrentFolder);
					}
					catch { }
				}
			}

			pathroot = newPath.PathRoot;
			dirs = PathFactory.GetDirectories(newPath.Path);

			// add drives
			foreach (string s in Directory.GetLogicalDrives())
			{
				IFolderItemViewModel info = FolderControlsLib.Factory.CreateLogicalDrive(s);
				CurrentItemsAdd(info);

				// add items under current folder if we currently create the root folder of the current path
				if (string.Compare(pathroot, s, true) == 0)
				{
					for (int i = 1; i < dirs.Length; i++)
					{
						try
						{
							string curdir = PathFactory.Join(dirs, 0, i + 1);

							var curPath = PathFactory.Create(curdir);
							var info2 = new FolderItemViewModel(curPath, dirs[i]);

							CurrentItemsAdd(info2);
						}
						catch // Non-existing/unknown items will throw an exception here...
						{
						}
					}

					selectedItem = _CurrentItems.Last();
				}
			}

			return selectedItem;
		}

		private async Task InternalPopulateViewAsync(BrowseRequest request, bool sendNotification)
		{
			IPathModel newPath = request.NewLocation;
			IsBrowsing = true;
			try
			{
				if (sendNotification == true && this.SelectedItem != null)
				{
					if (BrowseEvent != null)
					{
						BrowseEvent(this, new BrowsingEventArgs(newPath, true, BrowseResult.Unknown));
					}
				}

				await Task.Run(() =>
				{
					FinalBrowseResult result = null;
					result = PopulateView(request);

					if (sendNotification == true && this.SelectedItem != null)
					{
						if (BrowseEvent != null)
						{
							BrowseEvent(this, new BrowsingEventArgs(
								newPath, false, (result == null ? BrowseResult.InComplete :
																  result.Result)));
						}
					}
				});
			}
			finally
			{
				IsBrowsing = false;
			}
		}

		/// <summary>
		/// Method executes when the SelectionChanged command is invoked.
		/// The parameter <paramref name="p"/> can be an array of objects
		/// containing objects of the <seealso cref="IFolderItemViewModel"/> type
		/// or p can also be string.
		/// 
		/// Each parameter item that adheres to the above types results in
		/// a OnCurrentPathChanged event being fired with the folder path
		/// as parameter.
		/// 
		/// This mwthod can typically be invoked by:
		/// 1> Edit the text portion + Enter in the control or
		/// 2> By selecting an entry from the drop down list of the combobox.
		/// </summary>
		/// <param name="p"></param>
		private async Task SelectionChanged_ExecutedAsync(object p)
		{
			if (p == null)
				return;

			// Check if the given parameter is a string, fire a corresponding event if so...
			if (p is string)
			{
				IPathModel param = null;
				try
				{
					param = PathFactory.Create(p as string);
				}
				catch
				{
					return;   // Control will refuse to select an unknown/non-existing item
				}

				// This breaks a possible recursion, if a new view is requested even though its
				// already available, because this could, otherwise, change the SelectedItem
				// which in turn could request another PopulateView(...) -> SelectedItem etc ...
				if (SelectedItem != null)
				{
					if (SelectedItem.Equals(param))
						return;
				}

				if (param != null)
				{
					var request = new BrowseRequest(param);
					await InternalPopulateViewAsync(request, true);
				}
			}
			else
			{
				if (p is object[])
				{
					var param = p as object[];

					if (param != null)
					{
						if (param.Length > 0)
						{
							var newPath = param[param.Length - 1] as IFolderItemViewModel;

							if (newPath != null)
							{
								IPathModel location = null;
								IPathModel selectedItemModel = null;
								try
								{
									location = PathFactory.Create(newPath.ItemPath, newPath.ItemType);
									selectedItemModel = PathFactory.Create(SelectedItem.ItemPath, SelectedItem.ItemType);
								}
								catch
								{
								}

								if (location == null || selectedItemModel == null)
									return;

								// This breaks a possible recursion, if a new view is requested even though its
								// already available, because this could, otherwise, change the SelectedItem
								// which in turn could request another PopulateView(...) -> SelectedItem etc ...
								if (location.Equals(selectedItemModel))
									return;

								var request = new BrowseRequest(location);
								await InternalPopulateViewAsync(request, true);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Clears the collection of current file/folder items and makes sure
		/// the operation is performed on the dispatcher thread.
		/// </summary>
		private void CurrentItemsClear()
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
		private IFolderItemViewModel CurrentItemsAdd(IFolderItemViewModel item)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				_CurrentItems.AddItem(item.ItemPath, item);
				_CurrentItems.Sort(it => it.ItemPath);
			});

			return item;
		}
		#endregion methods
	}
}
