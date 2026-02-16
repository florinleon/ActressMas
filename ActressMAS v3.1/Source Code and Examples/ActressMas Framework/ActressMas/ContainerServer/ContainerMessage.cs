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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ActressMas
{
    /// <summary>
    /// A message that the containers use to communicate.
    /// </summary>
    [Serializable]
    internal class ContainerMessage
    {
        /// <summary>
        /// Initializes a new instance of the ContainerMessage class.
        /// </summary>
        /// <param name="sender">The name of the container that sends the message</param>
        /// <param name="receiver">The name of the container that needs to receive the message</param>
        /// <param name="info">The high-level subject of the message</param>
        /// <param name="content">The low-level content of the message, i.e. additional information</param>
        public ContainerMessage(string sender, string receiver, string info, string content)
        {
            Sender = sender;
            Receiver = receiver;
            Info = info;
            Content = content;
        }

        public string Content { get; set; }
        public string Info { get; set; }
        public string Receiver { get; set; }
        public string Sender { get; set; }

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        public static object Deserialize(string s)
        {
            byte[] bytes = Convert.FromBase64String(s);
            var stream = new MemoryStream(bytes);
            var bf = new BinaryFormatter();
            return bf.Deserialize(stream);
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        public static string Serialize(object o)
        {
            if (!o.GetType().IsSerializable)
                throw new Exception($"Object {o.GetType().FullName} is not serializable");

            var stream = new MemoryStream();
            var bf = new BinaryFormatter();
            bf.Serialize(stream, o);
            return Convert.ToBase64String(stream.ToArray());
        }

        /// <summary>
        /// Converts the object to a string using the Serialize method.
        /// </summary>
        public override string ToString() => 
            Serialize(this);

        /// <summary>
        /// Returns a string of the form "{Sender} -> {Receiver}: {Info} # {Content}"
        /// </summary>
        public string Format()
        {
            string mc = Content;
            if (mc.Length > 20)
                mc = mc.Substring(0, 20) + "...";
            return $"{Sender} -> {Receiver}: {Info} # {mc}";
        }
    }
}