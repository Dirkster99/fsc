namespace FileSystemModels.Models
{
    using System.Collections.Generic;
    using System.IO;
    using FileSystemModels.Interfaces;
    using FileSystemModels.Models.FSItems.Base;

    /// <summary>
    /// Class implements a model that keeps track of a browsing history similar to the well known
    /// Internet browser functionality that lets users go forward and backward between visited URLs.
    /// </summary>
    public class BrowseNavigation : IBrowseNavigation
    {
        #region fields
        /// <summary>
        /// Log4Net instance for logging errors, warnings etc
        /// </summary>
        protected static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Defines the delimitor for multiple regular expression filter statements.
        /// eg: "*.txt;*.ini"
        /// </summary>
        private const char FilterSplitCharacter = ';';

        /// <summary>
        /// Determines whether the redo stack (FutureFolders) should be cleared when the
        /// CurrentFolder changes next time
        /// </summary>
        private string mFilterString = string.Empty;
        #endregion fields

        #region constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        public BrowseNavigation()
        {
        }
        #endregion constructor

        #region properties
        /// <summary>
        /// Gets/sets the current folder which is being
        /// queried to list the current files and folders for display.
        /// </summary>
        public IPathModel CurrentFolder { get; private set; }
        #endregion properties

        #region methods
        /// <summary>
        /// Converts a filter string from "*.txt;*.tex" into a
        /// string array based format string[] filterString = { "*.txt", "*.tex" };
        /// </summary>
        /// <param name="inputFilterString"></param>
        public static string[] GetParsedFilters(string inputFilterString)
        {
            string[] filterString = { "*" };

            try
            {
                if (string.IsNullOrEmpty(inputFilterString) == false)
                {
                    if (inputFilterString.Split(BrowseNavigation.FilterSplitCharacter).Length > 1)
                        filterString = inputFilterString.Split(BrowseNavigation.FilterSplitCharacter);
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

        /// <summary>
        /// Method is executed when a listview item is double clicked.
        /// </summary>
        /// <param name="infoType"></param>
        /// <param name="newPath"></param>
        FSItemType IBrowseNavigation.BrowseDown(FSItemType infoType, string newPath)
        {
            if (infoType == FSItemType.Folder || infoType == FSItemType.LogicalDrive)
                SetCurrentFolder(newPath, true);

            return infoType;
        }

        /// <summary>
        /// Determines whether the current path represents a directory or not.
        /// </summary>
        /// <returns>true if directory otherwise false</returns>
        bool IBrowseNavigation.IsCurrentPathDirectory()
        {
            if (CurrentFolder != null)
                return (CurrentFolder.DirectoryPathExists());

            return false;
        }

        /// <summary>
        /// Get the file system object that represents the current directory.
        /// </summary>
        /// <returns></returns>
        DirectoryInfo IBrowseNavigation.GetDirectoryInfoOnCurrentFolder()
        {
            try
            {
                return new DirectoryInfo(this.CurrentFolder.Path);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets an array of the current filter items eg: "*.txt", "*.pdf", "*.doc"
        /// </summary>
        /// <returns></returns>
        string[] IBrowseNavigation.GetFilterArray()
        {
            string[] filterString = { "*.*" };

            if (string.IsNullOrEmpty(this.mFilterString) == false)
            {
                if (this.mFilterString.Split(BrowseNavigation.FilterSplitCharacter).Length > 1)
                    filterString = this.mFilterString.Split(BrowseNavigation.FilterSplitCharacter);
                else
                    filterString = new string[] { this.mFilterString };
            }

            return filterString;
        }

        /// <summary>
        /// Resets the current filter string in raw format.
        /// </summary>
        /// <param name="filterText"></param>
        void IBrowseNavigation.SetFilterString(string filterText)
        {
            this.mFilterString = filterText;
        }

        /// <summary>
        /// Sets the current folder to a new folder (with or without adjustments of History).
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bSetHistory"></param>
        public void SetCurrentFolder(string path, bool bSetHistory)
        {
            try
            {
                this.CurrentFolder = PathFactory.Create(path);
            }
            catch
            {
            }
        }
        #endregion methods
    }
}
