namespace WPFProcessingLib.Models
{
    using System;

    /// <summary>
    /// System throws this exception when the consumer of the library starts
    /// a background process on a specific viewmodel for a second time.
    /// This situation occurs when consumer starts a background process on 
    /// a process viemodel without waiting for the already running process to exit.
    /// </summary>
    public class ProcessAlreadyRunningException : Exception
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="message"></param>
        public ProcessAlreadyRunningException(string message)
            : base (message)
        {
        }
    }
}
