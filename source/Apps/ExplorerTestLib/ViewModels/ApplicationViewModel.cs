namespace ExplorerTestLib.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Input;
    using System.Xml.Serialization;
    using ExplorerTestLib.Interfaces;
    using FileSystemModels;
    using FileSystemModels.Interfaces;
    using FileSystemModels.Models;
    using FolderBrowser;
    using ViewModels.Base;

    /// <summary>
    /// Class implements an application viewmodel that manages the test application.
    /// </summary>
    public class ApplicationViewModel : Base.ViewModelBase
    {
        #region fields
        private ICommand mAddRecentFolder;
        private ICommand mRemoveRecentFolder;

        private int _SelectedTestviewModelIndex;
        private object[] _SelectedControllerTestViewModel;

        private string _SettingsXml = string.Empty;
        private string _SessionXml = string.Empty;

        private ICommand mTestSaveConfigCommand;
        private ICommand mTestLoadConfigCommand;
        #endregion fields

        #region constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        public ApplicationViewModel()
        {
            // Create and initialize list and combobox viemodel
            FolderView = ExplorerTestFactory.CreateList();

            FolderView.AddRecentFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            FolderView.AddRecentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), true);

            FolderView.AddFilter("Executeable files", "*.exe;*.bat");
            FolderView.AddFilter("Image files", "*.png;*.jpg;*.jpeg");
            FolderView.AddFilter("LaTex files", "*.tex");
            FolderView.AddFilter("Text files", "*.txt");
            FolderView.AddFilter("All Files", "*.*");

            // Create and initialize tree, list, and combobox viemodel
            FolderTreeView = ExplorerTestFactory.CreateTreeList();
            FolderTreeView.AddRecentFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            FolderTreeView.AddRecentFolder(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), true);

            FolderTreeView.AddFilter("Executeable files", "*.exe;*.bat");
            FolderTreeView.AddFilter("Image files", "*.png;*.jpg;*.jpeg");
            FolderTreeView.AddFilter("LaTex files", "*.tex");
            FolderTreeView.AddFilter("Text files", "*.txt");
            FolderTreeView.AddFilter("All Files", "*.*");

            _SelectedControllerTestViewModel = new object[2];
            _SelectedControllerTestViewModel[0] = FolderView;
            _SelectedControllerTestViewModel[1] = FolderTreeView;

            // Set this as selected test by default ...
            SelectedTestviewModelIndex = 0;
        }
        #endregion constructor

        #region properties
        /// <summary>
        /// Expose a viewmodel that controls the combobox folder drop down
        /// and the folder/file list view.
        /// </summary>
        public IListControllerViewModel FolderView { get; }

        /// <summary>
        /// Gets a viewmodel that drives the folderbrowser in sync with the
        /// folder listview and combobox...
        /// </summary>
        public ITreeListControllerViewModel FolderTreeView { get; }

        /// <summary>
        /// Gets the currently selected index to a test viewmodel for andvanced testing
        /// such as config persistence and restore ...
        /// </summary>
        public int SelectedTestviewModelIndex
        {
            get { return _SelectedTestviewModelIndex; }
            set
            {
                if (_SelectedTestviewModelIndex != value)
                {

                    _SelectedTestviewModelIndex = value;
                    NotifyPropertyChanged(() => this.SelectedTestviewModelIndex);
                    NotifyPropertyChanged(() => this.SelectedControllerTestViewModel);
                }
            }
        }

        /// <summary>
        /// Gets the viewmodel that is aasociated with the currently selected test index.
        /// </summary>
        public object SelectedControllerTestViewModel
        {
            get
            {
                return _SelectedControllerTestViewModel[_SelectedTestviewModelIndex];
            }
        }

        /// <summary>
        /// Gets/sets a text property for testing serialization and de-serialization
        /// of settings data for this component.
        /// </summary>
        public string SettingsXml
        {
            get { return _SettingsXml; }
            set
            {
                if (_SettingsXml != value)
                {
                    _SettingsXml = value;
                    NotifyPropertyChanged(() => this.SettingsXml);
                }
            }
        }

        /// <summary>
        /// Gets/sets a text property for testing serialization and de-serialization
        /// of session data for this component.
        /// </summary>
        public string SessionXml
        {
            get { return _SessionXml; }
            set
            {
                if (_SessionXml != value)
                {
                    _SessionXml = value;
                    NotifyPropertyChanged(() => this.SessionXml);
                }
            }
        }
        
        /// <summary>
        /// Gets/sets a command for testing the serialization of session
        /// and settings data for this component.
        /// </summary>
        public ICommand TestSaveConfigCommand
        {
            get
            {
                if (this.mTestSaveConfigCommand == null)
                    this.mTestSaveConfigCommand = new RelayCommand<object>(
                         (p) =>
                         {
                             var param = p as IConfigExplorerSettings;

                             if (param == null)
                                 return;

                             var model = new ExplorerSettingsModel();
                             var result = param.GetExplorerSettings(model);

                             // Write explorer settings into string for testing
                             var serializer = new XmlSerializer(typeof(ExplorerSettingsModel));
                             using (var writer = new StringWriter())   // Write Xml to string
                             {
                                 serializer.Serialize(writer, result);
                                 this.SettingsXml = writer.ToString(); // Convert result to string to read below
                             }
                             
                             // Write explorer session data into string for testing
                             serializer = new XmlSerializer(typeof(ExplorerUserProfile));
                             using (var writer = new StringWriter())            // Write Xml to string
                             {
                                 serializer.Serialize(writer, result.UserProfile);
                                 this.SessionXml = writer.ToString();  // Convert result to string to read below
                             }
                         });

                return this.mTestSaveConfigCommand;
            }
        }

        /// <summary>
        /// Gets/sets a command for testing the de-serialization of session
        /// and settings data for this component.
        /// </summary>
        public ICommand TestLoadConfigCommand
        {
            get
            {
                if (this.mTestLoadConfigCommand == null)
                    this.mTestLoadConfigCommand = new RelayCommand<object>(
                         (p) =>
                         {
                             var param = p as IConfigExplorerSettings;

                             if (param == null)
                                 return;

                             // Read Session data (if any useful available)
                             ExplorerUserProfile userProfile = new ExplorerUserProfile();
                             try
                             {
                               var deserializer = new XmlSerializer(typeof(ExplorerUserProfile));
                               using (var reader = new StringReader(this.SessionXml))  // Read Xml from string
                               {
                                   
                                   userProfile = (ExplorerUserProfile)deserializer.Deserialize(reader);
                               }
                             }
                             catch{}

                             // Read Settings data (if any useful available)
                             ExplorerSettingsModel settings = new ExplorerSettingsModel();
                             try
                             {
                               var deserializer = new XmlSerializer(typeof(ExplorerSettingsModel));
                               using (var reader = new StringReader(SettingsXml))        // Read Xml from string
                               {
                                 settings = (ExplorerSettingsModel)deserializer.Deserialize(reader);
                               }
                             }
                             catch{}

                             settings.SetUserProfile(userProfile);         // Bring session and settings together
                             param.ConfigureExplorerSettings(settings); // and apply to current instance
                         });

                return this.mTestLoadConfigCommand;
            }
        }

        #region Commands for tests with bookmarks
        /// <summary>
        /// Add a folder to the list of recent folders.
        /// </summary>
        public ICommand AddRecentFolder
        {
          get
          {
            if (this.mAddRecentFolder == null)
              this.mAddRecentFolder = new RelayCommand<object>((p) =>
              {
                this.AddRecentFolder_Executed(p);
              });
        
            return this.mAddRecentFolder;
          }
        }
        
        /// <summary>
        /// Remove a folder from the list of recent folders.
        /// </summary>
        public ICommand RemoveRecentFolder
        {
          get
          {
            if (this.mRemoveRecentFolder == null)
              this.mRemoveRecentFolder = new RelayCommand<object>(
                   (p) => this.RemoveRecentFolder_Executed(p),
                   (p) => this.FolderView.SelectedRecentLocation != null);
        
            return this.mRemoveRecentFolder;
          }
        }
        #endregion Commands for tests with bookmarks
        #endregion properties

        #region methods
        /// <summary>
        /// Call this method to initialize viewmodel items that might need to display
        /// progress information (e.g. call this in OnLoad() method of view)
        /// </summary>
        /// <param name="path"></param>
        public void InitializeViewModel(IPathModel path)
        {
            FolderView.NavigateToFolder(path);
            FolderTreeView.NavigateToFolder(path);
        }

        /// <summary>
        /// Free resources (if any) when application exits.
        /// </summary>
        public void ApplicationClosed()
        {

        }

        private void AddRecentFolder_Executed(object p)
        {
            string path;
            IListControllerViewModel vm;
            
            this.ResolveParameterList(p as List<object>, out path, out vm);
            
            if (vm == null)
              return;

            var browser = FolderBrowserFactory.CreateBrowserViewModel();

            path = (string.IsNullOrEmpty(path) == true ? PathFactory.SysDefault.Path : path);
            browser.InitialPath = path;

            var dlg = new FolderBrowser.Views.FolderBrowserDialog();
            
            var dlgViewModel = FolderBrowserFactory.CreateDialogViewModel(
                browser, vm.RecentFolders.CloneBookmark());
            
            dlg.DataContext = dlgViewModel;
            
            bool? bResult = dlg.ShowDialog();
            
            if (dlgViewModel.DialogCloseResult == true || bResult == true)
            {
                vm.CloneBookmarks(dlgViewModel.BookmarkedLocations, vm.RecentFolders);
                vm.AddRecentFolder(dlgViewModel.TreeBrowser.SelectedFolder, true);
            }
        }

        private void RemoveRecentFolder_Executed(object p)
        {
            string path;
            IListControllerViewModel vm;

            this.ResolveParameterList(p as List<object>, out path, out vm);

            if (vm == null || path == null)
                return;

            vm.RemoveRecentFolder(path);
        }

        /// <summary>
        /// Resolves the parameterlist retrieved from a multibinding command parameter
        /// which has packed parameters via List<object> container into 1 object.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="path"></param>
        /// <param name="vm"></param>
        private void ResolveParameterList(List<object> l,
                                          out string path, out IListControllerViewModel vm)
        {
            path = null;
            vm = null;

            if (l == null)
                return;

            foreach (var item in l)
            {
                if (item is IListItemViewModel)
                {
                    var pathItem = item as IListItemViewModel;

                    if (pathItem != null)
                        path = pathItem.ItemPath;
                }
                else
                    if (item is IListControllerViewModel)
                    {
                        var vmItem = item as IListControllerViewModel;

                        if (vmItem != null)
                            vm = item as IListControllerViewModel;
                    }
            }

            if (path == null)
                path = PathFactory.SysDefault.Path;
        }
        #endregion methods
    }
}
