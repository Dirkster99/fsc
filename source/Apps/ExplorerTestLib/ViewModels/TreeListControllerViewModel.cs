namespace ExplorerTestLib.ViewModels
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using ExplorerTestLib.Interfaces;
    using ExplorerTestLib.Tasks;
    using FileListView.Interfaces;
    using FileSystemModels.Browse;
    using FileSystemModels.Events;
    using FileSystemModels.Interfaces;
    using FileSystemModels.Interfaces.Bookmark;
    using FilterControlsLib.Interfaces;
    using FolderBrowser.Interfaces;

    /// <summary>
    /// Class implements a tree/folder/file view model class
    /// that can be used to dispaly filesystem related content in a view or dialog.
    /// 
    /// Common Sample dialogs are file pickers for load/save etc.
    /// </summary>
    internal class TreeListControllerViewModel : ControllerBaseViewModel, ITreeListControllerViewModel
    {
        #region fields
        protected static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly SemaphoreSlim _SlowStuffSemaphore;
        private readonly OneTaskLimitedScheduler _OneTaskScheduler;
        private readonly CancellationTokenSource _CancelToken;
        #endregion fields

        #region constructor
        /// <summary>
        /// Custom class constructor
        /// </summary>
        /// <param name="onFileOpenMethod"></param>
        public TreeListControllerViewModel(System.EventHandler<FileOpenEventArgs> onFileOpenMethod)
          : this()
        {
            // Remove the standard constructor event that is fired when a user opens a file
            WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                .RemoveHandler(FolderItemsView, "OnFileOpen", FolderItemsView_OnFileOpen);

            // ...and establish a new link (if any)
            if (onFileOpenMethod != null)
            {
                WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                    .AddHandler(FolderItemsView, "OnFileOpen", onFileOpenMethod);
            }
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public TreeListControllerViewModel()
        {
            _SlowStuffSemaphore = new SemaphoreSlim(1, 1);
            _OneTaskScheduler = new OneTaskLimitedScheduler();
            _CancelToken = new CancellationTokenSource();

            FolderItemsView = FileListView.Factory.CreateFileListViewModel();
            FolderTextPath = FolderControlsLib.Factory.CreateFolderComboBoxVM();
            RecentFolders = FileSystemModels.Factory.CreateBookmarksViewModel();
            TreeBrowser = FolderBrowser.FolderBrowserFactory.CreateBrowserViewModel(false);

            // This is fired when the user selects a new folder bookmark from the drop down button
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(RecentFolders, "BrowseEvent", FolderTextPath_BrowseEvent);

            // This is fired when the text path in the combobox changes to another existing folder
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(FolderTextPath, "BrowseEvent", FolderTextPath_BrowseEvent);

            Filters = FilterControlsLib.Factory.CreateFilterComboBoxViewModel();
            WeakEventManager<IFilterComboBoxViewModel, FilterChangedEventArgs>
                .AddHandler(Filters, "OnFilterChanged", FileViewFilter_Changed);

            // This is fired when the current folder in the listview changes to another existing folder
            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(FolderItemsView, "BrowseEvent", FolderTextPath_BrowseEvent);

            // Event fires when the user requests to add a folder into the list of recently visited folders
            WeakEventManager<IEditBookmarks, EditBookmarkEvent>
                .AddHandler(FolderItemsView.BookmarkFolder, "RequestEditBookmarkedFolders", FolderItemsView_RequestEditBookmarkedFolders);

            // This event is fired when a user opens a file
            WeakEventManager<IFileOpenEventSource, FileOpenEventArgs>
                .AddHandler(FolderItemsView, "OnFileOpen", FolderItemsView_OnFileOpen);

            WeakEventManager<ICanNavigate, BrowsingEventArgs>
                .AddHandler(TreeBrowser, "BrowseEvent", FolderTextPath_BrowseEvent);

            // Event fires when the user requests to add a folder into the list of recently visited folders
            WeakEventManager<IEditBookmarks, EditBookmarkEvent>
                .AddHandler(TreeBrowser.BookmarkFolder, "RequestEditBookmarkedFolders", FolderItemsView_RequestEditBookmarkedFolders);
        }
        #endregion constructor

        #region properties
        /// <summary>
        /// Gets the viewmodel that drives the folder picker control.
        /// </summary>
        public IBrowserViewModel TreeBrowser { get; }
        #endregion properties

        #region methods
        /// <summary>
        /// Master controller interface method to navigate all views
        /// to the folder indicated in <paramref name="folder"/>
        /// - updates all related viewmodels.
        /// </summary>
        /// <param name="itemPath"></param>
        /// <param name="requestor"</param>
        public override void NavigateToFolder(IPathModel itemPath)
        {
            try
            {
                // XXX Todo Keep task reference, support cancel, and remove on end?
                var timeout = TimeSpan.FromSeconds(5);
                var actualTask = new Task(() =>
                {
                    var request = new BrowseRequest(itemPath, _CancelToken.Token);

                    var t = Task.Factory.StartNew(() => NavigateToFolderAsync(request, null),
                                                        request.CancelTok,
                                                        TaskCreationOptions.LongRunning,
                                                        _OneTaskScheduler);

                    if (t.Wait(timeout) == true)
                        return;

                    _CancelToken.Cancel();       // Task timed out so lets abort it
                    return;                     // Signal timeout here...
                });

                actualTask.Start();
                actualTask.Wait();
            }
            catch (System.AggregateException e)
            {
                Logger.Error(e);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        /// Master controler interface method to navigate all views
        /// to the folder indicated in <paramref name="folder"/>
        /// - updates all related viewmodels.
        /// </summary>
        /// <param name="request"></param>
        private async Task<FinalBrowseResult> NavigateToFolderAsync(BrowseRequest request,
                                                                    object sender)
        {
            // Make sure the task always processes the last input but is not started twice
            await _SlowStuffSemaphore.WaitAsync();
            try
            {
                var newPath = request.NewLocation;
                var cancel = request.CancelTok;

                if (cancel != null)
                    cancel.ThrowIfCancellationRequested();

                TreeBrowser.SetExternalBrowsingState(true);
                FolderItemsView.SetExternalBrowsingState(true);
                FolderTextPath.SetExternalBrowsingState(true);

                FinalBrowseResult browseResult = null;

                // Navigate TreeView to this file system location
                if (TreeBrowser != sender)
                {
                    browseResult = await TreeBrowser.NavigateToAsync(request);

                    if (cancel != null)
                        cancel.ThrowIfCancellationRequested();

                    if (browseResult.Result != BrowseResult.Complete)
                        return FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);
                }

                // Navigate Folder ComboBox to this folder
                if (FolderTextPath != sender)
                {
                    browseResult = await FolderTextPath.NavigateToAsync(request);

                    if (cancel != null)
                        cancel.ThrowIfCancellationRequested();

                    if (browseResult.Result != BrowseResult.Complete)
                        return FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);
                }

                if (cancel != null)
                    cancel.ThrowIfCancellationRequested();

                // Navigate Folder/File ListView to this folder
                if (FolderItemsView != sender)
                {
                    browseResult = await FolderItemsView.NavigateToAsync(request);

                    if (cancel != null)
                        cancel.ThrowIfCancellationRequested();

                    if (browseResult.Result != BrowseResult.Complete)
                        return FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);
                }

                if (browseResult != null)
                {
                    if (browseResult.Result == BrowseResult.Complete)
                    {
                        SelectedFolder = newPath.Path;

                        // Log location into history of recent locations
                        NaviHistory.Forward(newPath);
                    }
                }

                return browseResult;
            }
            catch (Exception exp)
            {
                var result = FinalBrowseResult.FromRequest(request, BrowseResult.InComplete);
                result.UnexpectedError = exp;
                return result;
            }
            finally
            {
                TreeBrowser.SetExternalBrowsingState(true);
                FolderItemsView.SetExternalBrowsingState(false);
                FolderTextPath.SetExternalBrowsingState(false);

                _SlowStuffSemaphore.Release();
            }
        }

        /// <summary>
        /// Executes when the file open event is fired and class was constructed with statndard constructor.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void FolderItemsView_OnFileOpen(object sender,
                                                           FileOpenEventArgs e)
        {
            MessageBox.Show("File Open:" + e.FileName);
        }

        /// <summary>
        /// One of the controls has changed its location in the filesystem.
        /// This method is invoked to synchronize this change with all other controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderTextPath_BrowseEvent(object sender,
                                                FileSystemModels.Browse.BrowsingEventArgs e)
        {
            var location = e.Location;

            SelectedFolder = location.Path;

            if (e.IsBrowsing == false && e.Result == BrowseResult.Complete)
            {
                // XXX Todo Keep task reference, support cancel, and remove on end?
                try
                {
                    var timeout = TimeSpan.FromSeconds(5);
                    var actualTask = new Task(() =>
                    {
                        var request = new BrowseRequest(location, _CancelToken.Token);

                        var t = Task.Factory.StartNew(() => NavigateToFolderAsync(request, sender),
                                                            request.CancelTok,
                                                            TaskCreationOptions.LongRunning,
                                                            _OneTaskScheduler);

                        if (t.Wait(timeout) == true)
                            return;

                        _CancelToken.Cancel();           // Task timed out so lets abort it
                        return;                         // Signal timeout here...
                    });

                    actualTask.Start();
                    actualTask.Wait();
                }
                catch (System.AggregateException ex)
                {
                    Logger.Error(ex);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            else
            {
                if (e.IsBrowsing == true)
                {
                    // The sender has messaged: "I am changing location..."
                    // So, we set this property to tell the others:
                    // 1) Don't change your location now (eg.: Disable UI)
                    // 2) We'll be back to tell you the location when we know it
                    if (TreeBrowser != sender)
                        TreeBrowser.SetExternalBrowsingState(true);

                    if (FolderTextPath != sender)
                        FolderTextPath.SetExternalBrowsingState(true);

                    if (FolderItemsView != sender)
                        FolderItemsView.SetExternalBrowsingState(true);
                }
            }
        }
        #endregion methods
    }
}
