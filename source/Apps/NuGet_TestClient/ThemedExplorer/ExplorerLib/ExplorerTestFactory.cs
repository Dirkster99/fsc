namespace ExplorerTestLib
{
    using ExplorerLib.Interfaces;
    using ExplorerLib.ViewModels;

    public sealed class ExplorerTestFactory
    {
        private ExplorerTestFactory()
        { }

        /// <summary>
        /// Creates a <see cref="ITreeListControllerViewModel"/> instance and returns it.
        /// This instance can be used to manage an Explorer like view into the filse system.
        /// This Explorer like view already supports a folder treeview and a folder/files list view.
        /// </summary>
        /// <returns></returns>
        public static ITreeListControllerViewModel CreateTreeList()
        {
            return new TreeListControllerViewModel();
        }
    }
}
