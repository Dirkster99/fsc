namespace FileSystemModels.ViewModels
{
	using FileSystemModels;
	using FileSystemModels.Interfaces;
	using FileSystemModels.Models.FSItems.Base;
	using FileSystemModels.Utils;
	using FileSystemModels.ViewModels.Base;

	/// <summary>
	/// Implements a viewmodel for file system items that are listed in a
	/// list like control (pop-up list, list box, combobox etc..)
	/// </summary>
	internal class ListItemViewModel : ViewModelBase, IListItemViewModel
	{
		#region fields
		/// <summary>
		/// Logger facility
		/// </summary>
		protected static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string _ItemName;
		private IPathModel _PathObject;
		private string _VolumeLabel;
		#endregion fields

		#region constructor
		/// <summary>
		/// class constructor
		/// </summary>
		/// <param name="curdir"></param>
		/// <param name="itemName"></param>
		/// <param name="itemType"></param>
		/// <param name="showIcon"></param>
		public ListItemViewModel(string curdir,
						FSItemType itemType,
						string itemName,
						bool showIcon)
			: this(curdir, itemType, itemName)
		{
			this.ShowIcon = showIcon;
		}

		/// <summary>
		/// class constructor
		/// </summary>
		/// <param name="curdir"></param>
		/// <param name="itemName"></param>
		/// <param name="itemType"></param>
		public ListItemViewModel(string curdir,
						FSItemType itemType,
						string itemName)
			: this()
		{
			this._PathObject = PathFactory.Create(curdir, itemType);
			this.ItemName = itemName;
		}

		/// <summary>
		/// Copy constructor
		/// </summary>
		/// <param name="copyThis"></param>
		public ListItemViewModel(ListItemViewModel copyThis)
			: this()
		{
			if (copyThis == null)
				return;

			_ItemName = copyThis._ItemName;

			_PathObject = copyThis._PathObject.Clone() as IPathModel;
			_VolumeLabel = copyThis._VolumeLabel;

			ShowIcon = copyThis.ShowIcon;
		}

		/// <summary>
		/// Hidden standard class constructor
		/// </summary>
		protected ListItemViewModel()
		{
			_PathObject = null;
			_VolumeLabel = null;
			ShowIcon = true;
		}
		#endregion constructor

		#region properties
		/// <summary>
		/// Gets the type (folder, file) of this item
		/// </summary>
		public FSItemType ItemType
		{
			get
			{
				return (this._PathObject != null ? this._PathObject.PathType : FSItemType.Unknown);
			}
		}

		/// <summary>
		/// Gets the path to this item
		/// </summary>
		public string ItemPath
		{
			get
			{
				return (this._PathObject != null ? this._PathObject.Path : null);
			}
		}

		/// <summary>
		/// Gets whether this folder is currently expanded or not.
		/// 
		/// This viewmodel, currently, has no use case for an expanded item.
		/// Therefore, this property returns a constanst false value.
		/// </summary>
		public bool IsExpanded { get { return false; } }

		/// <summary>
		/// Gets a name that can be used for display
		/// (is not necessarily the same as path)
		/// </summary>
		public string ItemName
		{
			get
			{
				return this._ItemName;
			}

			private set
			{
				if (this._ItemName != value)
				{
					this._ItemName = value;
					this.RaisePropertyChanged(() => this.ItemName);
				}
			}
		}

		/// <summary>
		/// Gets a folder item string for display purposes.
		/// This string can evaluete to 'C:\ (Windows)' for drives,
		/// if the 'C:\' drive was named 'Windows'.
		/// </summary>
		public string ItemDisplayString
		{
			get
			{
				return IItemExtension.GetDisplayString(this as IItem);
			}
		}

		/// <summary>
		/// Gets whether or not to show a tooltip for this item.
		/// </summary>
		public bool ShowIcon { get; private set; }
		#endregion properties

		#region methods
		/// <summary>
		/// Standard method to display contents of this class.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.ItemPath;
		}

		/// <summary>
		/// Determine whether a given path is an exeisting directory or not.
		/// </summary>
		/// <returns>true if this directory exists and otherwise false</returns>
		public bool DirectoryPathExists()
		{
			return this._PathObject.DirectoryPathExists();
		}
		#endregion methods
	}
}
