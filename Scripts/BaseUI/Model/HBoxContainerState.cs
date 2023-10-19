using Godot;

#nullable enable

public class HBoxContainerState
{
	public int ElementId { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public int? Separation { get; set; }
    public int Margin { get; set; }
    public Control.LayoutPreset LayoutPreset { get; set; }
	public HBoxContainerState() {}
}
