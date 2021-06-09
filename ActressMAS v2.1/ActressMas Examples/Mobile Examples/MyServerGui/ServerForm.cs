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
using System.Windows.Forms;

namespace MyServerGui
{
    public partial class ServerForm : Form
    {
        private Server server;

        public ServerForm()
        {
            InitializeComponent();

            listBoxContainers.Sorted = true;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            server = new Server(Convert.ToInt32(textBoxPort.Text), Convert.ToInt32(textBoxPing.Text));
            server.NewText += server_NewText;
            server.Start();

            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            server.Stop();
            server.NewText -= server_NewText;

            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
        }

        private void server_NewText(object source, NewTextEventArgs e)
        {
            string newText = e.Text;

            if (!newText.StartsWith("Message received") && !newText.StartsWith("Sending message"))
            {
                richTextBoxLog.AppendText(newText + "\r\n");
                richTextBoxLog.Focus();
                Application.DoEvents();

                if (newText.StartsWith("Containers:"))
                {
                    listBoxContainers.BeginUpdate();
                    listBoxContainers.Items.Clear();
                    string[] toks = newText.Split("\t\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 1; i < toks.Length; i++)
                        listBoxContainers.Items.Add(toks[i]);
                    listBoxContainers.EndUpdate();
                }
            }
        }

        private void ServerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (server != null)
                server.Stop();
        }
    }
}