using System;

namespace SocketServerLib.SocketHandler
{
    /// <summary>
    /// This class represent a socket exception in the libray.
    /// </summary>
    internal class TcpSocketException : Exception
    {
        /// <summary>
        /// Flag to decide if a disconnection is required due the exception.
        /// </summary>
        private bool disconnectionRequired = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="disconnectionRequired">Flag to indicate if a disconnection is required</param>
        public TcpSocketException(string message, bool disconnectionRequired) 
            :base(message)
        {
            this.disconnectionRequired = disconnectionRequired;
        }

        /// <summary>
        /// Constructor for an exception that not requied a disconnection.
        /// </summary>
        /// <param name="message">The exception message</param>
        public TcpSocketException(string message) 
            :this(message, false)
        {
        }

        /// <summary>
        /// Get if a disconnection is required.
        /// </summary>
        public bool DisconnectionRequired
        {
            get
            {
                return this.disconnectionRequired;
            }
        }
    }
}
