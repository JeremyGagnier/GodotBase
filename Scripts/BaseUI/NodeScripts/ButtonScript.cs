using Godot;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable enable

namespace Base
{
    public partial class ButtonScript : Button
    {
        public int nodeId = -1;
        public override void _GuiInput(InputEvent @event)
        {
            #if DEBUG
            if (nodeId == -1)
            {
                GD.PrintErr("Node received gui input but nodeId was not set!");
            }
            #endif
            Input.GuiInput(@event, nodeId);
        }
    }
}