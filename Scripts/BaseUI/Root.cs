using Godot;
using System;

public partial class Root : Node
{
	[Export] public bool headlessMode;
	private static Root _instance;
	public static Root instance
	{
		get { return _instance; }
	}
	
	public bool testBool;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_instance = this;
		
		testBool = (bool)GetMeta("testBool");
		
		// Creates a Node as a child of the screen.
		Node myNode = new Node();
		AddChild(myNode);
		// Creates a NinePatchRect as a child of the node and makes it full screen size.
		NinePatchRect myRect = new NinePatchRect();
		myRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		Image icon = Image.LoadFromFile("res://icon.svg");
		ImageTexture myTexture = ImageTexture.CreateFromImage(icon);
		myRect.Texture = myTexture;
		myNode.AddChild(myRect);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
