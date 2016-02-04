using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace VCursor
{
    internal class Program
    {
        public static Menu Menu;
        public static List<Vector2> Points = new List<Vector2>();
        public static Vector2 Point1;
        public static Vector2 Point2;

        private static void Main(string[] args)
        {
            Console.WriteLine("WOO");
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("VCursor", "VCursor", true);
            Menu.AddItem(new MenuItem("Go", "Go").SetValue(new KeyBind('N', KeyBindType.Press)));
            Menu.AddToMainMenu();
            VirtualCursor.Initialize();
            VirtualCursor.SetPosition(Cursor.ScreenPosition);
            VirtualCursor.Draw();

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Point1.IsValid())
            {
                Drawing.DrawText(Point1.X, Point1.Y + 20, Color.Red, "START");
            }
            if (Point2.IsValid())
            {
                Drawing.DrawText(Point2.X, Point2.Y - 20, Color.Red, "END");
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Points.Count == 2)
            {
                Console.WriteLine("START");
                MouseManager.StartPath(Points[0], Points[1]);
                Point1 = Points[0];
                Point2 = Points[1];
                Points.Clear();
            }

            if (Menu.Item("Go").IsActive())
            {
                Point1 = Vector2.Zero;
                Point2 = Vector2.Zero;
                Points.Add(Cursor.ScreenPosition);
                Console.WriteLine("SET POINT" + Points.Count);
                Menu.Item("Go").SetValue(new KeyBind('N', KeyBindType.Press));
            }
        }
    }
}