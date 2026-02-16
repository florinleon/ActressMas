
namespace SocketServerLib.Message
{
    /// <summary>
    /// This abstract class represent a message header. You have to extend and implement this class to define your message header.
    /// </summary>
    internal abstract class AbstractMessageHeader
    {
        /// <summary>
        /// Flag for a complete header read from a buffer.
        /// </summary>
        protected bool complete = true;

        /// <summary>
        /// Constructor for an empty message header.
        /// </summary>
        public AbstractMessageHeader()
        {
        }

        /// <summary>
        /// Get the ClientUID.
        /// </summary>
        public abstract string ClientUID { get; }
        /// <summary>
        /// Get the Message UID.
        /// </summary>
        public abstract string MessageUID { get; }
        /// <summary>
        /// Get the message length.
        /// </summary>
        public abstract int MessageLength { get; }
        /// <summary>
        /// Get the header length.
        /// </summary>
        public abstract int HeaderLength { get; }

        /// <summary>
        /// Write the header in a buffer.
        /// </summary>
        /// <param name="destBuffer">The buffer</param>
        /// <param name="offset">The offset in the buffer</param>
        /// <returns>The next position in the buffer after the header</returns>
        public abstract int Write(byte[] destBuffer, int offset);
        /// <summary>
        /// Read the header from a buffer.
        /// </summary>
        /// <param name="sourceBuffer">The buffer</param>
        /// <param name="offset">The offset in the buffer</param>
        /// <returns>The next position in the buffer after the header. Return 0 in case the hedaer is not complete</returns>
        public abstract int Read(byte[] sourceBuffer, int offset);

        /// <summary>
        /// Chekk if the header is complete.
        /// </summary>
        /// <returns>True if the header is complete, otherwise false</returns>
        public bool IsComplete()
        {
            return complete;
        }
    }
}
