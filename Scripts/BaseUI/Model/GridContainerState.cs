using Godot;

#nullable enable

public class GridContainerState
{
	public int ElementId { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public int Columns { get; set; }
	public int? HSeparation { get; set; }
	public int? VSeparation { get; set; }
	public int Margin { get; set; }
	public Control.LayoutPreset LayoutPreset { get; set; }
	public GridContainerState() {}
}
