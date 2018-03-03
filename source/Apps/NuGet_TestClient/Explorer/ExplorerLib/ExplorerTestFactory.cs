namespace ExplorerTestLib
{
    using ExplorerLib.Interfaces;
    using ExplorerLib.ViewModels;

    public sealed class ExplorerTestFactory
    {
        private ExplorerTestFactory()
        { }

        /// <summary>
        /// Creates a <see cref="IListControllerViewModel"/> instance and returns it.
        /// </summary>
        /// <returns></returns>
        public static IListControllerViewModel CreateList()
        {
            return new ListControllerViewModel();
        }

        public static ITreeListControllerViewModel CreateTreeList()
        {
            return new TreeListControllerViewModel();
        }
    }
}
