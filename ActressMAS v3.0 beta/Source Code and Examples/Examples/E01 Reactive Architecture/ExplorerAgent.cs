/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: The reactive architecture using the ActressMas framework *
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
using System.Collections.Generic;

namespace Reactive
{
    public class ExplorerAgent : Agent
    {
        private int _x, _y;
        private State _state;
        private string _resourceCarried;
        private int _size;
        private static Random _rand = new Random();

        private enum State { Free, Carrying };

        public override void Setup()
        {
            Console.WriteLine($"Starting {Name}");

            _size = Environment.Memory["Size"];

            _x = _size / 2;
            _y = _size / 2;
            _state = State.Free;

            while (IsAtBase())
            {
                _x = _rand.Next(_size);
                _y = _rand.Next(_size);
            }

            Send("planet", $"position {_x} {_y}");
        }

        private bool IsAtBase()
        {
            return (_x == _size / 2 && _y == _size / 2); // the position of the base
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                message.Parse(out string action, out List<string> parameters);

                if (action == "block")
                {
                    // R1. If you detect an obstacle, then change direction
                    MoveRandomly();
                    Send("planet", $"change {_x} {_y}");
                }
                else if (action == "move" && _state == State.Carrying && IsAtBase())
                {
                    // R2. If carrying samples and at the base, then unload samples
                    _state = State.Free;
                    Send("planet", $"unload {_resourceCarried}");
                }
                else if (action == "move" && _state == State.Carrying && !IsAtBase())
                {
                    // R3. If carrying samples and not at the base, then travel up gradient
                    MoveToBase();
                    Send("planet", $"carry {_x} {_y}");
                }
                else if (action == "rock")
                {
                    // R4. If you detect a sample, then pick sample up
                    _state = State.Carrying;
                    _resourceCarried = parameters[0];
                    Send("planet", $"pick-up {_resourceCarried}");
                }
                else if (action == "move")
                {
                    // R5. If (true), then move randomly
                    MoveRandomly();
                    Send("planet", $"change {_x} {_y}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.ToString()); // for debugging
            }
        }

        private void MoveRandomly()
        {
            int d = _rand.Next(4);
            switch (d)
            {
                case 0: if (_x > 0) _x--; break;
                case 1: if (_x < _size - 1) _x++; break;
                case 2: if (_y > 0) _y--; break;
                case 3: if (_y < _size - 1) _y++; break;
            }
        }

        private void MoveToBase()
        {
            int dx = _x - _size / 2;
            int dy = _y - _size / 2;

            if (Math.Abs(dx) > Math.Abs(dy))
                _x -= Math.Sign(dx);
            else
                _y -= Math.Sign(dy);
        }
    }
}