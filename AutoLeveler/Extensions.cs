using System.Windows.Input;

namespace AutoLeveler
{
    internal static class Extensions
    {
        public static bool IsKeyPressed(this Key key)
        {
            return Keyboard.IsKeyDown(key);
        }
    }
}