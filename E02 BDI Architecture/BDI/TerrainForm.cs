/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: The BDI architecture using the ActressMas framework      *
 *  Copyright:   (c) 2018, Florin Leon					  *
 *									  *
 *  Acknowledgement:	                                                  *
 *  The idea of this example is inspired by this project:	          *
 *  https://github.com/gama-platform/gama				  *
 *  The actual implementation is completely original.			  *
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
        private TerrainAgent ownerAgent;
        private Bitmap doubleBufferImage;

        public TerrainForm()
        {
            InitializeComponent();
        }

        public void SetOwner(TerrainAgent a)
        {
            ownerAgent = a;
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

                if (doubleBufferImage != null)
                {
                    doubleBufferImage.Dispose();
                    GC.Collect(); // prevents memory leaks
                }

                doubleBufferImage = new Bitmap(w, h);
                Graphics g = Graphics.FromImage(doubleBufferImage);
                g.Clear(Color.White);

                g.DrawRectangle(Pens.DarkGray, 0, 0, w - 1, h - 1);

                double cellSize = (double)w / (double)Utils.Size;

                for (int i = 1; i < Utils.Size; i++)
                    g.DrawLine(Pens.DarkGray, (int)(i * cellSize), 0, (int)(i * cellSize), h);

                if (ownerAgent != null && ownerAgent.States != null)
                {
                    for (int i = 0; i < Utils.Size; i++)
                    {
                        if (ownerAgent.States[i] == TerrainState.Water)
                            g.FillRectangle(Brushes.Blue, (int)(i * cellSize + 4), h / 2 + 2, (int)(cellSize - 7), h / 2 - 4);
                        else if (ownerAgent.States[i] == TerrainState.Fire)
                            g.FillRectangle(Brushes.OrangeRed, (int)(i * cellSize + 4), h / 2 + 2, (int)(cellSize - 7), h / 2 - 4);
                        if (ownerAgent.States[i] == TerrainState.GettingWater)
                            g.FillRectangle(Brushes.LightBlue, (int)(i * cellSize + 4), h / 2 + 2, (int)(cellSize - 7), h / 2 - 4);

                        if (i == ownerAgent.PatrolPosition)
                            g.FillEllipse(Brushes.Black, (int)(i * cellSize + 4), 4, (int)(cellSize - 7), h / 2 - 6);
                    }
                }

                Graphics pbg = pictureBox.CreateGraphics();
                pbg.DrawImage(doubleBufferImage, 0, 0);
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}