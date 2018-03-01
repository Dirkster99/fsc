namespace ExplorerTest
{
    using ExplorerTest.Interfaces;
    using ExplorerTest.ViewModels;

    public sealed class ExplorerTest
    {
        private ExplorerTest()
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
