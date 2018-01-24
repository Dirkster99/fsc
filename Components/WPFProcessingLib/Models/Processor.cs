namespace WPFProcessingLib.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using WPFProcessingLib.ProcessorLocal;

    /// <summary>
    /// This class implements a task based log4net loader
    /// that can run in an async task and generate an event
    /// when loading is done. The class can be used as a template
    /// for processing other tasks asynchronously.
    /// </summary>
    internal class Processor : IDisposable
    {
        #region fields
        public const string KeyProcessItems = "ProcessItems";

        private bool mAbortedWithCancel;
        private bool mAbortedWithErrors;
        private CancellationTokenSource mCancelTokenSource;

        private ApplicationException mInnerException;
        private Dictionary<string, object> mObjColl;

        /// <summary>
        /// This property is used to tell the context of the thread that started this thread
        /// originally. We use this to generate callbacks in the correct context when the long
        /// running task is done
        /// 
        /// Quote:
        /// One of the most important parts of this pattern is calling the MethodNameCompleted
        /// method on the same thread that called the MethodNameAsync method to begin with. You
        /// could do this using WPF fairly easily, by storing CurrentDispatcher—but then the
        /// nongraphical component could only be used in WPF applications, not in Windows Forms
        /// or ASP.NET programs. 
        /// 
        /// The DispatcherSynchronizationContext class addresses this need—think of it as a
        /// simplified version of Dispatcher that works with other UI frameworks as well.
        /// 
        /// http://msdn.microsoft.com/en-us/library/ms741870.aspx
        /// </summary>
        private DispatcherSynchronizationContext mRequestingContext;
        private bool mDisposed = false;
        #endregion fields

        #region Constructor
        /// <summary>
        /// Class constructor
        /// </summary>
        public Processor()
        {
            mAbortedWithErrors = mAbortedWithCancel = false;
            mInnerException = null;
            mObjColl = new Dictionary<string, object>();
        }

        #endregion Constructor

        public event EventHandler<ResultEvent> ProcessResultEvent;

        #region Properties
        public Dictionary<string, object> ResultObjects
        {
            get
            {
                return (mObjColl == null
                            ? new Dictionary<string, object>()
                            : new Dictionary<string, object>(mObjColl));
            }
        }

        protected ApplicationException InnerException
        {
            get { return mInnerException; }
            set { mInnerException = value; }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Cancel Asynchronous processing (if there is any right now)
        /// </summary>
        public void Cancel()
        {
            if (mCancelTokenSource != null)
                mCancelTokenSource.Cancel();
        }

        /// <summary>
        /// Process an asynchronous function
        /// </summary>
        /// <param name="execFunc"></param>
        /// <param name="async"></param>
        public void ExecuteAsynchronously(
            Action execFunc, bool async)
        {
            SaveThreadContext(async);

            mCancelTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = mCancelTokenSource.Token;

            Task taskToProcess = Task.Factory.StartNew(stateObj =>
            {
                mAbortedWithErrors = mAbortedWithCancel = false;
                mObjColl = new Dictionary<string, object>();
                var processResults = new ObservableCollection<string>();

                // This is not run on the UI thread.
                try
                {
                    cancelToken.ThrowIfCancellationRequested();
                    execFunc();
                }
                catch (OperationCanceledException exp)
                {
                    mAbortedWithCancel = true;
                    processResults.Add(exp.Message);
                }
                catch (Exception exp)
                {
                    string label = ProcessorStrings.STR_AN_ERROR_HAS_OCCURRED;
                    mInnerException = new ApplicationException(label, exp);
                    mAbortedWithErrors = true;

                    processResults.Add(exp.ToString());
                }

                return processResults;
                // End of async task with summary list of result strings
            },
            cancelToken).ContinueWith(ant =>
            {
                try
                {
                    ReportResultEvent(async);
                }
                catch (AggregateException aggExp)
                {
                    mAbortedWithErrors = true;
                    throw new Exception(aggExp.Message, aggExp);
                }
            });

            if (async == false) // Execute 'synchronously' via wait/block method
                taskToProcess.Wait();
        }

        /// <summary>
        /// Process an asynchronous function
        /// </summary>
        /// <param name="execFunc"></param>
        /// <param name="async"></param>
        public void ExecuteAsynchronously(
            Action<CancellationTokenSource> execFunc, bool async)
        {
            SaveThreadContext(async);

            mCancelTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = mCancelTokenSource.Token;

            Task taskToProcess = Task.Factory.StartNew(stateObj =>
            {
                mAbortedWithErrors = mAbortedWithCancel = false;
                mObjColl = new Dictionary<string, object>();
                var processResults = new ObservableCollection<string>();

                // This is not run on the UI thread.
                try
                {
                    cancelToken.ThrowIfCancellationRequested();
                    execFunc(mCancelTokenSource);
                }
                catch (OperationCanceledException exp)
                {
                    mAbortedWithCancel = true;
                    processResults.Add(exp.Message);
                }
                catch (Exception exp)
                {
                    string label = ProcessorStrings.STR_AN_ERROR_HAS_OCCURRED;
                    mInnerException = new ApplicationException(label, exp);
                    mAbortedWithErrors = true;

                    processResults.Add(exp.ToString());
                }

                return processResults;
                // End of async task with summary list of result strings
            },
            cancelToken).ContinueWith(ant =>
            {
                try
                {
                    ReportResultEvent(async);
                }
                catch (AggregateException aggExp)
                {
                    mAbortedWithErrors = true;
                    throw new Exception(aggExp.Message, aggExp);
                }
            });

            if (async == false) // Execute 'synchronously' via wait/block method
                taskToProcess.Wait();
        }

        /// <summary>
        /// Standard dispose method of the <seealso cref="IDisposable" /> interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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
                    if (mCancelTokenSource != null)
                    {
                        // Dispose of the curently used resources
                        //// this.mUpdateTargetFiles.DirtyFlagChangedEvent -= this.UpdateTargetFiles_DirtyFlagChangedEvent;

                        mCancelTokenSource.Dispose();

                        // NUll 'em out boys
                        Interlocked.Exchange(ref mCancelTokenSource, null);
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

        // Disposable types implement a finalizer.
        ~Processor()
        {
            Dispose(false);
        }

        /// <summary>
        /// Analyze AggregateException (typically returned from Task class) and return human-readable
        /// string for display in GUI.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="agg"></param>
        /// <param name="taskName"></param>
        /// <returns></returns>
        protected string PrintException(Task task, AggregateException agg, string taskName)
        {
            var sResult = string.Empty;

            foreach (Exception ex in agg.InnerExceptions)
            {
                sResult += string.Format("{0} Caught exception '{1}'", taskName, ex.Message) + Environment.NewLine;
            }

            sResult += string.Format("{0} cancelled? {1}", taskName, task.IsCanceled) + Environment.NewLine;

            return sResult;
        }

        /// <summary>
        /// Report a result to the attached eventholders (if any) on whether execution succeded or not.
        /// </summary>
        protected void ReportResultEvent(bool bAsnc)
        {
            // non-Asnyc threads are simply blocked until they finish
            // hence completed event is not required to fire
            if (bAsnc == false)
                return;

            SendOrPostCallback callback = ReportTaskCompletedEvent;

            if (mRequestingContext != null)
            {
                mRequestingContext.Post(callback, null);
                mRequestingContext = null;
            }
        }

        /// <summary>
        /// Save the threading context of a calling thread to enable event completion handling
        /// in original context when async task has finished (WPF, Winforms and co require this)
        /// </summary>
        /// <param name="bAsnc"></param>
        protected void SaveThreadContext(bool bAsnc)
        {
            // non-Asnyc threads are simply blocked until they finish
            // hence completed event is not required to fire
            if (bAsnc == false) return;

            if (mRequestingContext != null)
                throw new InvalidOperationException(ProcessorStrings.STR_CAN_HANDLE_ONLE_1_REQUEST_AT_A_TIME);

            mRequestingContext = (DispatcherSynchronizationContext)SynchronizationContext.Current;
        }

        /// <summary>
        /// Report the asynchronous task as having completed
        /// </summary>
        /// <param name="e"></param>
        private void ReportTaskCompletedEvent(object e)
        {
            if (ProcessResultEvent != null)
            {
                if (mAbortedWithErrors == false && mAbortedWithCancel == false)
                {
                    string label = ProcessorStrings.STR_SUCCESFUL_PROCESSING;
                    ProcessResultEvent(this, new ResultEvent(label, false, false, mObjColl));
                }
                else
                {
                    string label = ProcessorStrings.STR_UNSUCCESFUL_PROCESSING;
                    ProcessResultEvent(this, new ResultEvent(label, mAbortedWithErrors, mAbortedWithCancel,
                                                             mObjColl, mInnerException));
                }
            }
        }
        #endregion Methods
    }
}
