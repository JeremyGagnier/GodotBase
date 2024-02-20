using Godot;
using System;
using System.Collections.Generic;

#nullable enable

namespace Base
{
	abstract public class Command {}

	struct CommandFrame
	{
		public int frameNumber;
		public Command command;
	}

	public class Commands
	{
		private const int MAX_COMMANDS_PER_UPDATE = 10000;

		private static int frameNumber;
		private static List<CommandFrame> commandHistory = new();
		private static List<CommandFrame> newCommands = new();
		private static Dictionary<Type, Action<Command>> registeredActions = new();

		public static void Update()
		{
			int commandsProcessed = 0;
			while (newCommands.Count > 0)
			{
				CommandFrame commandToProcess = newCommands[newCommands.Count - 1];
				newCommands.RemoveAt(newCommands.Count - 1);
				commandHistory.Add(commandToProcess);
				if (registeredActions.ContainsKey(commandToProcess.GetType()))
				{
					registeredActions[commandToProcess.GetType()](commandToProcess.command);
				}
				else
				{
					GD.PrintErr($"Command of type {commandToProcess.GetType()} was not registered.");
				}
				commandsProcessed += 1;
				if (commandsProcessed > MAX_COMMANDS_PER_UPDATE)
				{
					// This can occur when processing a command adds another command to the list. This condition exists
					// to prevent an infinite loop.
					GD.PrintErr("Too many commands, likely an infinite loop.");
					break;
				}
			}
			frameNumber += 1;
		}

		public static void Add(Command command)
		{
			newCommands.Add(new CommandFrame { command = command, frameNumber = frameNumber });
		}

		public static void RegisterCommand(Type commandType, Action<Command> action)
		{
			registeredActions.Add(commandType, action);
		}
	}	
}
