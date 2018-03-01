namespace FileSystemModels.Interfaces
{
    using System.IO;
    using FileSystemModels.Models.FSItems.Base;

    /// <summary>
    /// Interface implements a model that keeps track of a browsing history similar to the well known
    /// Internet browser functionality that lets user go forward and backward between visited URIs.
    /// </summary>
    public interface IBrowseNavigation
    {
        #region properties
        /// <summary>
        /// Gets/sets the current folder which is being
        /// queried to list the current files and folders for display.
        /// </summary>
        IPathModel CurrentFolder { get; }
        #endregion properties

        #region methods
        /// <summary>
        /// Determines whether the current path represents a directory or not.
        /// </summary>
        /// <returns></returns>
        bool IsCurrentPathDirectory();

        /// <summary>
        /// Gets an array of the current filter items eg: "*.txt", "*.pdf", "*.doc"
        /// </summary>
        /// <returns></returns>
        string[] GetFilterArray();

        /// <summary>
        /// Get the file system object that represents the current directory.
        /// </summary>
        /// <returns></returns>
        DirectoryInfo GetDirectoryInfoOnCurrentFolder();

        /// <summary>
        /// Resets the current filter string in raw format.
        /// </summary>
        /// <param name="filterText"></param>
        void SetFilterString(string filterText);

        /// <summary>
        /// Sets the current folder to a new folder (with or without adjustments of History).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bSetHistory"></param>
        void SetCurrentFolder(string path, bool bSetHistory);

        /// <summary>
        /// Browse into a given path.
        /// </summary>
        /// <param name="infoType"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        FSItemType BrowseDown(FSItemType infoType, string newPath);
        #endregion methods
    }
}
