using Godot;
using System;

public partial class Root : Node
{
	private static Root _instance;
	public static Root instance
	{
		get { return _instance; }
	}

	[Export] private bool toggleHeadless = false;
	public bool headlessMode = false;

	public int width;
	public int height;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_instance = this;

		Vector2I screenSize = GetTree().Root.Size;
		width = screenSize.X;
		height = screenSize.Y;

		int testPanelId = UI.AddPanel(-1, width / 2, height / 2, Control.LayoutPreset.Center);
		int testButtonId = UI.AddButton(testPanelId, "This is a button", "");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (toggleHeadless != headlessMode)
		{
			headlessMode = toggleHeadless;
			if (headlessMode)
			{
				UI.FreeNodes();
			}
			else
			{
				UI.Generate();
			}
		}
	}
}
