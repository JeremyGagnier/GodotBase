using Godot;

#nullable enable

namespace Base
{
	public partial class Root : Node
	{
		// _Ready needs to run before _instance will exist. This should happen right after the game loads, any static setup
		// should not require the Root instance.
		private static Root? _instance;
		public static Root instance
		{
			get { return _instance!; }
		}

		[Export] private bool toggleHeadless = false;
		public bool headlessMode = false;

		public int screenWidth;
		public int screenHeight;
		
		public override void _Ready()
		{
			_instance = this;

			Vector2I screenSize = GetTree().Root.Size;
			screenWidth = screenSize.X;
			screenHeight = screenSize.Y;

			TitleScreen.Init();

			// TODO: Remove below, for testing only
			TitleScreen.Open();
			var x = UI.Serialize();
			UI.Clear();
			UI.Deserialize(x);
		}

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
			Commands.Update();

		}

		public void Quit()
		{
			GetTree().Quit();
		}
	}
}