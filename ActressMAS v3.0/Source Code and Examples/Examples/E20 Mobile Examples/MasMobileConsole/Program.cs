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
using System.IO;

namespace MasMobileConsole
{
    public static class Program
    {
        private static string _containerName;
        private static string _serverIP;
        private static string _serverPort;

        [STAThread]
        public static void Main()
        {
            ReadSettings("settings.txt");
            var container = new Container(_serverIP, Convert.ToInt32(_serverPort), _containerName);

            container.NewText += container_NewText;
            container.Start();

            Console.WriteLine($"Press ENTER to start MAS in container {container.Name}");
            Console.ReadLine();

            container.RunMas(new EnvironmentMas(), new MasSetup());
        }

        private static void container_NewText(object source, NewTextEventArgs e)
        {
            string newText = e.Text;

            if (!newText.StartsWith("Message received") && !newText.StartsWith("Sending message"))
                Console.WriteLine(newText);
        }

        private static void ReadSettings(string filename)
        {
            var sr = new StreamReader(filename);

            string[] t = sr.ReadLine().Split(); _serverIP = t[1];
            t = sr.ReadLine().Split(); _serverPort = t[1];
            t = sr.ReadLine().Split(); _containerName = t[1];

            sr.Close();
        }
    }
}