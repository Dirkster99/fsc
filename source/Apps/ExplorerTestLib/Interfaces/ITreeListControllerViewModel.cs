namespace ExplorerTestLib.Interfaces
{
    using FolderBrowser.Interfaces;

    /// <summary>
    /// Interface implements a folder/file view model class
    /// that can be used to dispaly filesystem related content in an ItemsControl.
    /// </summary>
    public interface ITreeListControllerViewModel : IListControllerViewModel
    {
        /// <summary>
        /// Gets the folder browser viewmodel that drives the tree view which displays
        /// drives and their folder items.
        /// </summary>
        IBrowserViewModel TreeBrowser { get; }
    }
}
