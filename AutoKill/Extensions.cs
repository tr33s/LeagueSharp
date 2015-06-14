using LeagueSharp;
using LeagueSharp.Common;

namespace AutoKill
{
    internal static class Extensions
    {
        public static bool IsValid(this SpellSlot spell)
        {
            return spell != SpellSlot.Unknown;
        }

        public static void AddBool(this Menu menu, string name, string displayName, bool value = true)
        {
            menu.AddItem(new MenuItem(name, displayName).SetValue(value));
        }

        public static bool IsCastableOnChampion(this Spell spell)
        {
            var name = spell.Instance.Name.ToLower();
            return name != "summonersmite" && name != "s5_summonersmitequick" && name != "itemsmiteaoe";
        }

        public static float GetAutoAttackRange(this Obj_AI_Hero hero)
        {
            return Orbwalking.GetRealAutoAttackRange(hero);
        }
    }
}