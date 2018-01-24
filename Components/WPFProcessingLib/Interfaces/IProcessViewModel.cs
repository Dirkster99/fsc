namespace WPFProcessingLib.Interfaces
{
    using System;
    using System.Threading;
    using WPFProcessingLib.ViewModels;

    /// <summary>
    /// Inteface to an object that implements a processing viewmodel
    /// - object starts a background task and executes a result event
    /// method when the task is finished.
    /// </summary>
    public interface IProcessViewModel
    {
        #region properties
        /// <summary>
        /// Gets whether the current background thread can be cancelled during execution or not.
        /// 
        /// Note: The cancel signal is transferred to the executing thread. But the thread cancels only
        /// if the executing program exits upon receiving the cancel via CancelToken.
        /// </summary>
        bool IsCancelable { get; }

        /// <summary>
        /// Gets whether the viewmodel is currently processing data or not.
        /// </summary>
        bool IsProcessing { get; }
        #endregion properties

        #region methods
        /// <summary>
        /// Initialized and starts a new background thread that is processing the given action item.
        /// <para/>The process can be cancelled via the <seealso cref="CancellationTokenSource"/>
        /// parameter of the <seealso cref="Action"/> parameter. use this lamda expression syntax:
        /// <para/>...
        /// StartCancelableProcess(
        /// cts =>
        /// {
        ///   ...
        /// 
        ///   if (cts != null)
        ///     cts.Token.ThrowIfCancellationRequested();
        ///   ...
        /// 
        ///   ProcessSomething("blah blah blah", cts);
        ///     },
        /// .. other parameters
        /// );
        /// ...
        /// <para/>
        /// to implement the <paramref name="execFunc"/> parameter correctly.
        /// </summary>
        /// <param name="execFunc"></param>
        /// <param name="processFinsishedMethod"></param>
        /// <param name="processAlreadyRunningMsg"></param>
        /// <returns></returns>
        bool StartCancelableProcess(
            Action<CancellationTokenSource> execFunc,
            ProcessFinishedEvent processFinsishedMethod,
            string processAlreadyRunningMsg);

        /// <summary>
        /// Initialized and starts a new background thread
        /// that is processing the given action item.
        /// </summary>
        /// <param name="execFunc"></param>
        /// <param name="processFinsishedMethod"></param>
        /// <param name="processAlreadyRunningMsg"></param>
        /// <returns></returns>
        bool StartProcess(
            Action execFunc,
            ProcessFinishedEvent processFinsishedMethod,
            string processAlreadyRunningMsg);

        /// <summary>
        /// Call this method to cancel a current process.
        /// </summary>
        /// <returns></returns>
        bool Cancel();

        /// <summary>
        /// Standard dispose method of the <seealso cref="IDisposable" /> interface.
        /// </summary>
        void Dispose();
        #endregion methods
    }
}
