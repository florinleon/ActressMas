using SocketServerLib.Message;

namespace SocketServerLib.SocketHandler
{
    /// <summary>
    /// This class represents the object for a received message in the asynchronous receiving.
    /// </summary>
    internal class ReceiveMessageStateObject
    {
        public AbstractTcpSocketClientHandler Handler { get; set; }
        public AbstractMessage Message { get; set; }
    }
}
