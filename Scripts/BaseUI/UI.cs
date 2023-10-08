using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;


public class UI
{
	public enum ElementType {
		Button,
		Panel,
	}
	
	public class ElementState
	{
		public ElementType Type { get; set; }
		public int ParentId { get; set; }
		public bool Disabled { get; set; }
		public ElementState() {}
	}

	[JsonInclude] private readonly static List<ElementState> _elementStates = new();
	public readonly static ManagedList<ElementState> elements = new(_elementStates);
	public readonly static List<Node> elementNodes = new();

	[JsonInclude] private readonly static List<ButtonState> _buttonStates = new();
	public readonly static ManagedList<ButtonState> buttons = new(_buttonStates, MakeButton, UnmakeButton);
	public readonly static List<Button> buttonNodes = new();

	public static void Clear()
	{
		/// <summary>
		/// Clears all of the UI data and disposes of any corresponding Godot nodes.
		/// </summary>
		_elementStates.Clear();
		_buttonStates.Clear();
		foreach (Node node in elementNodes)
		{
			node.Dispose();
		}
		elementNodes.Clear();
		buttonNodes.Clear();
	}

	public static void Generate()
	{
		/// <summary>
		/// Runs all of the node generation code for each element. This is useful when headless mode is disabled and
		/// the nodes need to be generated.
		/// </summary>
		if (elementNodes.Count != 0)
		{
			GD.PrintErr($"Tried to generate the UI when {elementNodes.Count} elements already exist.");
		}
		else
		{
			int buttonIdx = 0;
			foreach (ElementState elementState in elements)
			{
				switch (elementState.Type)
				{
					case ElementType.Button:
						MakeButton(buttonIdx, buttons.Get(buttonIdx));
						buttonIdx += 1;
						break;
				}
			}
		}
	}

	public static void Deserialize(string json)
	{
		Serialization.DeserializeStaticClass<UI>(json);
	}

	public static string Serialize()
	{
		return JsonSerializer.Serialize(new UI());
	}

	public static void RegisterFunction(string name, Action function)
	{
		// TODO
	}

	public static int AddButton(int parentId, string text, bool disabled = false)
	{
		ElementState elementState = new()
		{
			Type = ElementType.Button,
			ParentId = parentId,
			Disabled = disabled,
		};
		int elementId = elements.Add(elementState);
		ButtonState buttonState = new()
		{
			ElementId = elementId,
			Text = text,
		};
		int buttonId = buttons.Add(buttonState);
		return buttonId;
	}

	public static void RemoveButton(int buttonId)
	{
		int elementId = buttons.Get(buttonId).ElementId;
		elements.Remove(elementId);
		buttons.Remove(buttonId);
	}

	public static void MakeButton(int buttonId, ButtonState buttonState)
	{
		if (!Root.instance.headlessMode)
		{
			ElementState elementState = elements.Get(buttonState.ElementId);
			Node parentNode = elementNodes[elementState.ParentId];
			Button button = new()
			{
				Text = buttonState.Text,
				Disabled = elementState.Disabled,
			};
			parentNode.AddChild(button);
			elementNodes.Add(button);
			buttonNodes.Add(button);
		}
	}

	public static void UnmakeButton(int buttonId, ButtonState buttonState)
	{
		if (!Root.instance.headlessMode)
		{
			elementNodes[buttonState.ElementId].Dispose();
			elementNodes[buttonState.ElementId] = null;
			buttonNodes[buttonId] = null;
		}
	}
}
