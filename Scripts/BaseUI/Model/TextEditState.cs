using Godot;

#nullable enable

public class TextEditState
{
	public int ElementId { get; set; }
	public bool Editable { get; set; }
	public string Text { get; set; }
	public string PlaceholderText { get; set; }
	public string OnChangeActionName { get; set; }
	public TextEditState()
	{
		Text = "";
        PlaceholderText = "";
		OnChangeActionName = "";
	}
}
