using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SocketServerLib.SocketHandler;
using System.Diagnostics;

namespace SocketServerLib.Message
{
    /// <summary>
    /// This abstact class represent A message. You have to extend and implement this class to define your message.
    /// The message header is already included.
    /// </summary>
    internal abstract class AbstractMessage
    {
        /// <summary>
        /// The message header.
        /// </summary>
        protected AbstractMessageHeader header = null;
        /// <summary>
        /// The body as byte[]
        /// </summary>
        protected byte[] body = null;
        /// <summary>
        /// Index in the buffer
        /// </summary>
        private int bufferIndex = -1;
        /// <summary>
        /// Buffer for pending message in receiving
        /// </summary>
        private byte[] previousBuffer = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AbstractMessage()
        {
        }

        /// <summary>
        /// Return an empty instance of a Message Header. Implement this method to define your message class.
        /// </summary>
        /// <returns>The empty Message Header instance</returns>
        protected abstract AbstractMessageHeader GetMessageHeaderInstance();

        #region Properties
        
        /// <summary>
        /// The Client UID.
        /// </summary>
        public string ClientUID
        {
            get
            {
                return header.ClientUID;
            }
        }

        /// <summary>
        /// The message UID.
        /// </summary>
        public string MessageUID
        {
            get
            {
                return header.MessageUID;
            }
        }

        /// <summary>
        /// The message length included the header.
        /// </summary>
        public int MessageLength
        {
            get
            {
                return header.MessageLength;
            }
        }

        /// <summary>
        /// The Message Header.
        /// </summary>
        internal AbstractMessageHeader Header
        {
            get
            {
                return header;
            }
        }

        #endregion

        /// <summary>
        /// The full message (header + body) as byte[]
        /// </summary>
        /// <returns>The buffer contains the full message</returns>
        public virtual byte[] GetEnvelope()
        {
            byte[] buffer = new byte[this.Header.MessageLength];
            int off = this.Header.Write(buffer, 0);
            Array.Copy(this.body, 0, buffer, off, this.body.Length);
            return buffer;
        }

        /// <summary>
        /// The message body as byte[]
        /// </summary>
        /// <returns>The buffer contains the message body</returns>
        public virtual byte[] GetBuffer()
        {
            return body;
        }

        /// <summary>
        /// Check if the message is complete.
        /// </summary>
        /// <returns>Return true if the message is complete, otherwise false.</returns>
        public bool IsComplete()
        {
            return bufferIndex == -1;
        }

        #region Read methods on the buffer

        /// <summary>
        /// Read the first message contained in the buffer.
        /// </summary>
        /// <param name="buffer">The buffer used for the receiving</param>
        /// <param name="len">The real size of the buffer</param>
        /// <returns>0 if the message in the buffer is not complete. Otherwise the len of the message read</returns>
        private int ReadFirstMessage(byte[] buffer, int len)
        {
            return ReadFirstMessage(buffer, 0, len);
        }

        /// <summary>
        /// Read the first message contained in the buffer.
        /// </summary>
        /// <param name="buffer">The buffer used for the receiving</param>
        /// <param name="offset">The offset for start read the buffer</param>
        /// <param name="len">The real size of the buffer</param>
        /// <returns>0 if the message in the buffer is not complete. Otherwise the len of the message read</returns>
        private int ReadFirstMessage(byte[] buffer, int offset, int len)
        {
            // Get a Message Header instance
            header = GetMessageHeaderInstance();
            // Read the Message Header from the buffer
            int idx = header.Read(buffer, offset);
            // Check if the message is complete
            if (!header.IsComplete())
            {
                // If it's not, save the buffer in the previous buffer (pending message) and return 0.
                previousBuffer = new byte[len - offset];
                Buffer.BlockCopy(buffer, offset, previousBuffer, 0, previousBuffer.Length);
                bufferIndex = 0;
                return 0;
            }
            // Get the message body
            body = new byte[header.MessageLength - header.HeaderLength];
            int max = (header.MessageLength >= len) ? (len - header.HeaderLength) : (header.MessageLength - header.HeaderLength);
            Array.Copy(buffer, idx, body, 0, max);
            // Check if the message is complete.
            if (body.Length > len - header.HeaderLength)
            {
                // If it's not, return 0.
                bufferIndex = (len - header.HeaderLength);
                return 0;
            }
            bufferIndex = -1;
            return (header.MessageLength >= len) ? 0 : (header.MessageLength);
        }

        /// <summary>
        /// Add a buffer to the pending message and try to read the first message.
        /// </summary>
        /// <param name="buffer">The buffer to add</param>
        /// <param name="len">The real size of the buffer</param>
        /// <returns>0 if the message in the buffer is still not complete. Otherwise the len of the message read</returns>
        private int AppendBuffer(byte[] buffer, int len)
        {
            if (!header.IsComplete())
            {
                int prevLen = previousBuffer.Length;
                byte[] fullBuffer = new byte[buffer.Length + prevLen];
                Buffer.BlockCopy(previousBuffer, 0, fullBuffer, 0, previousBuffer.Length);
                Buffer.BlockCopy(buffer, 0, fullBuffer, previousBuffer.Length, buffer.Length);
                previousBuffer = null;
                int moremsg = ReadFirstMessage(fullBuffer, fullBuffer.Length);
                if (moremsg != 0)
                {
                    moremsg -= prevLen;
                }
                return moremsg;
            }
            int max = ((bufferIndex + len) > body.Length) ? (body.Length - bufferIndex) : len;
            Array.Copy(buffer, 0, body, bufferIndex, max);
            bufferIndex += max;
            if (bufferIndex == (header.MessageLength - header.HeaderLength))
            {
                bufferIndex = -1;
                return (max == len) ? 0 : max;
            }
            return 0;
        }

        /// <summary>
        /// Try to read a message from the buffer.
        /// </summary>
        /// <param name="message">The destination message</param>
        /// <param name="state">The state object</param>
        /// <param name="byteRead">The umber of bytes in the input buffer</param>
        /// <returns>The message read, otherwise false.</returns>
        internal static AbstractMessage TryReadMessage(AbstractMessage message, SocketStateObject state, int byteRead)
        {
            AbstractMessage messageRead = null;
            int moreMessage = 0;
            byte[] buffer = state.buffer;  // Get buffer

            if (state.pendingBuffer != null) //Check for pending data and merge it
            {
                buffer = new byte[byteRead + state.pendingBuffer.Length];
                Array.Copy(state.pendingBuffer, 0, buffer, 0, state.pendingBuffer.Length);
                Array.Copy(state.buffer, 0, buffer, state.pendingBuffer.Length, byteRead);
                byteRead = buffer.Length;
            }
            state.pendingBuffer = null;
            if (state.message == null)
            {
                state.message = message;
                moreMessage = state.message.ReadFirstMessage(buffer, byteRead);
                Trace.WriteLine(string.Format("Receive 1st package MessageUID {0} ClientUID {1}", state.message.MessageUID, state.message.ClientUID));
            }
            else
            {
                moreMessage = state.message.AppendBuffer(buffer, byteRead);
                Trace.WriteLine(string.Format("Receive more package MessageUID {0} ClientUID {1}", state.message.MessageUID, state.message.ClientUID));
            }
            if (state.message.IsComplete())
            {
                Trace.WriteLine(string.Format("Receive complete message {0} len {1}", state.message.MessageUID, state.message.MessageLength));
                messageRead = state.message;
                Trace.WriteLine(string.Format("Prepare to receive a new message. moreMessage = {0}", moreMessage));
                state.message = null;
                if (moreMessage > 0)
                {
                    state.pendingBuffer = new byte[byteRead - moreMessage];
                    Array.Copy(buffer, moreMessage, state.pendingBuffer, 0, state.pendingBuffer.Length);
                    Trace.WriteLine(string.Format("Copy {0} bytes to pending buffer", state.pendingBuffer.Length));
                }
            }
            return messageRead;
        }

        #endregion
    }
}
