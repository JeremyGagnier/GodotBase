using Godot;
using System.Text.Json.Serialization;

#nullable enable

namespace Base
{
    public class TitleScreen
    {
        [JsonInclude] private static bool open = false; // Whether the title screen is currently open.
        [JsonInclude] private static bool opened = false; // Whether the title screen has ever been opened.

        [JsonInclude] public static int titlePanelId;

        public static void Init()
        {
            // TODO
        }

        public static void Open()
        {
            if (open)
            {
                GD.PrintErr("Tried to open the title screen but it was already open.");
            }
            else
            {
                open = true;
                if (opened)
                {
                    // Make the UI elements visible
                    UI.ModifyPanel(titlePanelId, visible: true);
                }
                else
                {
                    opened = true;
                    // Create the UI elements
                    UI.IdInfo panelIds = UI.AddPanel(
                        UI.RootId,
                        Root.instance.screenWidth,
                        Root.instance.screenHeight,
                        Control.LayoutPreset.Center);
                    titlePanelId = panelIds.ComponentId;
                    int buttonContainerElementId = UI.AddGridContainer(
                        panelIds.ElementId,
                        300,
                        400,
                        1,
                        Control.LayoutPreset.CenterBottom).ElementId;
                    UI.AddButton(
                        buttonContainerElementId,
                        "Play",
                        "TitleScreen.PressedPlay",
                        Control.SizeFlags.ExpandFill,
                        Control.SizeFlags.ExpandFill);
                    UI.AddButton(
                        buttonContainerElementId,
                        "Instructions",
                        "TitleScreen.PressedInstructions",
                        Control.SizeFlags.ExpandFill,
                        Control.SizeFlags.ExpandFill);
                    UI.AddButton(
                        buttonContainerElementId,
                        "Options",
                        "TitleScreen.PressedOptions",
                        Control.SizeFlags.ExpandFill,
                        Control.SizeFlags.ExpandFill);
                    UI.AddButton(
                        buttonContainerElementId,
                        "Exit",
                        "TitleScreen.PressedExit",
                        Control.SizeFlags.ExpandFill,
                        Control.SizeFlags.ExpandFill);
                }
            }
        }

        public static void Close()
        {
            if (open)
            {
                open = false;
                // Make the UI elements invisible
                UI.ModifyPanel(titlePanelId, visible: false);
            }
            else
            {
                GD.PrintErr("Tried to close the title screen but it was already closed.");
            }
        }

        public static void PressedPlay()
        {
            // TODO
        }

        public static void PressedInstructions()
        {
            // TODO
        }

        public static void PressedOptions()
        {
            // TODO
        }

        public static void PressedExit()
        {
            Root.instance.Quit();
        }
    }
}