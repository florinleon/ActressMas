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

namespace RemoteMessages
{
    public partial class MasMobileForm : Form
    {
        private Container _container;
        private string _containerName;
        private EnvironmentMas _environment;
        private string _serverIP;
        private string _serverPort;

        public MasMobileForm()
        {
            InitializeComponent();

            listBoxAgents.Sorted = true;
            comboBoxName.SelectedIndex = 0;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (_container != null)
                _container.Stop();
            System.Environment.Exit(0);
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            buttonConnect.Enabled = true;
            buttonDisconnect.Enabled = false;
            buttonRunMas.Enabled = false;

            if (_container != null)
                _container.Stop();
        }

        private void buttonRunMas_Click(object sender, EventArgs e)
        {
            _environment = new EnvironmentMas();
            _container.RunMas(_environment, new MasSetup());
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            try
            {
                _containerName = comboBoxName.Text;
                _serverIP = textBoxServerIP.Text;
                _serverPort = textBoxServerPort.Text;

                buttonConnect.Enabled = false;
                buttonDisconnect.Enabled = true;
                buttonRunMas.Enabled = true;

                _container = new Container(_serverIP, Convert.ToInt32(_serverPort), _containerName);

                Global.Rtb = richTextBoxLog; // agents will write to this

                _container.NewText += container_NewText;
                _container.Start();
            }
            catch (Exception ex)
            {
                richTextBoxLog.AppendText($"Exception: {ex.Message}.\r\n");
                richTextBoxLog.Focus(); Application.DoEvents();
            }
        }

        private void container_NewText(object source, NewTextEventArgs e)
        {
            string newText = e.Text;

            if (newText.StartsWith("Exception"))
            {
                buttonConnect.Enabled = true;
                buttonDisconnect.Enabled = false;
                buttonRunMas.Enabled = false;
            }

            if (!newText.StartsWith("Message received") && !newText.StartsWith("Sending message"))
            {
                richTextBoxLog.AppendText($"{e.Text}\r\n");
                richTextBoxLog.Focus(); Application.DoEvents();
            }
        }

        private void ContainerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_container != null)
                _container.Stop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_container == null || _environment == null || _environment.NoAgents == 0)
                return;

            try
            {
                listBoxAgents.Items.Clear();
                foreach (string a in _environment.AllAgents())
                    listBoxAgents.Items.Add(a);
            }
            catch (Exception ex)
            {
                richTextBoxLog.AppendText($"Exception: {ex.Message}.\r\n");
                richTextBoxLog.Focus(); Application.DoEvents();
            }
        }
    }
}