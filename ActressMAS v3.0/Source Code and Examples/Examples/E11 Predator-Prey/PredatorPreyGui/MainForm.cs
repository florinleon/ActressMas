/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Predator-prey simulation (ants and doodlebugs) using the *
 *               ActressMas framework                                     *
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
using System.Windows.Forms;

namespace PredatorPreyGui
{
    public partial class MainForm : Form
    {
        private World worldEnv = null;
        private static Random _rand = new Random();

        public MainForm()
        {
            InitializeComponent();
        }

        private void runMasButton_Click(object sender, EventArgs e)
        {
            worldEnv = new World(Settings.NoTurns); // derived from ActressMas.TurnBasedEnvironment
            worldEnv.SetControls(listBox1, richTextBox1);

            int noCells = Settings.GridSize * Settings.GridSize;

            int[] randVect = Settings.RandomPermutation(noCells);

            for (int i = 0; i < Settings.NoDoodlebugs; i++)
            {
                var a = new DoodlebugAgent();
                worldEnv.Add(a, worldEnv.CreateName(a)); // unique name
                worldEnv.AddAgentToMap(a, randVect[i]);
            }

            for (int i = Settings.NoDoodlebugs; i < Settings.NoDoodlebugs + Settings.NoAnts; i++)
            {
                var a = new AntAgent();
                worldEnv.Add(a, worldEnv.CreateName(a));
                worldEnv.AddAgentToMap(a, randVect[i]);
            }

            worldEnv.Start();

            richTextBox1.LoadFile(Settings.WorldStateFileName, RichTextBoxStreamType.PlainText);
        }
    }
}