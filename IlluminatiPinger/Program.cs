using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace IlluminatiPinger
{
    internal class Program
    {
        public static Menu Menu;
        public static int Radius = 500;
        public static int LastPing;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Illuminati Pinger", "IlluminatiPinger", true);
            Menu.AddItem(
                new MenuItem("Type", "Ping Type").SetValue(
                    new StringList(
                        new[]
                        {
                            PingCategory.Danger.ToString(), PingCategory.AssistMe.ToString(),
                            PingCategory.EnemyMissing.ToString(), PingCategory.Fallback.ToString(),
                            PingCategory.Normal.ToString(), PingCategory.OnMyWay.ToString()
                        })));
            Menu.AddItem(new MenuItem("Points", "Number of Points").SetValue(new Slider(3, 2, 5)));
            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("Ping", "Ping").SetValue(new KeyBind('G', KeyBindType.Press)));
            Menu.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnChat += Game_OnChat;
        }

        private static void Game_OnChat(GameChatEventArgs args)
        {
            if (args.Message.StartsWith("You must wait"))
            {
                args.Process = false;
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Menu.Item("Enabled").IsActive() || !Menu.Item("Ping").IsActive() || Utils.TickCount - LastPing < 3000)
            {
                return;
            }


            var unit = UnitUnderCursor();

            if (unit == null || !unit.IsValid)
            {
                return;
            }

            SendPing(unit);
        }

        private static void SendPing(Obj_AI_Base unit)
        {
            var type =
                (PingCategory) Enum.Parse(typeof(PingCategory), Menu.Item("Type").GetValue<StringList>().SelectedValue);
            var count = Menu.Item("Points").GetValue<Slider>().Value;
            var point = unit.ServerPosition;

            Game.SendPing(type, point);

            for (var i = 0; i < count; i++)
            {
                var v = new Vector2
                {
                    X = (float) (point.X + Radius * Math.Cos(i * 2 * Math.PI / count)),
                    Y = (float) (point.Y + Radius * Math.Sin(i * 2 * Math.PI / count))
                };

                Game.SendPing(type, v);
            }

            LastPing = Utils.TickCount;
        }

        private static Obj_AI_Base UnitUnderCursor()
        {
            return
                ObjectManager.Get<Obj_AI_Base>()
                    .FirstOrDefault(unit => Game.CursorPos.Distance(unit.ServerPosition) < 200);
        }
    }
}