using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;

namespace SkinHack
{
    internal class Program
    {
        public static List<ModelUnit> PlayerList = new List<ModelUnit>();
        public static ModelUnit Player;
        public static Menu Config;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Config = new Menu("SkinHack", "SkinHack", true);

            var settings = Config.AddSubMenu(new Menu("Settings", "Settings"));
            settings.AddItem(new MenuItem("Champions", "Reskin Champions").SetValue(true));
            //settings.AddItem(new MenuItem("Pets", "Reskin Pets").SetValue(true));
            settings.AddItem(new MenuItem("Minions", "Pool Party Minions").SetValue(false));

            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                var champMenu = new Menu(hero.ChampionName, hero.ChampionName);
                var modelUnit = new ModelUnit(hero);

                PlayerList.Add(modelUnit);

                if (hero.IsMe)
                {
                    Player = modelUnit;
                }

                foreach (Dictionary<string, object> skin in ModelManager.GetSkins(hero.ChampionName))
                {
                    Console.WriteLine(skin["name"].ToString());
                    var skinName = skin["name"].ToString().Equals("default")
                        ? hero.ChampionName
                        : skin["name"].ToString();
                    var skinId = (int) skin["num"];

                    var changeSkin = champMenu.AddItem(new MenuItem(skinName, skinName).SetValue(false));

                    if (hero.BaseSkinId.Equals(skinId) || changeSkin.IsActive())
                    {
                        changeSkin.SetValue(true);
                        modelUnit.SetModel(hero.CharData.BaseSkinName, skinId);
                    }

                    var hero1 = hero;
                    changeSkin.ValueChanged += (s, e) =>
                    {
                        if (e.GetNewValue<bool>())
                        {
                            champMenu.Items.ForEach(
                                p =>
                                {
                                    if (p.GetValue<bool>() && p.Name != skinName)
                                    {
                                        p.SetValue(false);
                                    }
                                });
                            modelUnit.SetModel(hero1.ChampionName, skinId);
                        }
                    };
                }
                Config.AddSubMenu(champMenu);
            }
            Config.AddToMainMenu();

            Game.OnInput += Game_OnInput;
        }

        private static void Game_OnInput(GameInputEventArgs args)
        {
            if (!args.Input.StartsWith("/"))
            {
                return;
            }

            if (args.Input.StartsWith("/model"))
            {
                args.Process = false;
                var model = args.Input.Replace("/model ", string.Empty).GetValidModel();

                if (!model.IsValidModel())
                {
                    return;
                }

                Player.SetModel(model);
                return;
            }

            if (args.Input.StartsWith("/skin"))
            {
                args.Process = false;
                try
                {
                    var skin = Convert.ToInt32(args.Input.Replace("/skin ", string.Empty));
                    Player.SetModel(Player.Unit.CharData.BaseSkinName, skin);
                }
                catch {}
            }
        }
    }
}