using HidWizards.UCR.Core.Attributes;
using HidWizards.UCR.Core.Models;
using HidWizards.UCR.Core.Models.Binding;
using System.ComponentModel;

namespace ButtonsToPOV
{
    [Plugin("Buttons to POV", Group = "Button", Description = "Map from 4 or 5 buttons to POV directions")]
    [PluginInput(DeviceBindingCategory.Momentary, "POV Up")]
    [PluginInput(DeviceBindingCategory.Momentary, "POV Right")]
    [PluginInput(DeviceBindingCategory.Momentary, "POV Down")]
    [PluginInput(DeviceBindingCategory.Momentary, "POV Left")]
    [PluginInput(DeviceBindingCategory.Momentary, "POV Recenter")]
    [PluginOutput(DeviceBindingCategory.Momentary, "POV Up")]
    [PluginOutput(DeviceBindingCategory.Momentary, "POV Right")]
    [PluginOutput(DeviceBindingCategory.Momentary, "POV Down")]
    [PluginOutput(DeviceBindingCategory.Momentary, "POV Left")]
    public class ButtonsToPOV : Plugin
    {
        [PluginGui("Up/Down Autocenter Mode")]
        public CenteringType AutoCenterUD { get; set; }

        [PluginGui("Left/Right Autocenter Mode")]
        public CenteringType AutoCenterLR { get; set; }

        private Direction currentUD = Direction.Center;
        private Direction currentLR = Direction.Center; // left = -1, right = 1

        private Direction UpdateDirection(
            CenteringType centering,
            Direction currentValue,
            bool negative,
            bool positive,
            Direction negativeDirection,
            Direction positiveDirection)
        {
            switch(centering)
            {
                case CenteringType.Toggle:
                    if ((negative && currentValue == negativeDirection) ||
                        (positive && currentValue == positiveDirection))
                        return Direction.Center;
                    else if (positive)
                        return positiveDirection;
                    else if (negative)
                        return negativeDirection;
                    else
                        return currentValue;
                case CenteringType.Opposite:
                    if ((positive && currentValue == negativeDirection) ||
                        (negative && currentValue == positiveDirection))
                        return Direction.Center;
                    else if (positive)
                        return positiveDirection;
                    else if (negative)
                        return negativeDirection;
                    else
                        return currentValue;
                default: // center on release as default
                case CenteringType.Release:
                    if (positive)
                        return positiveDirection;
                    else if (negative)
                        return negativeDirection;
                    else
                        return Direction.Center;
            }
        }

        public override void Update(params short[] values)
        {
            bool up = values[(int)Direction.Up] == 1;
            bool right = values[(int)Direction.Right] == 1;
            bool down = values[(int)Direction.Down] == 1;
            bool left = values[(int)Direction.Left] == 1;
            bool center = values[(int)Direction.Center] == 1;

            currentUD = center ? Direction.Center : UpdateDirection(AutoCenterUD, currentUD, down, up, Direction.Down, Direction.Up);
            currentLR = center ? Direction.Center : UpdateDirection(AutoCenterLR, currentLR, left, right, Direction.Left, Direction.Right);

            // output Up/Down
            if (currentUD != Direction.Up && currentUD != Direction.Down)
            { // output Up = 0 for centering
                currentUD = Direction.Center;
                WriteOutput((int)Direction.Up, 0);
            }
            else
                WriteOutput((int)currentUD, 1);

            // output Left/Right
            if (currentLR != Direction.Left && currentLR != Direction.Right)
            { // output Left = 0 for centering
                currentLR = Direction.Center;
                WriteOutput((int)Direction.Left, 0);
            }
            else
                WriteOutput((int)currentLR, 1);
        }
    }

    public enum CenteringType
    {
        [Description("Center when both directions are released")]
        Release, // center if both directions are released
        [Description("Center the current direction is pressed again")]
        Toggle, // center if the current direction is pressed again
        [Description("Center when the direction opposite to the current is pressed")]
        Opposite // center if direction opposite to the current is pressed
    }

    public enum Direction
    {
        Up = 0,
        Right,
        Down,
        Left,
        Center
    }
}
