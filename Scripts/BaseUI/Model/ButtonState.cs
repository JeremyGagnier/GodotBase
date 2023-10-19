using Godot;

#nullable enable

public class ButtonState
{
	public int ElementId { get; set; }
	public bool Disabled { get; set; }
	public string Text { get; set; }
	public string OnPressActionName { get; set; }
	public ButtonState()
	{
		Text = "";
		OnPressActionName = "";
	}
}
