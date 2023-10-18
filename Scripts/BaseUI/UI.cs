using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

public class UI
{
	public enum ElementType {
		Button,
		Panel,
		HBoxContainer,
		GridContainer,
		TextEdit,
		HScrollBar,
		VScrollBar,
		HSlider,
		VSlider,
		ProgressBar,
		Label,
		LineEdit,
		TextureRect,
	}
	
	public class ElementState
	{
		public ElementType Type { get; set; }
		public int ParentId { get; set; }
		public bool Visible { get; set; }
		public ElementState() {}
	}

	[JsonInclude] private readonly static List<ElementState?> _elementStates = new();
	public readonly static ManagedList<ElementState> elements = new(_elementStates);
	public readonly static List<Node?> elementNodes = new();

	[JsonInclude] private readonly static List<ButtonState?> _buttonStates = new();
	public readonly static ManagedList<ButtonState> buttons = new(_buttonStates, MakeButton, UnmakeButton);
	public readonly static List<Button?> buttonNodes = new();
	private readonly static Dictionary<string, Action> buttonActions = new() {
		{"", () => {}}
	};

	[JsonInclude] private readonly static List<PanelState?> _panelStates = new();
	public readonly static ManagedList<PanelState> panels = new(_panelStates, MakePanel, UnmakePanel);
	public readonly static List<Panel?> panelNodes = new();

	public static void Clear()
	{
		/// <summary>
		/// Clears all of the UI data and frees any corresponding Godot nodes.
		/// </summary>
		_elementStates.Clear();
		_buttonStates.Clear();
		_panelStates.Clear();
		FreeNodes();
	}

	public static void FreeNodes()
	{
		/// <summary>
		/// Frees of all Godot nodes. This is useful when headless mode is enabled and the nodes need to be
		/// destroyed.
		/// </summary>
		foreach (Node? node in elementNodes)
		{
			if (node != null)
			{
				node.QueueFree();
			}
		}
		elementNodes.Clear();
		buttonNodes.Clear();
		panelNodes.Clear();
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
			int panelIdx = 0;
			foreach (ElementState? elementState in elements)
			{
				switch (elementState?.Type)
				{
					case ElementType.Button:
						ButtonState? buttonState = buttons.Get(buttonIdx);
						if (buttonState == null)
						{
							GD.PrintErr("Found null button state in UI.Generate.");
						}
						else
						{
							MakeButton(buttonIdx, buttonState);
							buttonIdx += 1;
						}
						break;
					case ElementType.Panel:
						PanelState? panelState = panels.Get(panelIdx);
						if (panelState == null)
						{
							GD.PrintErr("Found null panel state in UI.Generate.");
						}
						else
						{
							MakePanel(panelIdx, panelState);
							panelIdx += 1;
						}
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

	public static void RegisterActions(Dictionary<string, Action> newButtonActions)
	{
		foreach (KeyValuePair<string, Action> nameAndAction in newButtonActions)
		{
			buttonActions.Add(nameAndAction.Key, nameAndAction.Value);
		}
	}

	public static int AddButton(int parentId, string text, string onPressFunctionName, bool disabled = false, bool visible = true)
	{
		ElementState elementState = new()
		{
			Type = ElementType.Button,
			ParentId = parentId,
			Visible = visible,
		};
		int elementId = elements.Add(elementState);
		ButtonState buttonState = new()
		{
			Disabled = disabled,
			ElementId = elementId,
			Text = text,
			OnPressActionName = onPressFunctionName,
		};
		int buttonId = buttons.Add(buttonState);
		return buttonId;
	}

	public static void RemoveButton(int buttonId)
	{
		ButtonState? buttonState = buttons.Get(buttonId);
		if (buttonState != null)
		{
			elements.Remove(buttonState.ElementId);
			buttons.Remove(buttonId);
		}
	}

	private static void MakeButton(int buttonId, ButtonState buttonState)
	{
		if (!Root.instance.headlessMode)
		{
			ElementState? elementState = elements.Get(buttonState.ElementId);
			if (elementState != null)
			{
				Node? parentNode;
				if (elementState.ParentId == -1)
				{
					parentNode = Root.instance;
				}
				else
				{
					parentNode = elementNodes[elementState.ParentId];
				}
				if (parentNode != null)
				{
					Button button = new()
					{
						Visible = elementState.Visible,
						Text = buttonState.Text,
						Disabled = buttonState.Disabled,
					};
					button.Pressed += buttonActions[buttonState.OnPressActionName];
					parentNode.AddChild(button);
					elementNodes.Add(button);
					buttonNodes.Add(button);
				}
			}
		}
	}

	private static void UnmakeButton(int buttonId, ButtonState buttonState)
	{
		if (!Root.instance.headlessMode)
		{
			Node? node = elementNodes[buttonState.ElementId];
			if (node != null)
			{
				node.QueueFree();
				elementNodes[buttonState.ElementId] = null;
			}
		}
	}

	public static int AddPanel(
		int parentId,
		int width,
		int height,
		Control.LayoutPreset layoutPreset,
		int margin = 0,
		bool visible = true)
	{
		ElementState elementState = new()
		{
			Type = ElementType.Panel,
			ParentId = parentId,
			Visible = visible,
		};
		int elementId = elements.Add(elementState);
		PanelState panelState = new()
		{
			ElementId = elementId,
			Width = width,
			Height = height,
			Margin = margin,
			LayoutPreset = layoutPreset,
		};
		int panelId = panels.Add(panelState);
		return panelId;
	}

	public static void RemovePanel(int panelId)
	{
		PanelState? panelState = panels.Get(panelId);
		if (panelState != null)
		{
			elements.Remove(panelState.ElementId);
			panels.Remove(panelId);
		}
	}

	private static void MakePanel(int panelId, PanelState panelState)
	{
		if (!Root.instance.headlessMode)
		{
			ElementState? elementState = elements.Get(panelState.ElementId);
			if (elementState != null)
			{
				Node? parentNode;
				if (elementState.ParentId == -1)
				{
					parentNode = Root.instance;
				}
				else
				{
					parentNode = elementNodes[elementState.ParentId];
				}
				if (parentNode != null)
				{
					Panel panel = new()
					{
						Visible = elementState.Visible,
						Size = new Vector2(panelState.Width, panelState.Height),
					};
					panel.SetAnchorsAndOffsetsPreset(
						panelState.LayoutPreset,
						margin: panelState.Margin,
						resizeMode: Control.LayoutPresetMode.KeepSize);
					parentNode.AddChild(panel);
					elementNodes.Add(panel);
					panelNodes.Add(panel);
				}
			}
		}
	}

	private static void UnmakePanel(int panelId, PanelState panelState)
	{
		if (!Root.instance.headlessMode)
		{
			Node? node = elementNodes[panelState.ElementId];
			if (node != null)
			{
				node.QueueFree();
				elementNodes[panelState.ElementId] = null;
			}
		}
	}
}
