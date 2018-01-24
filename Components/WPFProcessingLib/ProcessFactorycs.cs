namespace WPFProcessingLib
{
    using WPFProcessingLib.Interfaces;
    using WPFProcessingLib.ViewModels;

    /// <summary>
    /// Class contains factory methods for initializing base services of this library.
    /// </summary>
    public class ProcessFactory
    {
        /// <summary>
        /// Creates a new viewmodel object for creating and
        /// observing background threads.
        /// </summary>
        /// <returns></returns>
        public static IProcessViewModel CreateProcessViewModel()
        {
            return new ProcessViewModel();
        }
    }
}
