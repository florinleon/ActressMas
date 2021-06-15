/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: The BDI architecture using the ActressMas framework      *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  Acknowledgement:                                                      *
 *  The idea of this example is inspired by this project:                 *
 *  https://github.com/gama-platform/gama/wiki/UsingBDI                   *
 *  The actual implementation is completely original.                     *
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
using System.Drawing;
using System.Windows.Forms;

namespace Bdi
{
    public partial class TerrainForm : Form
    {
        private TerrainAgent _ownerAgent;
        private Bitmap _doubleBufferImage;
        private int _size;

        public TerrainForm()
        {
            InitializeComponent();
        }

        public void SetOwner(TerrainAgent a)
        {
            _ownerAgent = a;
            _size = a.Environment.Memory["Size"];
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            DrawTerrain();
        }

        public void UpdateTerrainGUI()
        {
            DrawTerrain();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            DrawTerrain();
        }

        private void DrawTerrain()
        {
            try
            {
                int w = pictureBox.Width;
                int h = pictureBox.Height;

                if (_doubleBufferImage != null)
                {
                    _doubleBufferImage.Dispose();
                    GC.Collect(); // prevents memory leaks
                }

                _doubleBufferImage = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(_doubleBufferImage);
                g.Clear(Color.White);

                g.DrawRectangle(Pens.DarkGray, 0, 0, w - 1, h - 1);

                double cellSize = (double)w / (double)_size;

                for (int i = 1; i < _size; i++)
                    g.DrawLine(Pens.DarkGray, (int)(i * cellSize), 0, (int)(i * cellSize), h);

                if (_ownerAgent != null && _ownerAgent.States != null)
                {
                    for (int i = 0; i < _size; i++)
                    {
                        if (_ownerAgent.States[i] == TerrainState.Water)
                            g.FillRectangle(Brushes.Blue, (int)(i * cellSize + 4), h / 2 + 2, (int)(cellSize - 7), h / 2 - 4);
                        else if (_ownerAgent.States[i] == TerrainState.Fire)
                            g.FillRectangle(Brushes.OrangeRed, (int)(i * cellSize + 4), h / 2 + 2, (int)(cellSize - 7), h / 2 - 4);
                        if (_ownerAgent.States[i] == TerrainState.GettingWater)
                            g.FillRectangle(Brushes.LightBlue, (int)(i * cellSize + 4), h / 2 + 2, (int)(cellSize - 7), h / 2 - 4);

                        if (i == _ownerAgent.PatrolPosition)
                            g.FillEllipse(Brushes.Black, (int)(i * cellSize + 4), 4, (int)(cellSize - 7), h / 2 - 6);
                    }
                }

                Graphics pbg = pictureBox.CreateGraphics();
                pbg.DrawImage(_doubleBufferImage, 0, 0);
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}