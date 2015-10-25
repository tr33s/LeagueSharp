using System;
using System.Collections.Generic;
using System.Linq;
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
            var skins = Enumerable.Range(0, 44).Select(n => n.ToString()).ToArray();


            Config = new Menu("SkinHack", "SkinHack", true);


            var champs = Config.AddSubMenu(new Menu("Champions", "Champions"));
            champs.AddItem(new MenuItem("Champions", "Reskin Champions").SetValue(true));

            foreach (var hero in HeroManager.AllHeroes.Where(h => !h.ChampionName.Equals("Ezreal")))
            {
                var champMenu = new Menu(hero.ChampionName, hero.ChampionName + hero.Team);
                var modelUnit = new ModelUnit(hero);

                PlayerList.Add(modelUnit);

                if (hero.IsMe)
                {
                    Player = modelUnit;
                }

                foreach (Dictionary<string, object> skin in ModelManager.GetSkins(hero.ChampionName))
                {
                    //Console.WriteLine(skin["name"].ToString());
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
                                    if (p.IsActive() && p.Name != skinName)
                                    {
                                        p.SetValue(false);
                                    }
                                });
                            modelUnit.SetModel(hero1.ChampionName, skinId);
                        }
                    };
                }
                champs.AddSubMenu(champMenu);
            }
            Config.AddToMainMenu();

            var wardMenu = Config.AddSubMenu(new Menu("Wards", "Wards"));
            wardMenu.AddItem(new MenuItem("Ward", "Reskin Wards").SetValue(true));
            wardMenu.AddItem(new MenuItem("WardOwn", "Reskin Only Own Wards").SetValue(true));
            wardMenu.AddItem(new MenuItem("WardIndex", "Ward Index").SetValue(new StringList(skins, 34))).ValueChanged
                += Program_ValueChanged;

            var minions = Config.AddSubMenu(new Menu("Minions", "Minions"));
            //settings.AddItem(new MenuItem("Pets", "Reskin Pets").SetValue(true));
            minions.AddItem(new MenuItem("Minions", "Pool Party Minions").SetValue(false));

            Game.OnInput += Game_OnInput;
        }

        private static void Program_ValueChanged(object sender, OnValueChangeEventArgs e)
        {
            foreach (var ward in
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(o => o.CharData.BaseSkinName.Contains("ward"))
                    .Where(
                        ward =>
                            !Config.Item("WardOwn").IsActive() ||
                            ward.Buffs.Any(b => b.SourceName.Equals(ObjectManager.Player.ChampionName))))
            {
                ward.SetSkin(ward.CharData.BaseSkinName, Convert.ToInt32(e.GetNewValue<StringList>().SelectedValue));
            }
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