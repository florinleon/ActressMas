/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Developing mobile agents using the ActressMas framework  *
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

using ActressMas;
using System;

namespace MyServerConsole
{
    public static class Program
    {
        private static void Main()
        {
            var server = new Server(5000, 3000);
            Console.WriteLine("Server listening on port 5000.");
            server.NewText += server_NewText;
            server.Start();
            Console.WriteLine("Press ENTER to close the server.");
            Console.ReadLine();
            server.Stop();
        }

        private static void server_NewText(object source, NewTextEventArgs e)
        {
            if (e.Text.StartsWith("Containers:"))
            {
                Console.Clear();
                Console.WriteLine(e.Text);
            }
        }
    }
}