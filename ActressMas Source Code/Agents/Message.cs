/**************************************************************************
 *                                                                        *
 *  Description: ActressMas multi-agent framework                         *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018, Florin Leon                                    *
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
    /// <summary>
    /// A message that the agents use to communicate. In an agent-based system, the communication between the agents is exclusively performed by exchanging messages.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Initializes a new instance of the Message class with an empty message.
        /// </summary>
        public Message()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Message class.
        /// </summary>
        /// <param name="sender">The name of the agent that sends the message</param>
        /// <param name="receiver">The name of the agent that needs to receive the message</param>
        /// <param name="content">The content of the message</param>
        public Message(string sender, string receiver, string content)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the Message class.
        /// </summary>
        /// <param name="sender">The name of the agent that sends the message</param>
        /// <param name="receiver">The name of the agent that needs to receive the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="conversationId">The conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        public Message(string sender, string receiver, string content, string conversationId)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Content = content;
            this.ConversationId = conversationId;
        }

        /// <summary>
        /// The content of the message.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic
        /// </summary>
        public string ConversationId { get; set; }

        /// <summary>
        /// The name of the agent that needs to receive the message
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// The name of the agent that sends the message
        /// </summary>
        public string Sender { get; set; }
    }
}