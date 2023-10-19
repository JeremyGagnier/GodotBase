using Godot;
using System.Collections.Generic;
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

	public class IdInfo
	{
		public int ElementId { get; set; }
		public int ComponentId { get; set; }
	}

	// Every UI component is recorded in elements and every node is recorded in elementNodes, in addition to their own
	// lists.
	[JsonInclude] private static List<ElementState?> _elementStates = new();
	public static ManagedList<ElementState> elements = new(_elementStates);
	public readonly static List<Node?> elementNodes = new();

	[JsonInclude] private static List<ButtonState?> _buttonStates = new();
	public static ManagedList<ButtonState> buttons = new(_buttonStates, MakeButton, UnmakeButton);
	public readonly static List<Button?> buttonNodes = new();

	[JsonInclude] private static List<PanelState?> _panelStates = new();
	public static ManagedList<PanelState> panels = new(_panelStates, MakePanel, UnmakePanel);
	public readonly static List<Panel?> panelNodes = new();

	[JsonInclude] private static List<HBoxContainerState?> _hBoxContainerStates = new();
	public static ManagedList<HBoxContainerState> hBoxContainers =
		new(_hBoxContainerStates, MakeHBoxContainer, UnmakeHBoxContainer);
	public readonly static List<HBoxContainer?> hBoxContainerNodes = new();

	public readonly static int RootId = -1;

	/// <summary>
	/// Clears all of the UI data and frees any corresponding Godot nodes.
	/// </summary>
	public static void Clear()
	{
		_elementStates.Clear();
		_buttonStates.Clear();
		_panelStates.Clear();
		_hBoxContainerStates.Clear();
		FreeNodes();
	}

	/// <summary>
	/// Frees of all Godot nodes. This is useful when headless mode is enabled and the nodes need to be
	/// destroyed.
	/// </summary>
	public static void FreeNodes()
	{
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
		hBoxContainerNodes.Clear();
	}

	/// <summary>
	/// Runs all of the node generation code for each element. This is useful when headless mode is disabled and
	/// the nodes need to be generated.
	/// </summary>
	public static void Generate()
	{
		if (elementNodes.Count != 0)
		{
			GD.PrintErr($"Tried to generate the UI when {elementNodes.Count} elements already exist.");
		}
		else if (!Root.instance.headlessMode)
		{
			int buttonIdx = 0;
			int panelIdx = 0;
			int hBoxContainerIdx = 0;
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
					case ElementType.HBoxContainer:
						HBoxContainerState? hBoxContainerState = hBoxContainers.Get(hBoxContainerIdx);
						if (hBoxContainerState == null)
						{
							GD.PrintErr("Found null panel state in UI.Generate.");
						}
						else
						{
							MakeHBoxContainer(hBoxContainerIdx, hBoxContainerState);
							hBoxContainerIdx += 1;
						}
						break;
				}
			}
		}
	}

	public static string Serialize()
	{
		return Serialization.SerializeStaticClass<UI>();
	}

	public static void Deserialize(string json)
	{
		Serialization.DeserializeStaticClass<UI>(json);
		// TODO: Find a way to re-link these datastructures automatically, otherwise the deserialization abstraction is
		// useless. Ref fields might solve this but they aren't available in C# 10.
		elements = new(_elementStates);
		buttons = new(_buttonStates, MakeButton, UnmakeButton);
		panels = new(_panelStates, MakePanel, UnmakePanel);
		hBoxContainers = new(_hBoxContainerStates, MakeHBoxContainer, UnmakeHBoxContainer);
		Generate();
	}

	public static IdInfo AddButton(
		int parentId,
		string text,
		string onPressActionName,
		bool disabled = false,
		bool visible = true)
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
			OnPressActionName = onPressActionName,
		};
		int buttonId = buttons.Add(buttonState);
		return new() { ElementId = elementId, ComponentId = buttonId };
	}

	public static void ModifyButton(
		int buttonId,
		string? text = null,
		string? onPressActionName = null,
		bool? disabled = null,
		bool? visible = null)
	{
		ButtonState? buttonState = buttons.Get(buttonId);
		if (buttonState == null)
		{
			GD.PrintErr("Tried to edit a button that was removed.");
		}
		else
		{
			string previousOnPressActionName = buttonState.OnPressActionName;
			if (text != null)
			{
				buttonState.Text = text;
			}
			if (onPressActionName != null)
			{
				buttonState.OnPressActionName = onPressActionName;
			}
			if (disabled != null)
			{
				buttonState.Disabled = disabled.Value;
			}
			if (visible != null)
			{
				ElementState? elementState = elements.Get(buttonState.ElementId);
				if (elementState == null)
				{
					GD.PrintErr("Tried to edit a button whose element was removed.");
				}
				else
				{
					elementState.Visible = visible.Value;
				}
			}
			if (text != null)
			{
				buttonState.Text = text;
			}
			if (!Root.instance.headlessMode)
			{
				Button? button = buttonNodes[buttonId];
				if (button != null)
				{
					if (visible != null)
					{
						button.Visible = visible.Value;
					}
					if (text != null)
					{
						button.Text = text;
					}
					if (onPressActionName != null)
					{
						button.Pressed -= UIActions.buttonActions[previousOnPressActionName];
						button.Pressed += UIActions.buttonActions[buttonState.OnPressActionName];
					}
					if (disabled != null)
					{
						button.Disabled = buttonState.Disabled;
					}
				}
			}
		}
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
					button.Pressed += UIActions.buttonActions[buttonState.OnPressActionName];
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

	public static IdInfo AddPanel(
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
		return new() { ElementId = elementId, ComponentId = panelId };
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

	public static IdInfo AddHBoxContainer(
		int parentId,
		int width,
		int height,
		Control.LayoutPreset layoutPreset,
		int? separation = null,
		int margin = 0,
		bool visible = true)
	{
		ElementState elementState = new()
		{
			Type = ElementType.HBoxContainer,
			ParentId = parentId,
			Visible = visible,
		};
		int elementId = elements.Add(elementState);
		HBoxContainerState hBoxContainerState = new()
		{
			ElementId = elementId,
			Width = width,
			Height = height,
			Margin = margin,
			Separation = separation,
			LayoutPreset = layoutPreset,
		};
		int hBoxContainerId = hBoxContainers.Add(hBoxContainerState);
		return new() { ElementId = elementId, ComponentId = hBoxContainerId };
	}

	public static void RemoveHBoxContainer(int hBoxContainerId)
	{
		HBoxContainerState? hBoxContainerState = hBoxContainers.Get(hBoxContainerId);
		if (hBoxContainerState != null)
		{
			elements.Remove(hBoxContainerState.ElementId);
			hBoxContainers.Remove(hBoxContainerId);
		}
	}

	private static void MakeHBoxContainer(int hBoxContainerId, HBoxContainerState hBoxContainerState)
	{
		if (!Root.instance.headlessMode)
		{
			ElementState? elementState = elements.Get(hBoxContainerState.ElementId);
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
					HBoxContainer hBoxContainer = new()
					{
						Visible = elementState.Visible,
						Size = new Vector2(hBoxContainerState.Width, hBoxContainerState.Height),
					};
					if (hBoxContainerState.Separation != null)
					{
						hBoxContainer.AddThemeConstantOverride("separation", hBoxContainerState.Separation.Value);
					}
					hBoxContainer.SetAnchorsAndOffsetsPreset(
						hBoxContainerState.LayoutPreset,
						margin: hBoxContainerState.Margin,
						resizeMode: Control.LayoutPresetMode.KeepSize);
					parentNode.AddChild(hBoxContainer);
					elementNodes.Add(hBoxContainer);
					hBoxContainerNodes.Add(hBoxContainer);
				}
			}
		}
	}

	private static void UnmakeHBoxContainer(int hBoxContainerId, HBoxContainerState hBoxContainer)
	{
		if (!Root.instance.headlessMode)
		{
			Node? node = elementNodes[hBoxContainer.ElementId];
			if (node != null)
			{
				node.QueueFree();
				elementNodes[hBoxContainer.ElementId] = null;
			}
		}
	}
}
