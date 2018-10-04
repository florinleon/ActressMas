/**************************************************************************
 *                                                                        *
 *  Description: Actress is a C# port of the F# MailboxProcessor          *
 *  Website:     https://github.com/kthompson/Actress                     *
 *  Copyright:   (c) 2017, Kevin Thompson                                 *
 *                                                                        *
 **************************************************************************/

using System;

namespace ActressMas
{
    public interface IReplyChannel<TReply>
    {
        void Reply(TReply reply);
    }

    internal class ReplyChannel<TReply> : IReplyChannel<TReply>
    {
        private readonly Action<TReply> _replyf;

        internal ReplyChannel(Action<TReply> replyf)
        {
            _replyf = replyf;
        }

        public void Reply(TReply reply)
        {
            _replyf(reply);
        }
    }
}