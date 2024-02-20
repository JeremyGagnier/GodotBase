using Godot;
using System;
using System.Collections.Generic;

#nullable enable

/// <summary>
/// UIActions contains various action dictionaries which should be modified to associate names to actions for the UI.
/// 
/// For example, a button press may need to trigger certain code, this is achieved by setting the action name in the
/// button (which can be serialized because it is a string) and then statically adding the corresponding action to the
/// buttonActions dictionary here.
/// </summary>
/// 
namespace Base
{
	public static class UIActions
	{
		public readonly static Dictionary<string, Action> buttonActions = new() {
			{"", () => {}},
			{"TitleScreen.PressedPlay", TitleScreen.PressedPlay},
			{"TitleScreen.PressedInstructions", TitleScreen.PressedInstructions},
			{"TitleScreen.PressedOptions", TitleScreen.PressedOptions},
			{"TitleScreen.PressedExit", TitleScreen.PressedExit},
		};

		public readonly static Dictionary<string, Action> textActions = new() {
			{"", () => {}},
		};
	}
}