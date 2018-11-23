using System;
using System.Threading;
using System.Diagnostics;

namespace SocketServerLib.Threads
{
    /// <summary>
    /// This abstract class represents a thread. You have to implement the method ThreadLoop to define what the thread continuoisly do.
    /// To start the thread call Init and then StartUp. To stop the thread call Shutdown.
    /// </summary>
    internal abstract class AbstractThread : IDisposable
    {
        /// <summary>
        /// The internal thread.
        /// </summary>
        private Thread th = null;
        /// <summary>
        /// Flag to stop the thread.
        /// </summary>
        protected bool shutdown = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AbstractThread()
        {
        }

        /// <summary>
        /// Implement this method to define what the thread do in its infinite loop.
        /// </summary>
        protected abstract void ThreadLoop();

        #region Public methods to start and stop the thread

        /// <summary>
        /// Init the thread.
        /// </summary>
        public virtual void Init()
        {
            th = new Thread(this.Run);
            th.Name = this.GetType().FullName;
        }

        /// <summary>
        /// Start the thread.
        /// </summary>
        public virtual void StartUp()
        {
            th.Start();
        }

        /// <summary>
        /// Stop the thread.
        /// </summary>
        public virtual void Shutdown()
        {
            lock (this)
            {
                if (shutdown)
                {
                    return;
                }
                shutdown = true;
                if (th != null)
                {
                    if (!th.Join(5000))
                    {
                        th.Interrupt();
                    }
                    if (!th.Join(5000))
                    {
                        th.Abort();
                    }
                    th = null;
                }
            }
        }

        /// <summary>
        /// This is the internal thread loop. The shutdown flag is checked and if false the abstract ThreadLoop method is called.
        /// It's a infinite loop until the shutdown.
        /// </summary>
        protected virtual void Run()
        {
            while (!shutdown)
            {
                try
                {
                    ThreadLoop();
                }
                catch (ThreadInterruptedException thInt)
                {
                    Trace.WriteLine(string.Format("Get exception {0} to exit from thread", thInt.GetType().FullName));
                }
                catch (ThreadAbortException thAbort)
                {
                    Trace.WriteLine(string.Format("Get exception {0} to exit from thread", thAbort.GetType().FullName));
                    shutdown = true;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("Exception {0}", ex));
                }
            }
            Trace.WriteLine("Exit from Run. Thread is stopped.");
        }

        #endregion

        #region IDisposable Members
        
        /// <summary>
        /// Shutdown and dispose the thread.
        /// </summary>
        public virtual void Dispose()
        {
            Shutdown();
        }

        #endregion

    }
}
