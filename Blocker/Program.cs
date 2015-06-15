using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Blocker
{
    internal class Program
    {
        public static Menu Menu;
        public static Dictionary<int, Menu> ChampMenus = new Dictionary<int, Menu>();

        public static bool Enabled
        {
            get { return Menu.Item("Enabled").IsActive(); }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Menu = new Menu("Blocker", "Blocker", true);
            var champs = Menu.AddSubMenu(new Menu("Heroes", "Heroes"));
            champs.AddSubMenu(new Menu("Allies", "Allies"));
            champs.AddSubMenu(new Menu("Enemies", "Enemies"));

            Menu.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Menu.AddToMainMenu();

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValid && !h.IsMe))
            {
                var champ =
                    champs.SubMenu(hero.IsAlly ? "Allies" : "Enemies")
                        .AddSubMenu(new Menu(hero.ChampionName, hero.ChampionName));

                if (hero.IsAlly)
                {
                    champ.AddItem(new MenuItem("Ping", "Block Ping")).SetValue(false);
                    champ.Item("Ping").SetValue(false);
                }

                champ.AddItem(new MenuItem("Chat", "Block Chat").SetValue(false));
                champ.Item("Chat").SetValue(false);
                //champ.AddItem(new MenuItem("Emotes", "Block Emotes").SetValue(false));
                //champ.Item("Emotes").SetValue(false);

                ChampMenus.Add(hero.NetworkId, champ);
            }

            Game.OnPing += Game_OnPing;
            Game.OnChat += Game_OnChat;
            //Obj_AI_Base.OnPlayAnimation += Obj_AI_Hero_OnPlayAnimation;
        }

        private static void Game_OnPing(GamePingEventArgs args)
        {
            var unit = args.Source as Obj_AI_Hero;

            if (!Enabled || unit == null || !unit.IsValid || unit.IsMe ||
                !ChampMenus[unit.NetworkId].Item("Ping").IsActive())
            {
                return;
            }

            args.Process = false;
        }

        private static void Game_OnChat(GameChatEventArgs args)
        {
            if (args.Sender == null || !args.Sender.IsValid) // ping or buy item message
            {
                return;
            }

            if (!Enabled || args.Sender.IsMe || !ChampMenus[args.Sender.NetworkId].Item("Chat").IsActive())
            {
                return;
            }

            args.Process = false;
        }

        private static void Obj_AI_Hero_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            var emotes = new List<string> { "Joke", "Taunt", "Dance", "Toggle" }; // add champ mastery
            var unit = sender as Obj_AI_Hero;

            if (!Enabled || unit == null || !unit.IsValid || unit.IsMe ||
                !ChampMenus[unit.NetworkId].Item("Emotes").IsActive() || !emotes.Contains(args.Animation)) {}

            //Console.WriteLine(args.Animation);
            //args.Process = false;
        }
    }
}