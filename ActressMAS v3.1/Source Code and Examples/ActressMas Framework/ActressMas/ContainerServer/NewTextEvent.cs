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

namespace ActressMas
{
    /// <summary>
    /// An event handler for a message from a server or a container.
    /// </summary>
    public delegate void NewTextEventHandler(object source, NewTextEventArgs e);

    /// <summary>
    /// The class that defines a message from a server or a container.
    /// </summary>
    public class NewTextEventArgs : EventArgs
    {
        private string _text;

        public NewTextEventArgs(string text)
        {
            _text = text;
        }

        /// <summary>
        /// The text of the message
        /// </summary>
        public string Text => _text;
    }
}