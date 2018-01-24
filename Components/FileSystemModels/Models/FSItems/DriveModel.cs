namespace FileSystemModels.Models.FSItems
{
    using Base;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;

    public class DriveModel : Base.FileSystemModel
    {
        #region fields
        DriveInfo mDrive;
        #endregion fields

        #region constructors
        /// <summary>
        /// Parameterized class  constructor
        /// </summary>
        /// <param name="model"></param>
        [SecuritySafeCritical]
        public DriveModel(PathModel model)
          : base(model)
        {
            mDrive = new DriveInfo(model.Path);
        }
        #endregion constructors

        #region properties
        public long AvailableFreeSpace
        {
            get
            {
                return mDrive.AvailableFreeSpace;
            }
        }

        public string DriveFormat
        {
            get
            {
                return mDrive.DriveFormat;
            }
        }

        ////    public DriveType DriveType
        ////    {
        ////      get
        ////      {
        ////        return this.mDrive.DriveType;
        ////      }
        ////    }

        public bool Exists
        {
            get
            {
                return mDrive.RootDirectory.Exists;
            }
        }

        public bool IsReady
        {
            get
            {
                return mDrive.IsReady;
            }
        }

        public long TotalFreeSpace
        {
            get
            {
                return mDrive.TotalFreeSpace;
            }
        }

        public long TotalSize
        {
            get
            {
                return mDrive.TotalSize;
            }
        }

        public string VolumeLabel
        {
            get
            {
                return mDrive.VolumeLabel;
            }
        }
        #endregion properties

        #region methods
        public static IEnumerable<FileSystemModel> GetLogicalDrives()
        {
            foreach (var item in Environment.GetLogicalDrives())
            {
                string driveLetter = item.TrimEnd('\\');

                if (string.IsNullOrEmpty(driveLetter) == false)
                    yield return new DriveModel(new PathModel(driveLetter, FSItemType.LogicalDrive));
            }
        }
        #endregion methods
    }
}
