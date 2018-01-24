
namespace FolderBrowser.FileSystem.Interfaces
{
    using System;

    public interface IFSItemViewModel
    {
        System.Windows.Media.ImageSource DisplayIcon { get; }
        string DisplayName { get; }
        string FullPath { get; }
        FileSystemModels.Models.PathModel GetModel { get; }
        int Indentation { get; }
        bool ShowToolTip { get; }
        FileSystemModels.Models.FSItems.FSItemType Type { get; }

        bool DirectoryPathExists();
        string DisplayItemString();
        void SetDisplayIcon(System.Windows.Media.ImageSource src = null);
    }
}
