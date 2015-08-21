using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace jesuisFiora
{
    internal static class SpellBlock
    {
        public static Dictionary<string, SpellSlot> List = new Dictionary<string, SpellSlot>();

        static SpellBlock()
        {
            const SpellSlot q = SpellSlot.Q;
            const SpellSlot w = SpellSlot.W;
            const SpellSlot e = SpellSlot.E;
            const SpellSlot r = SpellSlot.R;
            const SpellSlot N48 = (SpellSlot) 48;

            List.Add("Annie", q);
            List.Add("Alistar", w);
            List.Add("Chogath", r);
            List.Add("Darius", r);
            List.Add("Fiddlesticks", q);
            List.Add("Gangplank", q);
            List.Add("Garen", r);
            List.Add("Irelia", e);
            List.Add("Jayce", e);
            List.Add("LeBlanc", r);
            List.Add("LeeSin", r);
            List.Add("Lissandra", N48);
            List.Add("Lulu", w);
            List.Add("Maokai", w);
            List.Add("Mordekaiser", r);
            List.Add("Nasus", w);
            List.Add("Nunu", e);
            List.Add("Malzahar", r);
            List.Add("Pantheon", w);
            List.Add("Poppy", e);
            List.Add("Quinn", e);
            List.Add("Rammus", e);
            List.Add("Ryze", w);
            List.Add("Singed", e);
            List.Add("Skarner", r);
            List.Add("Syndra", r);
            List.Add("Swain", e);
            List.Add("TahmKench", w);
            List.Add("Talon", e);
            List.Add("Taric", e);
            List.Add("Teemo", q);
            List.Add("Tristana", r);
            List.Add("Urgot", r);
            List.Add("Vayne", e);
            List.Add("Veigar", r);
            List.Add("Vi", r);
            List.Add("Volibear", w);
            List.Add("Warwick", r);
            List.Add("XinZhao", r);
            List.Add("Zed", r);
        }

        public static void Initiate(Menu menu)
        {
            foreach (var s in
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.IsValid && hero.IsEnemy && List.ContainsKey(hero.ChampionName)))
            {
                menu.AddBool(s.ChampionName, "Block " + s + " " + List[s.ChampionName]);
            }
        }

        public static bool Contains(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            var name = unit.CharData.BaseSkinName;
            var slot = unit.GetSpellSlot(args);

            if (!List.ContainsKey(name) || !Program.Menu.Item(name).IsActive())
            {
                return false;
            }

            var correctSlot = List[name].Equals(slot);

            if (args.SData.IsAutoAttack())
            {
                if (name.Equals("Blitzcrank")) {}
            }

            if (name.Equals("Jayce"))
            {
                correctSlot = correctSlot && unit.CharData.BaseSkinName.Equals("JayceHammerForm");
            }

            if (name.Equals("LeBlanc"))
            {
                correctSlot = args.SData.Name.Equals("LeblancChaosOrbM");
            }

            if (name.Equals("Lissandra"))
            {
                //correctSlot = spellslot 48
            }

            //Game.PrintChat("{0} {1} {2}", name, slot, active);
            return correctSlot;
        }
    }
}