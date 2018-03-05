namespace FsContentDialogDemo.Demos
{
    using FileSystemModels.Interfaces.Bookmark;
    using FolderBrowser;
    using MWindowDialogLib.Dialogs;
    using MWindowInterfacesLib.Interfaces;
    using System.Threading.Tasks;

    public class FolderBrowserControler : FsContentDialogDemo.ViewModels.Base.ModelBase
    {
        #region Fields
        private bool _SpecialFolderVisibility;
        private string _InitialPath;
        private IBookmarksViewModel _BookmarkedLocations;
        #endregion Fields

        #region ctor
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="initialPath"></param>
        /// <param name="bookmarks"></param>
        /// <param name="specialFolderVisibility"></param>
        public FolderBrowserControler(string initialPath
                                     , IBookmarksViewModel bookmarks
                                     , bool specialFolderVisibility = true)
            : this()
        {
            _SpecialFolderVisibility = specialFolderVisibility;
            _InitialPath = initialPath;

            BookmarkedLocations = bookmarks;
        }

        /// <summary>
        /// Default Class Constructor
        /// </summary>
        protected FolderBrowserControler()
        {
            _SpecialFolderVisibility = true;
            _InitialPath = string.Empty;
            _BookmarkedLocations = null;
        }
        #endregion ctor

        #region properties
        /// <summary>
        /// Gets a bookmark folder property to manage bookmarked folders.
        /// </summary>
        public IBookmarksViewModel BookmarkedLocations
        {
            get
            {
                return _BookmarkedLocations;
            }

            private set
            {
                if (value == null)
                    _BookmarkedLocations = null;
                else
                    _BookmarkedLocations = value.CloneBookmark();
            }
        }
        #endregion  properties

        #region methods
        /// <summary>
        /// Shows a sample progress dialog that was invoked via a bound viewmodel.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="progressIsFinite"></param>
        internal async Task<string> ShowContentDialogFromVM(
              object context
            , bool progressIsFinite
            )
        {
            // See Loaded event in FolderBrowserTreeView_Loaded method to understand initial load
            var treeBrowserVM = FolderBrowserFactory.CreateBrowserViewModel(_SpecialFolderVisibility
                                                                          , _InitialPath);

            // Switch updates to view of by default to speed up load of view
            // Loading the view will kick-off the browsing via View.Loaded Event
            // and that in turn will switch on view updates ...
            treeBrowserVM.UpdateView = false;

            var fsDlg = FolderBrowserFactory.CreateDialogViewModel(treeBrowserVM, BookmarkedLocations);

            var customDialog = CreateFolderBrowserDialog(new ViewModels.FolderBrowserContentDialogViewModel(fsDlg));

            var coord = GetService<IContentDialogService>().Coordinator;
            var manager = GetService<IContentDialogService>().Manager;

            string returnPath = null;

            // Show a progress dialog to initialize the viewmodel - in case file system is slow...
            await coord.ShowMetroDialogAsync(context, customDialog).ContinueWith
            (
                (t) =>
                {
                    if (t.Result == DialogIntResults.OK)
                        returnPath = treeBrowserVM.SelectedFolder;
                }
            );

            if (fsDlg.BookmarkedLocations != null)
                this.BookmarkedLocations = fsDlg.BookmarkedLocations.CloneBookmark();

            return returnPath;
        }

        /// <summary>
        /// Creates a <seealso cref="MWindowDialogLib.Dialogs.CustomDialog"/> frame that contains a
        /// <seealso cref="Demos.Views.FolderBrowserContentDialogView"/> with a
        /// in its content and has a <param name="viewModel"/> attached to its datacontext.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private CustomDialog CreateFolderBrowserDialog(object viewModel)
        {
            var dlg = new CustomDialog(new Demos.Views.FolderBrowserContentDialogView(),viewModel);

            // Strech dialog to always use complete space since dialog will otherwise
            // flip open/shut/flicker when browsing beween small path 'c:\'
            // and deep/long path like "c:\windows\system32"
            dlg.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            dlg.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            return dlg;
        }
        #endregion methods
    }
}
