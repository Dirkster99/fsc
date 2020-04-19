namespace FileSystemModels.Utils
{
	using FileSystemModels.Interfaces;
	using FileSystemModels.Models.FSItems.Base;
	using System;

	/// <summary>
	/// Implements simple method based extensions that can be used for classes
	/// implementing the <see cref="IItem"/> interface.
	/// </summary>
	public class IItemExtension
	{
		/// <summary>
		/// Gets a display string for a file system item (file, folder, drive).
		/// 
		/// The string for display is not necessarily the same as the actual name
		/// of the item - drives for example are named like 'F:\' but an intended
		/// display string may be 'F:\ (drive is not ready)'.
		/// 
		/// The label is displayed in brackets if supplied to this call of if it
		/// can be determined, for examples, for drives.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static string GetDisplayString(IItem item, string label = null)
		{
			switch (item.ItemType)
			{
				case FSItemType.LogicalDrive:
					try
					{
						if (label == null)
						{
							var di = new System.IO.DriveInfo(item.ItemName);

							if (di.IsReady == true)
								label = di.VolumeLabel;
							else
								return string.Format("{0} ({1})", item.ItemName, "device is not ready");
						}

						return string.Format("{0} {1}", item.ItemName, (string.IsNullOrEmpty(label)
																		? string.Empty
																		: string.Format("({0})", label)));
					}
					catch (Exception exp)
					{
						// Just return a folder name if everything else fails (drive may not be ready etc).
						return string.Format("{0} ({1})", item.ItemName, exp.Message.Trim());
					}

				case FSItemType.Folder:
				case FSItemType.Unknown:
				default:
					return item.ItemName;
			}
		}
	}
}
