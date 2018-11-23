using System;

namespace SocketServerLib.Message
{
    /// <summary>
    /// This class represents the message exception in the library.
    /// </summary>
    internal class MessageException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message of the exception</param>
        public MessageException(string message) 
            :base(message)
        {
        }
    }
}
