/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  Using Actress, a C# port of the F# MailboxProcessor                   *
 *  by Kevin Thompson: https://github.com/kthompson/Actress               *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/
 
namespace ActressMas
{
    public class Message
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }
        public string ConversationId { get; set; }

        public Message()
        {
        }

        public Message(string sender, string receiver, string content)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Content = content;
        }

        public Message(string sender, string receiver, string content, string conversationId)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Content = content;
            this.ConversationId = conversationId;
        }
    }
}