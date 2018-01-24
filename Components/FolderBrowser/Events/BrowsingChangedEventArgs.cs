namespace FolderBrowser.Events
{
    using System;
    using FileSystemModels.Models;

    /// <summary>
    /// A simple event based state model that informs the subscriber about the
    /// state of the browser when changing between locations.
    /// </summary>
    public class BrowsingChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Event type class constructor from parameter
        /// </summary>
        public BrowsingChangedEventArgs(PathModel path, bool isBrowsing)
        : this()
        {
            this.Folder = path;
            this.IsBrowsing = IsBrowsing;
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public BrowsingChangedEventArgs()
        : base()
        {
            this.Folder = null;
            this.IsBrowsing = false;
        }

        /// <summary>
        /// Path we are browsing to or being arrived at.
        /// </summary>
        public PathModel Folder { get; private set; }

        /// <summary>
        /// Determines if we are:
        /// 1) Currently browsing towards this path (display progress) or
        /// 2) if the browsing process has arrived at this location (finish progress display).
        /// </summary>
        public bool IsBrowsing { get; private set; }
    }
}
