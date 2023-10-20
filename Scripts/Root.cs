using Godot;

public partial class Root : Node
{
	private static Root _instance;
	public static Root instance
	{
		get { return _instance; }
	}

	[Export] private bool toggleHeadless = false;
	public bool headlessMode = false;

	public int screenWidth;
	public int screenHeight;
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_instance = this;

		Vector2I screenSize = GetTree().Root.Size;
		screenWidth = screenSize.X;
		screenHeight = screenSize.Y;

		int testPanelId = UI.AddPanel(UI.RootId, screenWidth / 2, screenHeight / 2, Control.LayoutPreset.Center).ElementId;
		int testHBoxContainerId = UI.AddHBoxContainer(testPanelId, screenWidth / 4, screenHeight / 4, Control.LayoutPreset.Center, separation: 16).ElementId;
		int testButtonId1 = UI.AddButton(testHBoxContainerId, "This is a button 1", "").ElementId;
		int testButtonId2 = UI.AddButton(testHBoxContainerId, "This is a button 2", "").ElementId;
		int testButtonId3 = UI.AddButton(testHBoxContainerId, "This is a button 3", "").ElementId;

		var x = UI.Serialize();
		UI.Clear();
		UI.Deserialize(x);
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
