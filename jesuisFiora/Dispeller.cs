using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace jesuisFiora
{
    internal static class Dispeller
    {
        public static Menu Menu;

        static Dispeller()
        {
            new Dispel("Vladimir", "VladimirHemoplagueDebuff", SpellSlot.R).Add();
            new Dispel("Tristana", "TristanaEChargeSound", SpellSlot.E).Add();
            new Dispel("Karthus", "KarthusFallenOne", SpellSlot.R).Add();
            new Dispel("Zed", "ZedUltExecute", SpellSlot.R).Add();
            //new Dispel("Fizz", "FizzRBonusBuff", SpellSlot.R, 2000).Add();
        }

        public static List<Dispel> Dispells
        {
            get { return Dispel.GetDispelList(); }
        }

        public static void Initialize(Menu menu)
        {
            foreach (var dispel in
                Dispel.GetDispelList().Where(d => HeroManager.Enemies.Any(h => h.ChampionName.Equals(d.ChampionName))))
            {
                menu.AddBool("Dispel" + dispel.ChampionName, "Dispel " + dispel.ChampionName + " " + dispel.Slot);
            }

            Menu = menu;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            var w = Program.W;

            if (!w.IsReady())
            {
                return;
            }

            foreach (var dispel in
                Dispells.Where(
                    d => Menu.Item("Dispel" + d.ChampionName) != null && Menu.Item("Dispel" + d.ChampionName).IsActive())
                )
            {
                var buff = ObjectManager.Player.Buffs.FirstOrDefault(b => b.DisplayName.Equals(dispel.BuffName));
                if (buff == null || !buff.IsValid || !buff.IsActive)
                {
                    continue;
                }

                if ((buff.EndTime - Game.Time) * 1000f + 500 < w.Delay * 1000f + Game.Ping / 1.85f + 750 + dispel.Offset)
                {
                    var target = TargetSelector.GetTargetNoCollision(w);
                    if (target != null && target.IsValidTarget(w.Range) && w.Cast(target).IsCasted())
                    {
                        return;
                    }

                    if (w.Cast(Game.CursorPos))
                    {
                        return;
                    }
                }
            }
        }
    }

    public class Dispel
    {
        private static readonly List<Dispel> DispelList = new List<Dispel>();
        public string BuffName;
        public string ChampionName;
        public int Offset;
        public SpellSlot Slot;

        public Dispel(string champName, string buff, SpellSlot slot, int offset = 0)
        {
            ChampionName = champName;
            BuffName = buff;
            Slot = slot;
            Offset = offset;
        }

        public void Add()
        {
            DispelList.Add(this);
        }

        public static List<Dispel> GetDispelList()
        {
            return DispelList;
        }
    }
}