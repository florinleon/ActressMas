/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Predator-prey simulation (ants and doodlebugs) using     *
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

using ActressMas;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PredatorPreyGui
{
    public enum CellState { Empty, Ant, Doodlebug };

    public enum Direction { Up, Down, Left, Right };

    public class Cell
    {
        public InsectAgent AgentInCell;
        public CellState State;

        public Cell()
        {
            State = CellState.Empty;
            AgentInCell = null;
        }
    }

    public class World : TurnBasedEnvironment
    {
        private int _currentId;
        private ListBox _listBox = null;
        private Cell[,] _map;
        private RichTextBox _richTextBox = null;
        private StreamWriter _sw;

        public World(int noTurns)
            : base(noTurns)
        {
            _map = new Cell[Utils.GridSize, Utils.GridSize];
            for (int i = 0; i < Utils.GridSize; i++)
                for (int j = 0; j < Utils.GridSize; j++)
                    _map[i, j] = new Cell();

            _currentId = 0;

            _sw = new StreamWriter(Utils.WorldStateFileName);
            _sw.WriteLine("Doodlebugs\tAnts");
        }

        public void AddAgentToMap(InsectAgent a, int line, int column)
        {
            if (a.GetType().Name == "AntAgent")
                _map[line, column].State = CellState.Ant;
            else if (a.GetType().Name == "DoodlebugAgent")
                _map[line, column].State = CellState.Doodlebug;

            a.Line = line; a.Column = column;
            _map[line, column].AgentInCell = a;
        }

        public void AddAgentToMap(InsectAgent a, int vectorPosition)
        {
            int line = vectorPosition / Utils.GridSize;
            int column = vectorPosition % Utils.GridSize;

            AddAgentToMap(a, line, column);
        }

        public void Breed(InsectAgent a, int newLine, int newColumn)
        {
            InsectAgent offspring = null;

            if (a.GetType().Name == "AntAgent")
                offspring = new AntAgent();
            else if (a.GetType().Name == "DoodlebugAgent")
                offspring = new DoodlebugAgent();

            Add(offspring, CreateName(offspring)); // Add is from ActressMas.Environment
            AddAgentToMap(offspring, newLine, newColumn);

            if (Utils.Verbose)
                Console.WriteLine("Breeding " + offspring.Name);
        }

        public void CountInsects(out int noDoodlebugs, out int noAnts)
        {
            noAnts = 0;
            noDoodlebugs = 0;

            for (int i = 0; i < Utils.GridSize; i++)
                for (int j = 0; j < Utils.GridSize; j++)
                {
                    if (_map[i, j].State == CellState.Doodlebug)
                        noDoodlebugs++;
                    else if (_map[i, j].State == CellState.Ant)
                        noAnts++;
                }
        }

        public string CreateName(InsectAgent a)
        {
            if (a.GetType().Name == "AntAgent")
                return string.Format("a{0}", _currentId++);
            else if (a.GetType().Name == "DoodlebugAgent")
                return string.Format("d{0}", _currentId++);

            throw new Exception("Unknown agent type: " + a.GetType().ToString());
        }

        public void Die(DoodlebugAgent da)
        {
            this.Remove(da); // from ActressMas.Environment

            _map[da.Line, da.Column].State = CellState.Empty;
            _map[da.Line, da.Column].AgentInCell = null;
        }

        public void Eat(DoodlebugAgent da, int newLine, int newColumn)
        {
            // removing the ant

            AntAgent ant = (AntAgent)_map[newLine, newColumn].AgentInCell;
            this.Remove(ant); // from ActressMas.Environment

            if (Utils.Verbose)
                Console.WriteLine("Removing " + ant.Name);

            // moving the doodlebug

            if (Utils.Verbose)
                Console.WriteLine("Moving " + da.Name);

            _map[newLine, newColumn].State = CellState.Doodlebug;
            _map[newLine, newColumn].AgentInCell = _map[da.Line, da.Column].AgentInCell;

            _map[da.Line, da.Column].State = CellState.Empty;
            _map[da.Line, da.Column].AgentInCell = null;

            // updating doodlebug position

            da.Line = newLine;
            da.Column = newColumn;
        }

        public void Move(InsectAgent a, int newLine, int newColumn)
        {
            // moving the agent

            _map[newLine, newColumn].State = _map[a.Line, a.Column].State;
            _map[newLine, newColumn].AgentInCell = _map[a.Line, a.Column].AgentInCell;

            _map[a.Line, a.Column].State = CellState.Empty;
            _map[a.Line, a.Column].AgentInCell = null;

            // updating agent position

            a.Line = newLine;
            a.Column = newColumn;
        }

        public string PrintMap()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Utils.GridSize; i++)
            {
                for (int j = 0; j < Utils.GridSize; j++)
                {
                    switch (_map[i, j].State)
                    {
                        case CellState.Empty:
                            sb.Append("-");
                            break;

                        case CellState.Ant:
                            sb.Append("a");
                            break;

                        case CellState.Doodlebug:
                            sb.Append("D");
                            break;

                        default:
                            break;
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public void SetControls(ListBox listBox, RichTextBox richTextBox)
        {
            _listBox = listBox;
            _richTextBox = richTextBox;
        }

        public override void SimulationFinished()
        {
            Console.WriteLine("\r\nSimulation finished.");
            _sw.Close();
        }

        public override void TurnFinished(int turn)
        {
            int noDoodlebugs, noAnts;
            CountInsects(out noDoodlebugs, out noAnts);

            _sw.WriteLine("{0}\t{1}", noDoodlebugs, noAnts);
            _sw.Flush();

            if (_listBox != null)
            {
                if (NoAgents == 0)
                    return;

                _listBox.BeginUpdate();
                _listBox.Items.Clear();
                foreach (string a in AllAgents())
                    _listBox.Items.Add(a);
                _listBox.EndUpdate();

                _richTextBox.Clear();
                _richTextBox.AppendText(string.Format("Turn {0} / {1}", turn, Utils.NoTurns));

                Application.DoEvents();
                Thread.Sleep(100);
            }
        }

        public bool ValidMovement(InsectAgent a, Direction direction, CellState desiredState, out int newLine, out int newColumn)
        {
            int currentLine = a.Line; int currentColumn = a.Column;
            newLine = currentLine; newColumn = currentColumn;

            switch (direction)
            {
                case Direction.Up:
                    if (currentLine == 0) return false;
                    if (_map[currentLine - 1, currentColumn].State != desiredState) return false;
                    newLine = currentLine - 1;
                    return true;

                case Direction.Down:
                    if (currentLine == Utils.GridSize - 1) return false;
                    if (_map[currentLine + 1, currentColumn].State != desiredState) return false;
                    newLine = currentLine + 1;
                    return true;

                case Direction.Left:
                    if (currentColumn == 0) return false;
                    if (_map[currentLine, currentColumn - 1].State != desiredState) return false;
                    newColumn = currentColumn - 1;
                    return true;

                case Direction.Right:
                    if (currentColumn == Utils.GridSize - 1) return false;
                    if (_map[currentLine, currentColumn + 1].State != desiredState) return false;
                    newColumn = currentColumn + 1;
                    return true;

                default:
                    break;
            }

            throw new Exception("Invalid direction");
        }
    }
}