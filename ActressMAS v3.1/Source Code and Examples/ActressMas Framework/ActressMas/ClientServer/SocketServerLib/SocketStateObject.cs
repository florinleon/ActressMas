using SocketServerLib.Message;

namespace SocketServerLib.SocketHandler
{
    /// <summary>
    /// This class is used internally foe the asynchronous receiving.
    /// </summary>
    internal class SocketStateObject
    {
        public AbstractTcpSocketClientHandler workHandler = null;
        public const int BufferSize = 8192;
        public byte[] buffer = new byte[BufferSize];
        public AbstractMessage message = null;
        public byte[] pendingBuffer = null;
    }
}
