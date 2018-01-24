namespace FolderBrowser.Interfaces
{
    using FolderBrowser.ViewModels;
    using InplaceEditBoxLib.Events;
    using UserNotification.ViewModel;

    public interface IFolderViewModel
    {
        #region properties
        /// <summary>
        /// Determine whether child is a dummy (must be evaluated and replaced
        /// with real data) or not.
        /// </summary>
        bool ChildFolderIsDummy { get; }

        /// <summary>
        /// Gets a string that is to be displayed for this item.
        /// This is primarily used for drives which can show more
        /// information than just their path portion ... 'C:'
        /// </summary>
        string DisplayItemString { get; }

        /// <summary>
        /// Gets the name (without the path) of this item.
        /// </summary>
        string FolderName { get; }

        /// <summary>
        /// Gets the complete path and filder name that
        /// represents the address of this folder.
        /// </summary>
        string FolderPath { get; }

        /// <summary>
        /// Gets a collection of sub-folders (if any) that are contained within this folder.
        /// </summary>
        FsCore.Collections.ObservableSortedDictionary<string, IFolderViewModel> Folders { get; }

        /// <summary>
        /// Gets whether this folder is currently expanded or not.
        /// </summary>
        bool IsExpanded { get; }

        /// <summary>
        /// Get/set whether this folder is currently selected or not.
        /// </summary>
        bool IsSelected { get; }

        /// <summary>
        /// Gets the type of the underlying model that is represented by this object.
        /// The underlying can be LogicalDrive, Folder, or some other item.
        /// </summary>
        FileSystemModels.Models.FSItems.FSItemType ItemType { get; }
        #endregion properties

        #region methods
        /// <summary>
        /// Adds the folder item into the collection of sub-folders of this folder.
        /// </summary>
        /// <param name="item"></param>
        void AddFolder(IFolderViewModel item);

        /// <summary>
        /// Create a new folder with a standard name
        /// 'New folder n' underneath this folder.
        /// </summary>
        /// <returns>a viewmodel of the newly created directory or null</returns>
        IFolderViewModel CreateNewDirectory();

        /// <summary>
        /// Rename the name of the folder into a new name.
        /// </summary>
        /// <param name="newFolderName"></param>
        void RenameFolder(string newFolderName);

        /// <summary>
        /// Remove all sub-folders from a given folder.
        /// </summary>
        void ClearFolders();
        #endregion methods
    }
}
