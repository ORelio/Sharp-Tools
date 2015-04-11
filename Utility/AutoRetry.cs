using System;
using System.Threading;

namespace SharpTools
{
    /// <summary>
    /// Allow easy retries on pieces of code
    /// </summary>
    /// <remarks>
    /// By ORelio - (c) 2014 - CDDL 1.0
    /// </remarks>
    public class AutoRetry
    {
        /// <summary>
        /// Perform the specified action, retrying if an exception is caught.
        /// If last attempt fails, exception is not caught.
        /// </summary>
        /// <param name="action">Action to run</param>
        /// <param name="attempts">Maximum amount of attempts</param>
        public static void Perform(Action action, int attempts = 3)
        {
            Perform(action, null, attempts);
        }

        /// <summary>
        /// Perform the specified action, retrying after performing the specified onError action if an exception is caught.
        /// If last attempt fails, exception is not caught and onError is not performed.
        /// </summary>
        /// <param name="action">Action to run</param>
        /// <param name="onError">Action to run before retrying if an error occured</param>
        /// <param name="attempts">Maximum amount of attempts</param>
        public static void Perform(Action action, Action onError, int attempts = 3)
        {
            if (attempts < 1)
                attempts = 1;

            bool success;

            do
            {
                success = true;
                try { action(); }
                catch (Exception e)
                {
                    success = false;
                    if (attempts <= 1)
                    {
                        if (!(e is ThreadAbortException))
                            throw;
                    }
                    else if (onError != null)
                    {
                        onError();
                    }
                }
                attempts--;
            } while (!success && attempts > 0);
        }
    }
}