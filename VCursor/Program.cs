using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace VCursor
{
    internal class Program
    {
        public static Menu Menu;
        public static bool FollowMovement => Menu.Item("Movement").IsActive();

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("VCursor", "VCursor", true);
            Menu.AddItem(new MenuItem("Movement", "Follow Cursor Movement").SetValue(true));
            Menu.AddItem(new MenuItem("Icon", "Change Icon [BROKEN IN L#]").SetValue(true));
            Menu.AddItem(new MenuItem("Chat", "Clear Chat on Load").SetValue(true));

            Menu.AddToMainMenu();

            if (Menu.Item("Chat").IsActive())
            {
                for (var i = 0; i < 15; i++)
                {
                    Game.PrintChat("<font color =\"\">");
                }
            }

            FakeClicks.Initialize(Menu);

            VirtualCursor.Initialize();
            VirtualCursor.SetPosition(Cursor.ScreenPosition);
            VirtualCursor.Draw();
        }
    }
}