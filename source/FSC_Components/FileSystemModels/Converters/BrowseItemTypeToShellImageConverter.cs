namespace FileSystemModels.Converters
{
	using FileSystemModels.Interfaces;
	using FileSystemModels.Models.FSItems.Base;
	using FileSystemModels.Utils;
	using System;
	using System.Globalization;
	using System.Windows.Data;

	/// <summary>
	/// Converts an <seealso cref="IItem"/>
	/// into <seealso cref="System.Windows.Media.ImageSource"/>
	/// using available Windows Shell Icons.
	/// </summary>
	[ValueConversion(typeof(IItem), typeof(System.Windows.Media.ImageSource))]
	public class BrowseItemTypeToShellImageConverter : IValueConverter
	{
		#region fields
		/// <summary>
		/// Log4net logger facility.
		/// </summary>
		protected static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		#endregion fields

		#region constructor
		/// <summary>
		/// Class constructor
		/// </summary>
		public BrowseItemTypeToShellImageConverter()
		{
		}
		#endregion constructor

		#region methods
		/// <summary>
		/// Convert a <see cref="IItem"/> into an image representation.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object Convert(object value, Type targetType,
							  object parameter, CultureInfo culture)
		{
			var item = value as IItem;

			if (item == null)
				return Binding.DoNothing;

			System.Windows.Media.ImageSource displayIcon = null;

			try
			{
				// a folder can be represented with a seperate icon for its expanded state
				if (item.ItemType == FSItemType.Folder)
					displayIcon = IconExtractor.GetFolderIcon(item.ItemPath,
															  item.IsExpanded).ToImageSource();
				else
					displayIcon = IconExtractor.GetFileIcon(item.ItemPath).ToImageSource();
			}
			catch
			{
			}

			return displayIcon;
		}

		/// <summary>
		/// Not Implemented - throws <see cref="NotImplementedException"/>.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns></returns>
		public object ConvertBack(object value, Type targetType,
								  object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion methods
	}
}
