using System.Collections.Generic;
using LeagueSharp;

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

            List.Add("Aatrox", q);
            List.Add("Ahri", e);
            List.Add("Alistar", w);
            List.Add("Amumu", q);
            List.Add("Anivia", q);
            List.Add("Annie", r);
            List.Add("Ashe", r);
            List.Add("Azir", r);
            List.Add("Bard", q);
            List.Add("Blitzcrank", q);
            List.Add("Braum", r);
            List.Add("Cassiopeia", r);
            List.Add("Chogath", r);
            List.Add("Darius", r);
            List.Add("Elise", e);
            List.Add("Fiddlesticks", q);
            List.Add("Fiora", w);
            List.Add("Fizz", r);
            List.Add("Galio", r);
            List.Add("Garen", r);
            List.Add("MegaGnar", r);
            List.Add("Graves", r);
            List.Add("Hecarim", r);
            List.Add("Heimerdinger", e);
            List.Add("Irelia", e);
            List.Add("Janna", r);
            List.Add("Jarvan", q);
            List.Add("Jax", e);
            List.Add("LeeSin", r);
            List.Add("Leona", r);
            List.Add("Lissandra", r);
            List.Add("Lulu", w);
            List.Add("Lux", q);
            List.Add("Malphite", r);
            List.Add("Maoaki", w);
            List.Add("Morgana", q);
            List.Add("Nami", q);
            List.Add("Nautilus", q);
            List.Add("Rengar", e);
            List.Add("Ryze", w);
            List.Add("Shen", e);
            List.Add("Singed", e);
            List.Add("Teemo", q);
            List.Add("Thresh", q);
            List.Add("Tristana", r);
            List.Add("VelKoz", e);
            List.Add("Vi", r);
            List.Add("Zyra", e);
        }

        public static bool Contains(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            var name = unit.CharData.BaseSkinName;
            var slot = unit.GetSpellSlot(args);
            //Game.PrintChat("{0} {1}", name, slot);
            return List.ContainsKey(name) && List[name].Equals(slot);
        }
    }
}