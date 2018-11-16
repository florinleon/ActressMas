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

using System;
using System.Collections.Generic;
using System.Text;
using ActressMas;

namespace PredatorPrey
{
    public enum CellState { Empty, Ant, Doodlebug };

    public enum Direction { Up, Down, Left, Right };

    public class Cell
    {
        public CellState State;
        public InsectAgent AgentInCell;

        public Cell()
        {
            State = CellState.Empty;
            AgentInCell = null;
        }
    }

    public class World : ConcurrentEnvironment
    {
        private Cell[,] _map;
        private int _currentId;
        private List<string> _activeDoodlebugs, _activeAnts;

        public void InitWorldMap()
        {
            // not static constructor; this method can be used if one needs to change the grid size at runtime

            _map = new Cell[Utils.GridSize, Utils.GridSize];
            for (int i = 0; i < Utils.GridSize; i++)
                for (int j = 0; j < Utils.GridSize; j++)
                    _map[i, j] = new Cell();

            _currentId = 0;
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

        public string CreateName(InsectAgent a)
        {
            if (a.GetType().Name == "AntAgent")
                return string.Format("a{0}", _currentId++);
            else if (a.GetType().Name == "DoodlebugAgent")
                return string.Format("d{0}", _currentId++);

            throw new Exception("Unknown agent type: " + a.GetType().ToString());
        }

        public void StartNewTurn()
        {
            _activeDoodlebugs = new List<string>();
            _activeAnts = new List<string>();

            for (int i = 0; i < Utils.GridSize; i++)
                for (int j = 0; j < Utils.GridSize; j++)
                {
                    if (_map[i, j].State == CellState.Doodlebug)
                        _activeDoodlebugs.Add(_map[i, j].AgentInCell.Name);
                    else if (_map[i, j].State == CellState.Ant)
                        _activeAnts.Add(_map[i, j].AgentInCell.Name);
                }
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

        public string GetNextInsect()
        {
            if (_activeDoodlebugs.Count != 0)
            {
                int r = Utils.RandNoGen.Next(_activeDoodlebugs.Count);
                string name = _activeDoodlebugs[r];
                _activeDoodlebugs.Remove(name);
                return name;
            }
            else if (_activeAnts.Count != 0)
            {
                int r = Utils.RandNoGen.Next(_activeAnts.Count);
                string name = _activeAnts[r];
                _activeAnts.Remove(name);
                return name;
            }
            else
                return "";
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

        public void Breed(InsectAgent a, int newLine, int newColumn)
        {
            InsectAgent offspring = null;

            if (a.GetType().Name == "AntAgent")
                offspring = new AntAgent();
            else if (a.GetType().Name == "DoodlebugAgent")
                offspring = new DoodlebugAgent();

            Add(offspring, CreateName(offspring)); // Add is from ActressMas.Environment
            AddAgentToMap(offspring, newLine, newColumn);

            if (a.GetType().Name == "AntAgent")
                _activeAnts.Add(offspring.Name);
            else if (a.GetType().Name == "DoodlebugAgent")
                _activeDoodlebugs.Add(offspring.Name);

            if (Utils.Verbose)
                Console.WriteLine("Breeding " + offspring.Name);

            offspring.Start();
        }

        public void Eat(DoodlebugAgent da, int newLine, int newColumn)
        {
            // removing the ant

            AntAgent ant = (AntAgent)_map[newLine, newColumn].AgentInCell;
            _activeAnts.Remove(ant.Name);
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

        public void Die(DoodlebugAgent da)
        {
            _activeDoodlebugs.Remove(da.Name);
            this.Remove(da); // from ActressMas.Environment

            _map[da.Line, da.Column].State = CellState.Empty;
            _map[da.Line, da.Column].AgentInCell = null;
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
    }
}