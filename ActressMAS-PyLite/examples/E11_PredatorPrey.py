"""
/**************************************************************************
 *                                                                        *
 *  Description: Example of using the ActressMas framework                *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2026, Florin Leon                                    *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/
"""

from ActressMas import *
import random
from enum import IntEnum


class Settings:
    GridSize = 50  # 20;
    NoTurns = 1000
    NoAnts = 1000  # 100
    NoDoodlebugs = 20  # 2
    ShowWorld = False
    Verbose = False
    WorldStateFileName = "world-state.txt"


class CellState(IntEnum):
    Empty = 0
    Ant = 1
    Doodlebug = 2


class Direction(IntEnum):
    Up = 0
    Down = 1
    Left = 2
    Right = 3


class Cell:
    def __init__(self):
        self.state = CellState.Empty
        self.agent_in_cell = None


class World:

    def __init__(self):
        self._map = [ [Cell() for _ in range(Settings.GridSize)] for _ in range(Settings.GridSize) ]
        self._current_id = 0


    def add_agent_to_map(self, insect, line, column):
        if isinstance(insect, AntAgent):
            self._map[line][column].state = CellState.Ant
        elif isinstance(insect, DoodlebugAgent):
            self._map[line][column].state = CellState.Doodlebug
        else:
            raise ValueError(f"Unknown agent type: {type(insect)}")

        insect.line = line
        insect.column = column
        self._map[line][column].agent_in_cell = insect


    def add_agent_to_map_vector(self, insect, vector_position):
        line = vector_position // Settings.GridSize
        column = vector_position % Settings.GridSize
        self.add_agent_to_map(insect, line, column)


    def create_name(self, insect):
        if isinstance(insect, AntAgent):
            name = f"a{self._current_id}"
        elif isinstance(insect, DoodlebugAgent):
            name = f"d{self._current_id}"
        else:
            raise ValueError(f"Unknown agent type: {type(insect)}")

        self._current_id += 1
        return name


    def count_insects(self):
        no_ants = 0
        no_doodlebugs = 0

        for i in range(Settings.GridSize):
            for j in range(Settings.GridSize):
                state = self._map[i][j].state
                if state == CellState.Doodlebug:
                    no_doodlebugs += 1
                elif state == CellState.Ant:
                    no_ants += 1

        return no_doodlebugs, no_ants


    def move(self, insect, new_line, new_column):
        # move the agent
        src_cell = self._map[insect.line][insect.column]
        dst_cell = self._map[new_line][new_column]

        dst_cell.state = src_cell.state
        dst_cell.agent_in_cell = src_cell.agent_in_cell

        src_cell.state = CellState.Empty
        src_cell.agent_in_cell = None

        # update agent position
        insect.line = new_line
        insect.column = new_column


    def breed(self, insect, new_line, new_column):
        if isinstance(insect, AntAgent):
            offspring = AntAgent()
        elif isinstance(insect, DoodlebugAgent):
            offspring = DoodlebugAgent()
        else:
            raise ValueError(f"Unknown agent type: {type(insect)}")

        name = self.create_name(offspring)
        offspring.name = name
        self.add_agent_to_map(offspring, new_line, new_column)

        if Settings.Verbose:
            print(f"Breeding {offspring.name}")

        return offspring


    def eat(self, doodlebug, new_line, new_column):
        ant = self._map[new_line][new_column].agent_in_cell

        if Settings.Verbose and ant is not None:
            print(f"Removing {ant.name}")

        # move the doodlebug
        if Settings.Verbose:
            print(f"Moving {doodlebug.name}")

        src_cell = self._map[doodlebug.line][doodlebug.column]
        dst_cell = self._map[new_line][new_column]

        dst_cell.state = CellState.Doodlebug
        dst_cell.agent_in_cell = src_cell.agent_in_cell

        src_cell.state = CellState.Empty
        src_cell.agent_in_cell = None

        # update doodlebug position
        doodlebug.line = new_line
        doodlebug.column = new_column

        return ant


    def die(self, doodlebug):
        cell = self._map[doodlebug.line][doodlebug.column]
        cell.state = CellState.Empty
        cell.agent_in_cell = None


    def valid_movement(self, insect, direction, desired_state):
        current_line = insect.line
        current_column = insect.column
        new_line = current_line
        new_column = current_column

        if direction == Direction.Up:
            if current_line == 0:
                return False, current_line, current_column
            if self._map[current_line - 1][current_column].state != desired_state:
                return False, current_line, current_column
            new_line = current_line - 1
            return True, new_line, new_column

        if direction == Direction.Down:
            if current_line == Settings.GridSize - 1:
                return False, current_line, current_column
            if self._map[current_line + 1][current_column].state != desired_state:
                return False, current_line, current_column
            new_line = current_line + 1
            return True, new_line, new_column

        if direction == Direction.Left:
            if current_column == 0:
                return False, current_line, current_column
            if self._map[current_line][current_column - 1].state != desired_state:
                return False, current_line, current_column
            new_column = current_column - 1
            return True, new_line, new_column

        if direction == Direction.Right:
            if current_column == Settings.GridSize - 1:
                return False, current_line, current_column
            if self._map[current_line][current_column + 1].state != desired_state:
                return False, current_line, current_column
            new_column = current_column + 1
            return True, new_line, new_column

        raise ValueError("Invalid direction")


    def print_map(self):
        lines = []
        for i in range(Settings.GridSize):
            row_chars = []
            for j in range(Settings.GridSize):
                state = self._map[i][j].state
                if state == CellState.Empty:
                    row_chars.append("-")
                elif state == CellState.Ant:
                    row_chars.append("a")
                elif state == CellState.Doodlebug:
                    row_chars.append("D")
                else:
                    row_chars.append("?")
            lines.append("".join(row_chars))
        return "\n".join(lines)


_insect_rand = random.Random()


class InsectAgent(Agent):

    def __init__(self):
        super().__init__()
        self._turns_survived = 0
        self._world = None
        self.line = 0
        self.column = 0
        self._rand = _insect_rand


    def _try_to_move(self):
        direction = Direction(self._rand.randrange(4))
        ok, new_line, new_column = self._world.valid_movement(self, direction, CellState.Empty)
        if ok:
            if Settings.Verbose:
                print(f"Moving {self.name}")
            self._world.move(self, new_line, new_column)
            return True
        return False


    def _try_to_breed(self):
        allowed_directions = []
        for i in range(4):
            direction = Direction(i)
            ok, new_line, new_column = self._world.valid_movement(self, direction, CellState.Empty)
            if ok:
                allowed_directions.append((direction, new_line, new_column))

        if not allowed_directions:
            return False

        _, new_line, new_column = self._rand.choice(allowed_directions)
        new_insect = self._world.breed(self, new_line, new_column)
        # The new insect must be added to the environment so that it participates in the simulation
        self.environment.add(new_insect, new_insect.name)
        return True


class AntAgent(InsectAgent):

    def setup(self):
        self._turns_survived = 0
        self._world = self.environment.memory["World"]

        if Settings.Verbose:
            print(f"AntAgent {self.name} started in ({self.line},{self.column})")


    def act_default(self):
        self._ant_action()


    def _ant_action(self):
        # Move: For every time step, the ants randomly try to move up, down, left, or right.
        # If the neighboring cell in the selected direction is occupied or would move the ant
        # off the grid, then the ant stays in the current cell.
        #
        # Breed: If an ant survives for three time steps, at the end of the time step
        # (i.e., after moving) the ant will breed. This is simulated by creating a new ant
        # in an adjacent (up, down, left, or right) cell that is empty. If there is no empty
        # cell available, no breeding occurs. Once an offspring is produced, an ant cannot
        # produce an offspring again until it has survived three more time steps.
        self._turns_survived += 1

        # move
        self._try_to_move()

        # breed
        if self._turns_survived >= 3:
            if self._try_to_breed():
                self._turns_survived = 0


class DoodlebugAgent(InsectAgent):

    def __init__(self):
        super().__init__()
        self._last_eaten = 0


    def setup(self):
        self._turns_survived = 0
        self._last_eaten = 0
        self._world = self.environment.memory["World"]

        if Settings.Verbose:
            print(f"DoodlebugAgent {self.name} started in ({self.line},{self.column})")


    def act_default(self):
        self._doodlebug_action()


    def _doodlebug_action(self):
        # Move: For every time step, the doodlebug will move to an adjacent cell
        # containing an ant and eat the ant. If there are no ants in adjoining cells,
        # the doodlebug moves according to the same rules as the ant (to an empty cell).
        # Breed: If a doodlebug survives for eight time steps, at the end of the time step
        # it will spawn off a new doodlebug in the same manner as the ant.
        # Starve: If a doodlebug has not eaten an ant within three time steps, at the end
        # of the third time step it will starve and die. The doodlebug should then be removed
        # from the grid of cells.
        self._turns_survived += 1
        self._last_eaten += 1

        # eat
        if not self._try_to_eat():
            # move as an ant would
            self._try_to_move()

        # breed
        if self._turns_survived >= 8:
            if self._try_to_breed():
                self._turns_survived = 0

        # starve
        if self._last_eaten >= 3:
            self._die()


    def _try_to_eat(self):
        allowed_directions = []
        for i in range(4):
            direction = Direction(i)
            ok, new_line, new_column = self._world.valid_movement(self, direction, CellState.Ant)
            if ok:
                allowed_directions.append((direction, new_line, new_column))

        if not allowed_directions:
            return False

        _, new_line, new_column = self._rand.choice(allowed_directions)

        ant = self._world.eat(self, new_line, new_column)
        if ant is not None:
            # The decision to remove the ant belongs to the predator / environment
            self.environment.remove(ant)

        self._last_eaten = 0
        return True


    def _die(self):
        # removing the doodlebug
        if Settings.Verbose:
            print(f"Removing {self.name}")

        self._world.die(self)
        self.stop()


class WorldEnvironment(EnvironmentMas):

    def __init__(self, no_turns):
        super().__init__(no_turns=no_turns, random_order=False)

        world_state_file_name = Settings.WorldStateFileName
        self._world_state_file = open(world_state_file_name, "w", encoding="utf-8")
        self._world_state_file.write("Doodlebugs\tAnts\n")
        self.memory["World"] = World()


    def turn_finished(self, turn):
        world = self.memory["World"]
        no_doodlebugs, no_ants = world.count_insects()

        print(f"Turn {turn + 1}: {no_doodlebugs} doodlebugs, {no_ants} ants")

        self._world_state_file.write(f"{no_doodlebugs}\t{no_ants}\n")
        self._world_state_file.flush()

        if Settings.ShowWorld:
            print(world.print_map())
            input("\nPress ENTER to continue ")


    def simulation_finished(self):
        print("\nSimulation finished.")
        self._world_state_file.close()


def _create_initial_population(env):
    world = env.memory["World"]

    # C#: int noCells = Settings.GridSize * Settings.GridSize;
    no_cells = Settings.GridSize * Settings.GridSize

    # C#: int[] randVect = RandomPermutation(noCells);
    positions = list(range(no_cells))
    random.shuffle(positions)  # Fisher–Yates via Python’s shuffle

    # First create doodlebugs
    for _ in range(Settings.NoDoodlebugs):
        if not positions:
            break
        doodlebug = DoodlebugAgent()
        pos = positions.pop()
        world.add_agent_to_map_vector(doodlebug, pos)
        name = world.create_name(doodlebug)   # unique name from World
        doodlebug.name = name
        env.add(doodlebug, name)

    # Then create ants
    for _ in range(Settings.NoAnts):
        if not positions:
            break
        ant = AntAgent()
        pos = positions.pop()
        world.add_agent_to_map_vector(ant, pos)
        name = world.create_name(ant)
        ant.name = name
        env.add(ant, name)


def main():
    env = WorldEnvironment(no_turns=Settings.NoTurns)
    _create_initial_population(env)
    env.start()


if __name__ == "__main__":
    main()
