namespace WPFProcessingLib.ViewModels
{
    using WPFProcessingLib.Models;
    using System;
    using System.Threading;

    /// <summary>
    /// This is a callback method that is always called when
    /// the internal processing is finished.
    /// (even when it failed to finish after initialization).
    /// </summary>
    /// <param name="loadWasSuccessful"></param>
    public delegate void ProcessFinishedEvent(bool processWasSuccessful, Exception exp, string caption);

    /// <summary>
    /// Class implements a processing viewmodel that starts a background task
    /// and executes a result event method when the task is finished.
    /// </summary>
    internal class ProcessViewModel : Base.ViewModelBase, IDisposable, WPFProcessingLib.Interfaces.IProcessViewModel
    {
        #region fields
        private static object lockObject = new object();

        private Processor mProcessor;
        private ProcessFinishedEvent mProcessFinsishedMethod;

        private string mProcessAlreadyRunningMsg;

        private bool mIsProcessing;

        private bool mDisposed;
        private bool mIsCancelable;
        #endregion fields

        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        public ProcessViewModel()
        {
            mProcessor = null;
            mProcessAlreadyRunningMsg = string.Empty;

            mIsProcessing = false;
            mDisposed = false;
            mIsCancelable = false;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets whether the viewmodel is currently processing data or not.
        /// </summary>
        public bool IsProcessing
        {
            get
            {
                return mIsProcessing;
            }

            private set
            {
                if (mIsProcessing != value)
                {
                    mIsProcessing = value;
                    RaisePropertyChanged(() => IsProcessing);
                }
            }
        }

        /// <summary>
        /// Gets whether the current background thread can be cancelled during execution or not.
        /// 
        /// Note: The cancel signal is transferred to the executing thread. But the thread cancels only
        /// if the executing program exits upon receiving the cancel via CancelToken.
        /// </summary>
        public bool IsCancelable
        {
            get
            {
                return mIsCancelable;
            }

            private set
            {
                if (mIsCancelable != value)
                {
                    mIsCancelable = value;
                    RaisePropertyChanged(() => IsCancelable);
                }
            }
        }
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
        /// <param name="processAlreadyRunningCaption"></param>
        /// <param name="errorMessageCaption"></param>
        /// <returns></returns>
        public bool StartCancelableProcess(Action<CancellationTokenSource> execFunc,
                                           ProcessFinishedEvent processFinsishedMethod,
                                           string processAlreadyRunningMsg)
        {
            return this.StartProcessCancelableOrNot(execFunc, null, processFinsishedMethod,
                                                    processAlreadyRunningMsg);
        }

        /// <summary>
        /// Initialized and starts a new background thread
        /// that is processing the given action item.
        /// </summary>
        /// <param name="execFunc"></param>
        /// <param name="processFinsishedMethod"></param>
        /// <param name="processAlreadyRunningMsg"></param>
        /// <param name="processAlreadyRunningCaption"></param>
        /// <param name="errorMessageCaption"></param>
        /// <returns></returns>
        public bool StartProcess(Action execFunc,
                                 ProcessFinishedEvent processFinsishedMethod,
                                 string processAlreadyRunningMsg)
        {
            return this.StartProcessCancelableOrNot( null, execFunc,
                                                     processFinsishedMethod,
                                                     processAlreadyRunningMsg);
        }

        /// <summary>
        /// Call this method to cancel a current process.
        /// </summary>
        /// <returns></returns>
        public bool Cancel()
        {
            if (this.IsProcessing == true && this.mProcessor != null)
            {
                this.mProcessor.Cancel();
                this.IsCancelable = false; // Allow cancelling only once

                return true;
            }

            return false;
        }

        /// <summary>
        /// Standard dispose method of the <seealso cref="IDisposable" /> interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Method is executed when the background process finishes and returns here
        /// because it was cancelled or is done processing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProcessorResultEvent(object sender, ResultEvent e)
        {
            try
            {
                lock (lockObject)
                {
                    if (mProcessor != null)
                    {
                        mProcessor.ProcessResultEvent -= ProcessorResultEvent;
                        mProcessor.Dispose();
                        mProcessor = null;
                    }

                    IsProcessing = false;

                    if (e.InnerException != null)
                    {
                        var exp = new ApplicationException(e.Message, e.InnerException) { Source = "ProcessViewModel" };
                        exp.Data.Add("Process cancelled?", e.Cancel.ToString());

                        ////mMsgBox.Show(exp, mErrorMessageCaption, MsgBoxButtons.OK, MsgBoxImage.Alert);

                        if (mProcessFinsishedMethod != null)
                            mProcessFinsishedMethod(false, exp, ProcessorLocal.ProcessorStrings.STR_UNSUCCESFUL_PROCESSING);
                    }
                    else
                    {
                        if (mProcessFinsishedMethod != null)
                            mProcessFinsishedMethod(true, null, null);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Method evaluates <paramref name="execCancelableFunc"/> or <paramref name="execFunc"/> and starts
        /// ans asynchronous process that can be cancelled or not, depending on whether one of the
        /// parameter is null or not.
        /// 
        /// A valid call of this method requires that either  <paramref name="execCancelableFunc"/> or
        /// <paramref name="execFunc"/> is null.
        /// </summary>
        /// <param name="execCancelableFunc"></param>
        /// <param name="execFunc"></param>
        /// <param name="processFinsishedMethod"></param>
        /// <param name="msg"></param>
        /// <param name="processAlreadyRunningMsg"></param>
        /// <param name="processAlreadyRunningCaption"></param>
        /// <param name="errorMessageCaption"></param>
        /// <returns></returns>
        private bool StartProcessCancelableOrNot(Action<CancellationTokenSource> execCancelableFunc,
                                                 Action execFunc,
                                                 ProcessFinishedEvent processFinsishedMethod,
                                                 string processAlreadyRunningMsg)
        {
            try
            {
                lock (lockObject)
                {
                    if (mProcessor != null)
                    {
                        throw new ProcessAlreadyRunningException(mProcessAlreadyRunningMsg);

////                        if (MsgBox.Show(
////                            ////"A process is currently in progress. Would you like to cancel the current process?",
////                            ////"Process in progress..."
////                            mProcessAlreadyRunningMsg, mProcessAlreadyRunningCaption,
////                            MsgBoxButtons.YesNo, MsgBoxImage.Question, MsgBoxResult.No) == MsgBoxResult.Yes)
////                        {
////                            if (mProcessor != null)
////                            {
////                                mProcessor.Cancel();
////                                return false;
////                            }
////                            else
////                                return false;
////                        }
                    }

                    mProcessor = new Processor();
                    mProcessor.ProcessResultEvent += ProcessorResultEvent;
                    IsProcessing = true;
                    mProcessFinsishedMethod = processFinsishedMethod;

                    // Initialize this here to enable usage of the previous parameters in the previous part
                    mProcessAlreadyRunningMsg = processAlreadyRunningMsg;

                    if (execCancelableFunc != null)
                    {
                        this.IsCancelable = true;
                        mProcessor.ExecuteAsynchronously(execCancelableFunc, true);
                    }
                    else
                    {
                        this.IsCancelable = false;
                        mProcessor.ExecuteAsynchronously(execFunc, true);
                    }

                    return true;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Source: http://www.codeproject.com/Articles/15360/Implementing-IDisposable-and-the-Dispose-Pattern-P
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (mDisposed == false)
            {
                if (disposing == true)
                {
                    // Dispose of the curently used resources
                    if (mProcessor != null)
                    {
                        mProcessor.Dispose();
                        Interlocked.Exchange(ref mProcessor, null);
                    }
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }

            mDisposed = true;

            //// If it is available, make the call to the
            //// base class's Dispose(Boolean) method
            //// base.Dispose(disposing);
        }

        /// <summary>
        /// Class finalizer
        /// </summary>
        ~ProcessViewModel()
        {
            Dispose(true);
        }
        #endregion methods
    }
}
