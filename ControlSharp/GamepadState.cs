#region

using System;
using SharpDX;
using SharpDX.XInput;

#endregion

namespace ControlSharp
{
    public class GamepadState
    {
        public readonly Controller Controller;
        public readonly UserIndex UserIndex;
        private int lastPacket;

        public GamepadState(UserIndex userIndex)
        {
            UserIndex = userIndex;
            Controller = new Controller(userIndex);
        }

        public DPadState DPad { get; private set; }
        public ThumbstickState LeftStick { get; private set; }
        public ThumbstickState RightStick { get; private set; }
        public bool A { get; private set; }
        public bool B { get; private set; }
        public bool X { get; private set; }
        public bool Y { get; private set; }
        public bool RightShoulder { get; private set; }
        public bool LeftShoulder { get; private set; }
        public bool Start { get; private set; }
        public bool Back { get; private set; }
        public float RightTrigger { get; private set; }
        public float LeftTrigger { get; private set; }

        public bool Connected
        {
            get { return Controller.IsConnected; }
        }

       /* public void Vibrate(float leftMotor, float rightMotor)
        {
            Controller.SetVibration(
                new Vibration
                {
                    LeftMotorSpeed = (ushort) (MathHelper.Saturate(leftMotor) * ushort.MaxValue),
                    RightMotorSpeed = (ushort) (MathHelper.Saturate(rightMotor) * ushort.MaxValue)
                });
        }*/

        public void Update()
        {
            // If not connected, nothing to update
            if (!Connected)
            {
                return;
            }

            var state = Controller.GetState();

            // If same packet, nothing to update
            if (lastPacket == state.PacketNumber)
            {
                return;
            }

            lastPacket = state.PacketNumber;
            var gamepadState = state.Gamepad;

            // Shoulders
            LeftShoulder = (gamepadState.Buttons & GamepadButtonFlags.LeftShoulder) != 0;
            RightShoulder = (gamepadState.Buttons & GamepadButtonFlags.RightShoulder) != 0;
            // Triggers
            LeftTrigger = gamepadState.LeftTrigger / (float) byte.MaxValue;
            RightTrigger = gamepadState.RightTrigger / (float) byte.MaxValue;

            // Buttons
            Start = (gamepadState.Buttons & GamepadButtonFlags.Start) != 0;
            Back = (gamepadState.Buttons & GamepadButtonFlags.Back) != 0;

            A = (gamepadState.Buttons & GamepadButtonFlags.A) != 0;
            B = (gamepadState.Buttons & GamepadButtonFlags.B) != 0;
            X = (gamepadState.Buttons & GamepadButtonFlags.X) != 0;
            Y = (gamepadState.Buttons & GamepadButtonFlags.Y) != 0;

            // D-Pad
            DPad = new DPadState(
                (gamepadState.Buttons & GamepadButtonFlags.DPadUp) != 0,
                (gamepadState.Buttons & GamepadButtonFlags.DPadDown) != 0,
                (gamepadState.Buttons & GamepadButtonFlags.DPadLeft) != 0,
                (gamepadState.Buttons & GamepadButtonFlags.DPadRight) != 0);

            // Thumbsticks
            LeftStick = new ThumbstickState(
                //Normalize(gamepadState.LeftThumbX, gamepadState.LeftThumbY, Gamepad.GamepadLeftThumbDeadZone),
                new Vector2(gamepadState.LeftThumbX, gamepadState.LeftThumbY),
                (gamepadState.Buttons & GamepadButtonFlags.LeftThumb) != 0);
            RightStick = new ThumbstickState(
                //Normalize(gamepadState.RightThumbX, gamepadState.RightThumbY, Gamepad.GamepadRightThumbDeadZone),
                new Vector2(gamepadState.RightThumbX, gamepadState.RightThumbY),
                (gamepadState.Buttons & GamepadButtonFlags.RightThumb) != 0);
        }

/*
        private static Vector2 Normalize(short rawX, short rawY, short threshold)
        {
            var value = new Vector2(rawX, rawY);
            var magnitude = value.Length();
            var direction = value / (magnitude == 0 ? 1 : magnitude);

            var normalizedMagnitude = 0.0f;
            if (magnitude - threshold > 0)
            {
                normalizedMagnitude = Math.Min((magnitude - threshold) / (short.MaxValue - threshold), 1);
            }

            return direction * normalizedMagnitude;
        }
*/

        public struct DPadState
        {
            public readonly int Count;
            public readonly bool Down, Left, Right;
            public readonly bool Up;

            public DPadState(bool up, bool down, bool left, bool right)
            {
                Count = 0;
                Up = up;
                Down = down;
                Left = left;
                Right = right;
                Count = Convert.ToInt32(up) + Convert.ToInt32(down) + Convert.ToInt32(left) + Convert.ToInt32(right);
            }
        }

        public struct ThumbstickState
        {
            public readonly bool Clicked;
            public readonly Vector2 Position;

            public ThumbstickState(Vector2 position, bool clicked)
            {
                Clicked = clicked;
                Position = position;
            }
        }
    }

    public static class MathHelper
    {
        public static float Saturate(float value)
        {
            return value < 0 ? 0 : value > 1 ? 1 : value;
        }
    }

    public static class Utilities
    {
        public static bool IsAnyPressed(this GamepadState.DPadState dpad)
        {
            return dpad.Count > 0;
        }

        public static bool IsABXYPressed(this GamepadState controller)
        {
            return controller.A || controller.B || controller.X || controller.Y;
        }
    }
}