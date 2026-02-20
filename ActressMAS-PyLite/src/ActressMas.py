"""
 **************************************************************************
 *                                                                        *
 *  Description: ActressMas PyLite multi-agent framework                  *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Copyright:   (c) 2018-2026, Florin Leon                               *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************
"""

from collections import deque
import random
import time


class Info:
    """Information about ActressMas version."""

    # ActressMas PyLite current version
    VERSION = "ActressMas PyLite Version 3.1"


class Message:
    """A message that the agents use to communicate.

    In an ActressMas system, communication between the agents is
    exclusively performed by exchanging messages.
    """

    def __init__(self, sender=None, receiver=None, content=None, content_obj=None, conversation_id=""):
        """Initializes a new instance of the Message class.

        Parameters
        ----------
        sender:
            The name of the agent that sends the message.
        receiver:
            The name of the agent that needs to receive the message.
        content:
            The content of the message (string).
        content_obj:
            The content of the message (object).
        conversation_id:
            The conversation identifier, for the cases when a conversation
            involves multiple messages that refer to the same topic.
        """
        self.sender = sender
        self.receiver = receiver
        self.content = content
        self.content_obj = content_obj
        self.conversation_id = conversation_id


    def format(self):
        """Returns a string of the form "[Sender -> Receiver]: Content"."""
        return f"[{self.sender} -> {self.receiver}]: {self.content}"


    def parse(self):
        """Parses the content and returns ``(action, parameters_list)``."""
        if self.content is None:
            raise ValueError("Cannot parse message with empty content.")

        tokens = self.content.split()
        if not tokens:
            raise ValueError("Cannot parse empty content string.")

        action = tokens[0]
        parameters = list(tokens[1:])
        return action, parameters


    def parse_to_string(self):
        """Parses the content and returns ``(action, parameters_string)``."""
        if self.content is None:
            raise ValueError("Cannot parse message with empty content.")

        tokens = self.content.split()
        if not tokens:
            raise ValueError("Cannot parse empty content string.")

        action = tokens[0]
        if len(tokens) > 1:
            parameters = " ".join(tokens[1:])
        else:
            parameters = ""

        return action, parameters


    def parse_1p(self):
        """Parses the content and returns ``(action, single_parameter)``."""
        if self.content is None:
            raise ValueError("Cannot parse message with empty content.")

        tokens = self.content.split()
        if len(tokens) < 2:
            raise ValueError("Content must contain at least an action and one parameter.")

        action = tokens[0]
        parameter = tokens[1]
        return action, parameter


class Agent:
    """The base class for an agent that runs on a turn-based multiagent
    environment.

    You must create your own agent classes derived from this abstract class.
    """
    
    def __init__(self):
        # internal flag: whether Setup must be run before the next turn
        self.must_run_setup = True
        # queue of pending messages
        self._messages = deque()
        # set by the environment
        self.environment = None
        # unique agent name
        self.name = None

    # ------------------------------------------------------------------
    # Hooks to be overridden by subclasses
    # ------------------------------------------------------------------

    def act(self, message):
        """Called when the agent receives a message in a turn.

        This is where the main logic of the agent should be placed.
        """
        # To be overridden in subclasses
        return None


    def act_default(self):
        """Called when the agent does not receive any messages in a turn."""
        # To be overridden in subclasses
        return None


    def setup(self):
        """Called on the first turn (or right after the agent has been added).

        Similar to a constructor, but intended only for agent-related logic,
        e.g. for sending initial messages.
        """
        # To be overridden in subclasses
        return None

    # ------------------------------------------------------------------
    # Message sending utilities
    # ------------------------------------------------------------------

    def broadcast(self, content, include_sender = False, conversation_id = ""):
        """Sends a string message to all agents in the environment."""
        if self.environment is None:
            raise RuntimeError("Agent has no environment associated.")

        receivers = list(self.environment.all_agents())

        if not include_sender and self.name in receivers:
            receivers.remove(self.name)

        for receiver in receivers:
            self.send(receiver, content, conversation_id=conversation_id)


    def broadcast_obj(self, content_obj, include_sender = False, conversation_id = ""):
        """Sends an object message to all agents in the environment."""
        if self.environment is None:
            raise RuntimeError("Agent has no environment associated.")

        receivers = list(self.environment.all_agents())

        if not include_sender and self.name in receivers:
            receivers.remove(self.name)

        for receiver in receivers:
            self.send_obj(receiver, content_obj, conversation_id=conversation_id)


    def send(self, receiver, content, conversation_id = ""):
        """Sends a string message to a specific agent, identified by name."""
        if "@" in receiver:
            raise ValueError(
                "Sending objects with Message.content is not supported. "
                "Serialize the object and send it as a string."
            )

        if self.environment is None:
            raise RuntimeError("Agent has no environment associated.")

        message = Message(self.name, receiver, content, None, conversation_id)
        self.environment.send(message)


    def send_obj(self, receiver, content_obj, conversation_id = ""):
        """Sends an object message to a specific agent, identified by name."""
        if "@" in receiver:
            raise ValueError(
                "Sending objects with Message.content_obj is not supported. "
                "Serialize the object and send it as a string."
            )

        if self.environment is None:
            raise RuntimeError("Agent has no environment associated.")

        message = Message(self.name, receiver, None, content_obj, conversation_id)
        self.environment.send(message)


    def send_to_many(self, receivers, content, conversation_id = ""):
        """Sends a string message to a specific set of agents."""
        for r in receivers:
            self.send(r, content, conversation_id=conversation_id)


    def send_obj_to_many(self, receivers, content_obj, conversation_id = ""):
        """Sends an object message to a specific set of agents."""
        for r in receivers:
            self.send_obj(r, content_obj, conversation_id=conversation_id)


    def stop(self):
        """Stops the execution of the agent and removes it from the environment.

        Use this method instead of calling ``environment.remove`` directly
        when the decision to stop belongs to the agent itself.
        """
        if self.environment is None:
            raise RuntimeError("Agent has no environment associated.")

        self.environment.remove(self)

    # ------------------------------------------------------------------
    # Internal methods used by the environment
    # ------------------------------------------------------------------

    def _internal_act(self):
        """Executes pending messages or the default behavior for this turn."""
        if self._messages:
            while self._messages:
                msg = self._messages.popleft()
                self.act(msg)
        else:
            self.act_default()


    def _post(self, message):
        """Queues an incoming message for processing at the next turn."""
        self._messages.append(message)


class EnvironmentMas:
    """Multiagent environment where all agents are executed."""

    def __init__(self, no_turns = 0, delay_after_turn_ms = 0, random_order = True, rand = None):
        """Initializes a new instance of the EnvironmentMas class.

        Parameters
        ----------
        no_turns:
            Maximum number of turns of the simulation. If 0, the simulation
            runs indefinitely, or until there are no more agents.
        delay_after_turn_ms:
            Delay (in milliseconds) after each turn.
        random_order:
            Whether the agents should be executed in a random order.
        rand:
            Random number generator for random ordering. If ``None``, a new
            ``random.Random`` instance is created.
        """
        self._no_turns = no_turns
        self._delay_after_turn_ms = delay_after_turn_ms
        self._random_order = random_order
        self._rand = rand if rand is not None else random.Random()

        # The agents in the environment (name -> Agent)
        self._agents = {}

        # An object that can be used as a shared memory by the agents.
        self.memory = {}


    # Number of agents in the environment
    @property
    def num_agents(self):
        return len(self._agents)

    # ------------------------------------------------------------------
    # Agent management
    # ------------------------------------------------------------------

    def add(self, agent, name = None):
        """Adds an agent to the environment.

        The agent must have a unique name.
        """
        agent_name = name if name is not None else agent.name
        if not agent_name:
            raise ValueError("Trying to add an agent without a name (EnvironmentMas.add)")

        if agent_name in self._agents:
            raise ValueError(
                f"Trying to add an agent with an existing name: {agent_name} "
                "(EnvironmentMas.add)"
            )

        agent.name = agent_name
        agent.environment = self
        self._agents[agent_name] = agent


    def all_agents(self):
        """Returns a list with the names of all the agents."""
        return list(self._agents.keys())


    def filtered_agents(self, name_fragment):
        """Returns the names of all agents that contain a certain string."""
        return [name for name in self._agents if name_fragment in name]


    def random_agent(self, rand = None):
        """Returns the name of a randomly selected agent from the environment.

        If ``rand`` is provided, it is used as the random number generator.
        Otherwise, the environment's own random number generator is used.
        This is useful for experiments involving non-determinism that should
        still be repeatable for analysis and debugging.
        """
        if not self._agents:
            raise RuntimeError("No agents in environment.")

        r = rand if rand is not None else self._rand
        index = r.randrange(len(self._agents))
        # dict preserves insertion order; we just need to index into keys
        names = list(self._agents.keys())
        return names[index]


    def remove(self, agent_or_name):
        """Stops the execution of an agent and removes it from the environment.

        This method should be used when the decision to stop the agent does
        not belong to the agent itself (for example, when another agent or
        some external factor decides to terminate it).

        The argument can be either an ``Agent`` instance or an agent name.
        """
        if isinstance(agent_or_name, Agent):
            agent = agent_or_name
            # find by identity
            name = agent.name
        else:
            name = str(agent_or_name)
            agent = self._agents.get(name)

        if name is None or agent is None or name not in self._agents:
            raise ValueError(f"Agent {name} does not exist (EnvironmentMas.remove)")

        del self._agents[name]

    # ------------------------------------------------------------------
    # Simulation control
    # ------------------------------------------------------------------

    def start(self):
        """Starts the simulation."""
        turn = 0
        while True:
            self._run_turn(turn)
            turn += 1

            if self._no_turns != 0 and turn >= self._no_turns:
                break
            if not self._agents:
                break

        self.simulation_finished()


    def continue_simulation(self, no_turns = 0):
        """Continues the simulation for an additional number of turns.

        The simulation may stop earlier if there are no more agents.
        If ``no_turns`` is 0, the simulation runs indefinitely, or until there
        are no more agents.
        """
        turn = 0
        while True:
            self._run_turn(turn)
            turn += 1

            if no_turns != 0 and turn >= no_turns:
                break
            if not self._agents:
                break

        self.simulation_finished()


    def simulation_finished(self):
        """Hook that may be optionally overridden to perform additional
        processing after the simulation has finished."""
        return None


    def turn_finished(self, turn):
        """Hook that may be optionally overridden to perform additional
        processing after a turn of the simulation has finished.

        Parameters
        ----------
        turn:
            The index of the turn that has just finished.
        """
        return None

    # ------------------------------------------------------------------
    # Message delivery
    # ------------------------------------------------------------------

    def send(self, message):
        """Sends a message from the outside of the multiagent system.

        When possible, agents should use their own ``send``/``broadcast`` methods
        rather than calling this directly. This method can also be used to
        simulate a forwarding behavior.
        """
        receiver_name = message.receiver
        if receiver_name in self._agents:
            self._agents[receiver_name]._post(message)

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _execute_act(self, agent):
        agent._internal_act()


    def _execute_setup(self, agent):
        agent.setup()
        agent.must_run_setup = False


    def _random_permutation(self, n):
        """Returns a random permutation of ``range(n)`` using the Fisher–Yates
        shuffle.

        This is an O(n) algorithm that uses the environment's random number
        generator and is more efficient than generating and sorting indices.
        """
        numbers = list(range(n))
        i = n
        while i > 1:
            k = self._rand.randrange(i)
            i -= 1
            numbers[i], numbers[k] = numbers[k], numbers[i]
        return numbers


    def _sorted_permutation(self, n):
        """Returns the identity permutation 0..n-1."""
        return list(range(n))


    def _run_turn(self, turn):
       """Runs a single turn of the simulation."""
       if not self._agents:
           return

       agent_names = list(self._agents.keys())
       count = len(agent_names)

       order = self._random_permutation(count) if self._random_order else self._sorted_permutation(count)

       for i in range(count):
           name = agent_names[order[i]]

           if name not in self._agents:  # agent may have been stopped/removed during this turn
               continue

           agent = self._agents[name]

           if agent.must_run_setup:  # first turn runs Setup
               self._execute_setup(agent)
           else:
               self._execute_act(agent)

       if self._delay_after_turn_ms > 0:
           time.sleep(self._delay_after_turn_ms / 1000.0)

       self.turn_finished(turn)
