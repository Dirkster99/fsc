namespace FolderBrowser.ViewModels
{
  using System;
  using System.Collections.ObjectModel;
  using System.Diagnostics;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Windows.Input;
  using FileSystemModels.Models;
  using FolderBrowser.Command;
  using FolderBrowser.ViewModels.Interfaces;
  using MsgBox;

  /// <summary>
  /// Implment the viewmodel for one folder entry for a collection of folders.
  /// </summary>
  public class FolderViewModel : Base.ViewModelBase, IFolderViewModel
  {
    #region fields
    protected static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private bool mIsSelected;
    private bool mIsExpanded;
    private FSItemType mItemType;
    private string mFolderName;
    private string mFolderPath;

    private ObservableCollection<IFolderViewModel> mFolders;

    private RelayCommand<object> mOpenInWindowsCommand;
    private RelayCommand<object> mCopyPathCommand;

    private string mVolumeLabel;
    #endregion fields

    #region constructor
    /// <summary>
    /// Parameterized <seealso cref="FolderViewModel"/> constructor
    /// </summary>
    public FolderViewModel(FSItemType itemType)
    : this()
    {
      this.mItemType = itemType;
    }

    /// <summary>
    /// Standard <seealso cref="FolderViewModel"/> constructor
    /// </summary>
    public FolderViewModel()
    {
      this.mIsExpanded = this.mIsSelected = false;
      this.mItemType = FSItemType.Unknown;
      this.mFolderName = this.mFolderPath = string.Empty;

      this.mFolders = null;

      this.mOpenInWindowsCommand = null;
      this.mCopyPathCommand = null;

      this.mVolumeLabel = null;
    }
    #endregion constructor

    #region properties
    /// <summary>
    /// Gets the name of this folder (without its root path component).
    /// </summary>
    public string FolderName
    {
      get
      {
        return this.mFolderName;
      }

      private set
      {
        if (this.mFolderName != value)
        {
          this.mFolderName = value;
          this.RaisePropertyChanged(() => this.FolderName);
          this.RaisePropertyChanged(() => this.DisplayItemString);
        }
      }
    }

    /// <summary>
    /// Get/set file system Path for this folder.
    /// </summary>
    public string FolderPath
    {
      get
      {
        return this.mFolderPath;
      }

      private set
      {
        if (this.mFolderPath != value)
        {
          this.mFolderPath = value;
          this.RaisePropertyChanged(() => this.FolderPath);
          this.RaisePropertyChanged(() => this.DisplayItemString);
        }
      }
    }

    /// <summary>
    /// Gets a folder item string for display purposes.
    /// This string can evaluete to 'C:\ (Windows)' for drives,
    /// if the 'C:\' drive was named 'Windows'.
    /// </summary>
    public string DisplayItemString
    {
      get
      {
        switch (this.ItemType)
        {
          case FSItemType.LogicalDrive:
            try
            {
              if (this.mVolumeLabel == null)
              {
                DriveInfo di = new System.IO.DriveInfo(this.FolderName);

                if (di.IsReady == true)
                  this.mVolumeLabel = di.VolumeLabel;
                else
                  return string.Format("{0} ({1})", this.FolderName, Local.Strings.STR_MSG_DEVICE_NOT_READY);
              }

              return string.Format("{0} {1}", this.FolderName, (string.IsNullOrEmpty(this.mVolumeLabel)
                                                                  ? string.Empty
                                                                  : string.Format("({0})", this.mVolumeLabel)));
            }
            catch (Exception exp)
            {
              Logger.Error("DriveInfo cannot be optained for:" + this.FolderName, exp);

              // Just return a folder name if everything else fails (drive may not be ready etc).
              return string.Format("{0} ({1})", this.FolderName, exp.Message.Trim());
            }

          case FSItemType.Folder:
          case FSItemType.Unknown:
          default:
            return this.FolderName;
        }
      }
    }

    /// <summary>
    /// Get/set observable collection of sub-folders of this folder.
    /// </summary>
    public ObservableCollection<IFolderViewModel> Folders
    {
      get
      {
        if (this.mFolders == null)
          this.mFolders = new ObservableCollection<IFolderViewModel>();

        return this.mFolders;
      }
    }

    /// <summary>
    /// Get/set whether this folder is currently selected or not.
    /// </summary>
    public bool IsSelected
    {
      get
      {
        return this.mIsSelected;
      }

      set
      {
        if (this.mIsSelected != value)
        {
          this.mIsSelected = value;

          this.RaisePropertyChanged(() => this.IsSelected);

          if (value == true)
            this.IsExpanded = true;                 // Default windows behaviour of expanding the selected folder
        }
      }
    }

    /// <summary>
    /// Get/set whether this folder is currently expanded or not.
    /// </summary>
    public bool IsExpanded
    {
      get
      {
        return this.mIsExpanded;
      }

      set
      {
        if (this.mIsExpanded != value)
        {
          this.mIsExpanded = value;

          this.RaisePropertyChanged(() => this.IsExpanded);

          // Load all sub-folders into the Folders collection.
          this.LoadFolders();
        }
      }
    }

    /// <summary>
    /// Gets the type of this item (eg: Folder, HardDisk etc...).
    /// </summary>
    public FSItemType ItemType
    {
      get
      {
        return this.mItemType;
      }

      private set
      {
        if (this.mItemType != value)
        {
          this.mItemType = value;

          this.RaisePropertyChanged(() => this.ItemType);
        }
      }
    }

    /// <summary>
    /// Gets a command that will open the selected item with the current default application
    /// in Windows. The selected item (path to a file) is expected as <seealso cref="FSItemVM"/> parameter.
    /// (eg: Item is HTML file -> Open in Windows starts the web browser for viewing the HTML
    /// file if thats the currently associated Windows default application.
    /// </summary>
    public ICommand OpenInWindowsCommand
    {
      get
      {
        if (this.mOpenInWindowsCommand == null)
          this.mOpenInWindowsCommand = new RelayCommand<object>(
            (p) =>
            {
              var path = p as FolderViewModel;

              if (path == null)
                return;

              if (string.IsNullOrEmpty(path.FolderPath) == true)
                return;

              FolderViewModel.OpenInWindowsCommand_Executed(path.FolderPath);
            });

        return this.mOpenInWindowsCommand;
      }
    }

    /// <summary>
    /// Gets a command that will copy the path of an item into the Windows Clipboard.
    /// The item (path to a file) is expected as <seealso cref="FSItemVM"/> parameter.
    /// </summary>
    public ICommand CopyPathCommand
    {
      get
      {
        if (this.mCopyPathCommand == null)
          this.mCopyPathCommand = new RelayCommand<object>(
            (p) =>
            {
              var path = p as FolderViewModel;

              if (path == null)
                return;

              if (string.IsNullOrEmpty(path.FolderPath) == true)
                return;

              FolderViewModel.CopyPathCommand_Executed(path.FolderPath);
            });

        return this.mCopyPathCommand;
      }
    }
    #endregion properties

    #region methods
    /// <summary>
    /// Construct a <seealso cref="FolderViewModel"/> item that represents a Windows
    /// file system drive object (eg.: 'C:\').
    /// </summary>
    /// <param name="driveLetter"></param>
    /// <returns></returns>
    public static FolderViewModel ConstructDriveFolderViewModel(string driveLetter)
    {
      try
      {
        FolderViewModel f = new FolderViewModel(FSItemType.LogicalDrive)
        {
          FolderPath = driveLetter.TrimEnd('\\'),  // Assign drive letter 'C:\' to both elements
          FolderName = driveLetter.TrimEnd('\\')
        };

        return f;
      }
      catch
      {
      }

      return null;
    }

    /// <summary>
    /// Construct a <seealso cref="FolderViewModel"/> item that represents a Windows
    /// file system folder object (eg.: FolderPath = 'C:\Temp\', FolderName = 'Temp').
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public static FolderViewModel ConstructFolderFolderViewModel(string dir)
    {
      try
      {
        string folderName = Path.GetFileName(dir);
        string folderPath = Path.GetFullPath(dir);

        FolderViewModel f = new FolderViewModel(FSItemType.Folder)
        {
          FolderName = folderName,
          FolderPath = folderPath
        };

        return f;
      }
      catch
      {
      }

      return null;
    }

    #region FileSystem Commands
    /// <summary>
    /// Convinience method to open Windows Explorer with a selected file (if it exists).
    /// Otherwise, Windows Explorer is opened in the location where the file should be at.
    /// </summary>
    /// <param name="sFileName"></param>
    /// <returns></returns>
    private static bool OpenContainingFolderCommand_Executed(string sFileName)
    {
      if (string.IsNullOrEmpty(sFileName) == true)
        return false;

      try
      {
        if (System.IO.File.Exists(sFileName) == true)
        {
          // combine the arguments together it doesn't matter if there is a space after ','
          string argument = @"/select, " + sFileName;

          System.Diagnostics.Process.Start("explorer.exe", argument);
          return true;
        }
        else
        {
          string sParentDir = string.Empty;

          if (System.IO.Directory.Exists(sFileName) == true)
            sParentDir = sFileName;
          else
            sParentDir = System.IO.Directory.GetParent(sFileName).FullName;

          if (System.IO.Directory.Exists(sParentDir) == false)
          {
            Msg.Show(string.Format(Local.Strings.STR_MSG_DIRECTORY_DOES_NOT_EXIST, sParentDir),
                                   Local.Strings.STR_MSG_ERROR_FINDING_RESOURCE,
                                   MsgBoxButtons.OK, MsgBoxImage.Error);

            return false;
          }
          else
          {
            // combine the arguments together it doesn't matter if there is a space after ','
            string argument = @"/select, " + sParentDir;

            System.Diagnostics.Process.Start("explorer.exe", argument);

            return true;
          }
        }
      }
      catch (System.Exception ex)
      {
        Msg.Show(string.Format("{0}\n'{1}'.", ex.Message, (sFileName == null ? string.Empty : sFileName)),
                  Local.Strings.STR_MSG_ERROR_FINDING_RESOURCE,
                  MsgBoxButtons.OK, MsgBoxImage.Error);

        return false;
      }
    }

    /// <summary>
    /// Process command when a hyperlink has been clicked.
    /// Start a web browser and let it browse to where this points to...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OpenInWindowsCommand_Executed(string sFileName)
    {
      if (string.IsNullOrEmpty(sFileName) == true)
        return;

      try
      {
        Process.Start(new ProcessStartInfo(sFileName));
        ////OpenFileLocationInWindowsExplorer(whLink.NavigateUri.OriginalString);
      }
      catch (System.Exception ex)
      {
        Msg.Show(string.Format(CultureInfo.CurrentCulture, "{0}", ex.Message),
                 Local.Strings.STR_MSG_ERROR_FINDING_RESOURCE,
                 MsgBoxButtons.OK, MsgBoxImage.Error);
      }
    }

    /// <summary>
    /// A hyperlink has been clicked. Start a web browser and let it browse to where this points to...
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// Load all sub-folders into the Folders collection.
    /// </summary>
    private void LoadFolders()
    {
      try
      {
        if (this.Folders.Count > 0)
          return;

        string[] dirs = null;

        string fullPath = Path.Combine(this.FolderPath, this.FolderName);

        if (this.FolderName.Contains(':'))                  // This is a drive
          fullPath = string.Concat(this.FolderName, "\\");
        else
          fullPath = this.FolderPath;

        try
        {
          dirs = Directory.GetDirectories(fullPath);
        }
        catch (Exception)
        {
        }

        this.Folders.Clear();

        if (dirs != null)
        {
          foreach (string dir in dirs)
          {
            try
            {
              DirectoryInfo di = new DirectoryInfo(dir);

              // create the sub-structure only if this is not a hidden directory
              if ((di.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                this.Folders.Add(ConstructFolderFolderViewModel(dir));
            }
            catch (UnauthorizedAccessException ae)
            {
              Logger.Warn("Directory Access not authorized", ae);
            }
            catch (Exception e)
            {
              Logger.Warn(e);
            }
          }
        }
      }
      catch (UnauthorizedAccessException ae)
      {
        Console.WriteLine(ae.Message);
      }
      catch (IOException ie)
      {
        Console.WriteLine(ie.Message);
      }
    }
    #endregion methods
  }
}
