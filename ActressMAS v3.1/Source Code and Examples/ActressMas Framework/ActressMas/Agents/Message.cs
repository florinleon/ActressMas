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

using System;
using System.Collections.Generic;
using System.Text;

namespace ActressMas
{
    /// <summary>
    /// A message that the agents use to communicate. In an agent-based system, the communication between the agents is exclusively performed by exchanging messages.
    /// </summary>
    [Serializable]
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
            Sender = sender;
            Receiver = receiver;
            Content = content;
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
            Sender = sender;
            Receiver = receiver;
            Content = content;
            ConversationId = conversationId;
        }

        /// <summary>
        /// Initializes a new instance of the Message class.
        /// </summary>
        /// <param name="sender">The name of the agent that sends the message</param>
        /// <param name="receiver">The name of the agent that needs to receive the message</param>
        /// <param name="contentObj">The content of the message</param>
        public Message(string sender, string receiver, dynamic contentObj)
        {
            Sender = sender;
            Receiver = receiver;
            ContentObj = contentObj;
        }

        /// <summary>
        /// Initializes a new instance of the Message class.
        /// </summary>
        /// <param name="sender">The name of the agent that sends the message</param>
        /// <param name="receiver">The name of the agent that needs to receive the message</param>
        /// <param name="contentObj">The content of the message</param>
        /// <param name="conversationId">The conversation identifier, for the cases when a conversation involves multiple messages that refer to the same topic</param>
        public Message(string sender, string receiver, dynamic contentObj, string conversationId)
        {
            Sender = sender;
            Receiver = receiver;
            ContentObj = contentObj;
            ConversationId = conversationId;
        }

        /// <summary>
        /// The content of the message (a string).
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The content of the message (an object).
        /// </summary>
        public dynamic ContentObj { get; set; }

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

        /// <summary>
        /// Returns a string of the form "[Sender -> Receiver]: Content"
        /// </summary>
        public string Format() =>
            $"[{Sender} -> {Receiver}]: {Content}";

        /// <summary>
        /// Parses the content of a message and identifies the action (similar, e.g., to a performative) and the list of parameters.
        /// </summary>
        public void Parse(out string action, out List<string> parameters)
        {
            string[] t = Content.Split();

            action = t[0];

            parameters = new List<string>();
            for (int i = 1; i < t.Length; i++)
                parameters.Add(t[i]);
        }

        /// <summary>
        /// Parses the content of a message and identifies the action (similar, e.g., to a performative) and the parameters concatenated in a string.
        /// </summary>
        public void Parse(out string action, out string parameters)
        {
            string[] t = Content.Split();

            action = t[0];

            StringBuilder sb = new StringBuilder();

            if (t.Length > 1)
            {
                for (int i = 1; i < t.Length - 1; i++)
                    sb.Append($"{t[i]} ");
                sb.Append(t[t.Length - 1]);
            }

            parameters = sb.ToString();
        }

        /// <summary>
        /// Parses the content of a message and identifies the action (similar, e.g., to a performative) and the single parameter.
        /// </summary>
        public void Parse1P(out string action, out string parameter)
        {
            string[] t = Content.Split();
            action = t[0];
            parameter = t[1];
        }
    }
}