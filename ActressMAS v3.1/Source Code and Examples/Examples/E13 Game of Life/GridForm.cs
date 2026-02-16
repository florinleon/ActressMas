/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Game of Life using the ActressMas framework              *
 *  Copyright:   (c) 2020, Florin Leon                                    *
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

namespace GameOfLife
{
    public partial class GridForm : Form
    {
        private GridAgent _ownerAgent;
        private Bitmap _doubleBufferImage;
        private int _size;

        public GridForm()
        {
            InitializeComponent();
        }

        public void SetOwner(GridAgent a)
        {
            _ownerAgent = a;
            _size = a.Environment.Memory["Size"];
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            DrawGrid();
        }

        public void UpdatePlanetGUI()
        {
            DrawGrid();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            DrawGrid();
        }

        private void DrawGrid()
        {
            int w = pictureBox.Width;
            int h = pictureBox.Height;

            if (_ownerAgent == null || w == 0 && h == 0)
                return;

            if (_doubleBufferImage != null)
            {
                _doubleBufferImage.Dispose();
                GC.Collect(); // prevents memory leaks
            }

            _doubleBufferImage = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(_doubleBufferImage);
            g.Clear(Color.White);

            int minXY = Math.Min(w, h);
            int cellSize = (minXY - 40) / _size;

            for (int i = 0; i <= _size; i++)
            {
                g.DrawLine(Pens.DarkGray, 20, 20 + i * cellSize, 20 + _size * cellSize, 20 + i * cellSize);
                g.DrawLine(Pens.DarkGray, 20 + i * cellSize, 20, 20 + i * cellSize, 20 + _size * cellSize);
            }

            if (_ownerAgent != null)
            {
                for (int i = 0; i < _size; i++)
                    for (int j = 0; j < _size; j++)
                    {
                        if (_ownerAgent.CellStates[i, j])
                            g.FillEllipse(Brushes.Red, 20 + i * cellSize + 6, 20 + j * cellSize + 6, cellSize - 12, cellSize - 12);
                    }
            }

            Graphics pbg = pictureBox.CreateGraphics();
            pbg.DrawImage(_doubleBufferImage, 0, 0);
        }
    }
}