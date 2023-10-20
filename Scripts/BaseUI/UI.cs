using Godot;
using System;
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

	[JsonInclude] private static List<GridContainerState?> _gridContainerStates = new();
	public static ManagedList<GridContainerState> gridContainers =
		new(_gridContainerStates, MakeGridContainer, UnmakeGridContainer);
	public readonly static List<GridContainer?> gridContainerNodes = new();

	[JsonInclude] private static List<TextEditState?> _textEditStates = new();
	public static ManagedList<TextEditState> textEdits = new(_textEditStates, MakeTextEdit, UnmakeTextEdit);
	public readonly static List<TextEdit?> textEditNodes = new();

	public readonly static int RootId = -1;

	#region Generic Methods
	/// <summary>
	/// Clears all of the UI data and frees any corresponding Godot nodes.
	/// </summary>
	public static void Clear()
	{
		_elementStates.Clear();
		_buttonStates.Clear();
		_panelStates.Clear();
		_hBoxContainerStates.Clear();
		_gridContainerStates.Clear();
		_textEditStates.Clear();
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
		gridContainerNodes.Clear();
		textEditNodes.Clear();
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
			int gridContainerIdx = 0;
			int textEditIdx = 0;
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
							GD.PrintErr("Found null horizontal box container state in UI.Generate.");
						}
						else
						{
							MakeHBoxContainer(hBoxContainerIdx, hBoxContainerState);
							hBoxContainerIdx += 1;
						}
						break;
					case ElementType.GridContainer:
						GridContainerState? gridContainerState = gridContainers.Get(gridContainerIdx);
						if (gridContainerState == null)
						{
							GD.PrintErr("Found null grid container state in UI.Generate.");
						}
						else
						{
							MakeGridContainer(gridContainerIdx, gridContainerState);
							gridContainerIdx += 1;
						}
						break;
					case ElementType.TextEdit:
						TextEditState? textEditState = textEdits.Get(textEditIdx);
						if (textEditState == null)
						{
							GD.PrintErr("Found null text edit state in UI.Generate.");
						}
						else
						{
							MakeTextEdit(textEditIdx, textEditState);
							textEditIdx += 1;
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
		gridContainers = new(_gridContainerStates, MakeGridContainer, UnmakeGridContainer);
		textEdits = new(_textEditStates, MakeTextEdit, UnmakeTextEdit);
		Generate();
	}
	#endregion Generic Methods

	#region Element Methods

	#region Button
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
	#endregion Button

	#region Panel
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

	public static void ModifyPanel(
		int panelId,
		int? width = null,
		int? height = null,
		Control.LayoutPreset? layoutPreset = null,
		int? margin = null,
		bool? visible = null)
	{
		PanelState? panelState = panels.Get(panelId);
		if (panelState == null)
		{
			GD.PrintErr("Tried to edit a panel that was removed.");
		}
		else
		{
			if (width != null)
			{
				panelState.Width = width.Value;
			}
			if (height != null)
			{
				panelState.Height = height.Value;
			}
			if (layoutPreset != null)
			{
				panelState.LayoutPreset = layoutPreset.Value;
			}
			if (margin != null)
			{
				panelState.Margin = margin.Value;
			}
			if (visible != null)
			{
				ElementState? elementState = elements.Get(panelState.ElementId);
				if (elementState == null)
				{
					GD.PrintErr("Tried to edit a panel whose element was removed.");
				}
				else
				{
					elementState.Visible = visible.Value;
				}
			}

			if (!Root.instance.headlessMode)
			{
				Panel? panel = panelNodes[panelId];
				if (panel != null)
				{
					if (visible != null)
					{
						panel.Visible = visible.Value;
					}
					if (width != null || height != null)
					{
						panel.Size = new(panelState.Width, panelState.Height);
					}
					if (width != null || height != null || layoutPreset != null || margin != null)
					{
						panel.SetAnchorsAndOffsetsPreset(
							panelState.LayoutPreset,
							margin: panelState.Margin,
							resizeMode: Control.LayoutPresetMode.KeepSize);
					}
				}
			}
		}
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
	#endregion Panel

	#region HBoxContainer
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

	public static void ModifyHBoxContainer(
		int hBoxContainerId,
		int? width = null,
		int? height = null,
		Control.LayoutPreset? layoutPreset = null,
		int? separation = null,
		int? margin = null,
		bool? visible = null)
	{
		HBoxContainerState? hBoxContainerState = hBoxContainers.Get(hBoxContainerId);
		if (hBoxContainerState == null)
		{
			GD.PrintErr("Tried to edit a horizontal box container that was removed.");
		}
		else
		{
			if (width != null)
			{
				hBoxContainerState.Width = width.Value;
			}
			if (height != null)
			{
				hBoxContainerState.Height = height.Value;
			}
			if (layoutPreset != null)
			{
				hBoxContainerState.LayoutPreset = layoutPreset.Value;
			}
			if (separation != null)
			{
				hBoxContainerState.Separation = separation.Value;
			}
			if (margin != null)
			{
				hBoxContainerState.Margin = margin.Value;
			}
			if (visible != null)
			{
				ElementState? elementState = elements.Get(hBoxContainerState.ElementId);
				if (elementState == null)
				{
					GD.PrintErr("Tried to edit a horizontal box container whose element was removed.");
				}
				else
				{
					elementState.Visible = visible.Value;
				}
			}

			if (!Root.instance.headlessMode)
			{
				HBoxContainer? hBoxContainer = hBoxContainerNodes[hBoxContainerId];
				if (hBoxContainer != null)
				{
					if (visible != null)
					{
						hBoxContainer.Visible = visible.Value;
					}
					if (width != null || height != null)
					{
						hBoxContainer.Size = new(hBoxContainerState.Width, hBoxContainerState.Height);
					}
					if (width != null || height != null || layoutPreset != null || margin != null)
					{
						hBoxContainer.SetAnchorsAndOffsetsPreset(
							hBoxContainerState.LayoutPreset,
							margin: hBoxContainerState.Margin,
							resizeMode: Control.LayoutPresetMode.KeepSize);
					}
					if (separation != null)
					{
						hBoxContainer.AddThemeConstantOverride("separation", separation.Value);
					}
				}
			}
		}
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

	private static void UnmakeHBoxContainer(int hBoxContainerId, HBoxContainerState hBoxContainerState)
	{
		if (!Root.instance.headlessMode)
		{
			Node? node = elementNodes[hBoxContainerState.ElementId];
			if (node != null)
			{
				node.QueueFree();
				elementNodes[hBoxContainerState.ElementId] = null;
			}
		}
	}
	#endregion HBoxContainer

	#region GridContainer
	public static IdInfo AddGridContainer(
		int parentId,
		int width,
		int height,
		int columns,
		Control.LayoutPreset layoutPreset,
		int? hSeparation = null,
		int? vSeparation = null,
		int margin = 0,
		bool visible = true)
	{
		ElementState elementState = new()
		{
			Type = ElementType.GridContainer,
			ParentId = parentId,
			Visible = visible,
		};
		int elementId = elements.Add(elementState);
		GridContainerState gridContainerState = new()
		{
			ElementId = elementId,
			Width = width,
			Height = height,
			Margin = margin,
			Columns = columns,
			HSeparation = hSeparation,
			VSeparation = vSeparation,
			LayoutPreset = layoutPreset,
		};
		int gridContainerId = gridContainers.Add(gridContainerState);
		return new() { ElementId = elementId, ComponentId = gridContainerId };
	}

	public static void ModifyGridContainer(
		int gridContainerId,
		int? width = null,
		int? height = null,
		int? columns = null,
		Control.LayoutPreset? layoutPreset = null,
		int? hSeparation = null,
		int? vSeparation = null,
		int? margin = null,
		bool? visible = null)
	{
		GridContainerState? gridContainerState = gridContainers.Get(gridContainerId);
		if (gridContainerState == null)
		{
			GD.PrintErr("Tried to edit a grid container that was removed.");
		}
		else
		{
			if (width != null)
			{
				gridContainerState.Width = width.Value;
			}
			if (height != null)
			{
				gridContainerState.Height = height.Value;
			}
			if (columns != null)
			{
				gridContainerState.Columns = columns.Value;
			}
			if (layoutPreset != null)
			{
				gridContainerState.LayoutPreset = layoutPreset.Value;
			}
			if (hSeparation != null)
			{
				gridContainerState.HSeparation = hSeparation.Value;
			}
			if (vSeparation != null)
			{
				gridContainerState.VSeparation = vSeparation.Value;
			}
			if (margin != null)
			{
				gridContainerState.Margin = margin.Value;
			}
			if (visible != null)
			{
				ElementState? elementState = elements.Get(gridContainerState.ElementId);
				if (elementState == null)
				{
					GD.PrintErr("Tried to edit a grid container whose element was removed.");
				}
				else
				{
					elementState.Visible = visible.Value;
				}
			}

			if (!Root.instance.headlessMode)
			{
				GridContainer? gridContainer = gridContainerNodes[gridContainerId];
				if (gridContainer != null)
				{
					if (visible != null)
					{
						gridContainer.Visible = visible.Value;
					}
					if (columns != null)
					{
						gridContainer.Columns = columns.Value;
					}
					if (width != null || height != null)
					{
						gridContainer.Size = new(gridContainerState.Width, gridContainerState.Height);
					}
					if (width != null || height != null || layoutPreset != null || margin != null)
					{
						gridContainer.SetAnchorsAndOffsetsPreset(
							gridContainerState.LayoutPreset,
							margin: gridContainerState.Margin,
							resizeMode: Control.LayoutPresetMode.KeepSize);
					}
					if (hSeparation != null)
					{
						gridContainer.AddThemeConstantOverride("h_separation", hSeparation.Value);
					}
					if (vSeparation != null)
					{
						gridContainer.AddThemeConstantOverride("v_separation", vSeparation.Value);
					}
				}
			}
		}
	}

	public static void RemoveGridContainer(int gridContainerId)
	{
		GridContainerState? gridContainerState = gridContainers.Get(gridContainerId);
		if (gridContainerState != null)
		{
			elements.Remove(gridContainerState.ElementId);
			gridContainers.Remove(gridContainerId);
		}
	}

	private static void MakeGridContainer(int gridContainerId, GridContainerState gridContainerState)
	{
		if (!Root.instance.headlessMode)
		{
			ElementState? elementState = elements.Get(gridContainerState.ElementId);
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
					GridContainer gridContainer = new()
					{
						Visible = elementState.Visible,
						Size = new Vector2(gridContainerState.Width, gridContainerState.Height),
						Columns = gridContainerState.Columns,
					};
					if (gridContainerState.HSeparation != null)
					{
						gridContainer.AddThemeConstantOverride("h_separation", gridContainerState.HSeparation.Value);
					}
					if (gridContainerState.VSeparation != null)
					{
						gridContainer.AddThemeConstantOverride("v_separation", gridContainerState.VSeparation.Value);
					}
					gridContainer.SetAnchorsAndOffsetsPreset(
						gridContainerState.LayoutPreset,
						margin: gridContainerState.Margin,
						resizeMode: Control.LayoutPresetMode.KeepSize);
					parentNode.AddChild(gridContainer);
					elementNodes.Add(gridContainer);
					gridContainerNodes.Add(gridContainer);
				}
			}
		}
	}

	private static void UnmakeGridContainer(int gridContainerId, GridContainerState gridContainerState)
	{
		if (!Root.instance.headlessMode)
		{
			Node? node = elementNodes[gridContainerState.ElementId];
			if (node != null)
			{
				node.QueueFree();
				elementNodes[gridContainerState.ElementId] = null;
			}
		}
	}
	#endregion GridContainer

	#region TextEdit
	public static IdInfo AddTextEdit(
		int parentId,
		string onChangeActionName,
		bool editable = true,
		string text = "",
		string placeholderText = "",
		bool visible = true)
	{
		ElementState elementState = new()
		{
			Type = ElementType.TextEdit,
			ParentId = parentId,
			Visible = visible,
		};
		int elementId = elements.Add(elementState);
		TextEditState textEditState = new()
		{
			ElementId = elementId,
			Editable = editable,
			Text = text,
			PlaceholderText = placeholderText,
			OnChangeActionName = onChangeActionName,
		};
		int textEditId = textEdits.Add(textEditState);
		return new() { ElementId = elementId, ComponentId = textEditId };
	}

	public static void ModifyTextEdit(
		int textEditId,
		string? onChangeActionName = null,
		bool? editable = null,
		string? text = null,
		string? placeholderText = null,
		bool? visible = null)
	{
		TextEditState? textEditState = textEdits.Get(textEditId);
		if (textEditState == null)
		{
			GD.PrintErr("Tried to edit a text edit that was removed.");
		}
		else
		{
			string previousOnChangeActionName = textEditState.OnChangeActionName;
			if (onChangeActionName != null)
			{
				textEditState.OnChangeActionName = onChangeActionName;
			}
			if (editable != null)
			{
				textEditState.Editable = editable.Value;
			}
			if (text != null)
			{
				textEditState.Text = text;
			}
			if (placeholderText != null)
			{
				textEditState.PlaceholderText = placeholderText;
			}
			if (visible != null)
			{
				ElementState? elementState = elements.Get(textEditState.ElementId);
				if (elementState == null)
				{
					GD.PrintErr("Tried to edit a text edit whose element was removed.");
				}
				else
				{
					elementState.Visible = visible.Value;
				}
			}

			if (!Root.instance.headlessMode)
			{
				TextEdit? textEdit = textEditNodes[textEditId];
				if (textEdit != null)
				{
					if (visible != null)
					{
						textEdit.Visible = visible.Value;
					}
					if (editable != null)
					{
						textEdit.Editable = editable.Value;
					}
					if (text != null)
					{
						textEdit.Text = text;
					}
					if (placeholderText != null)
					{
						textEdit.PlaceholderText = placeholderText;
					}
					if (onChangeActionName != null)
					{
						textEdit.TextChanged -= UIActions.textActions[previousOnChangeActionName];
						textEdit.TextChanged += UIActions.textActions[textEditState.OnChangeActionName];
					}
				}
			}
		}
	}

	public static void RemoveTextEdit(int textEditId)
	{
		TextEditState? textEditState = textEdits.Get(textEditId);
		if (textEditState != null)
		{
			elements.Remove(textEditState.ElementId);
			textEdits.Remove(textEditId);
		}
	}

	private static void MakeTextEdit(int textEditId, TextEditState textEditState)
	{
		if (!Root.instance.headlessMode)
		{
			ElementState? elementState = elements.Get(textEditState.ElementId);
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
					TextEdit textEdit = new()
					{
						Visible = elementState.Visible,
						Editable = textEditState.Editable,
						Text = textEditState.Text,
						PlaceholderText = textEditState.PlaceholderText,
					};
					textEdit.TextChanged += UIActions.textActions[textEditState.OnChangeActionName];
					parentNode.AddChild(textEdit);
					elementNodes.Add(textEdit);
					textEditNodes.Add(textEdit);
				}
			}
		}
	}

	private static void UnmakeTextEdit(int textEditId, TextEditState textEditState)
	{
		if (!Root.instance.headlessMode)
		{
			Node? node = elementNodes[textEditState.ElementId];
			if (node != null)
			{
				node.QueueFree();
				elementNodes[textEditState.ElementId] = null;
			}
		}
	}
	#endregion TextEdit

	#endregion Element Methods
}
