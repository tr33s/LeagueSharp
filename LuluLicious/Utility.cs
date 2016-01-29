using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Objects;
using TreeLib.SpellData;

namespace LuluLicious
{
    internal static class Utility
    {
        public static Obj_AI_Hero GetBestWETarget()
        {
            var ally =
                HeroManager.Allies.Where(h => !h.IsMe && h.IsValidTarget(SpellManager.W.Range, false))
                    .OrderByDescending(h => Champion.Menu.Item(h.ChampionName + "WEPriority").GetValue<Slider>().Value)
                    .FirstOrDefault();

            return ally == null || Champion.Menu.Item(ally.ChampionName + "WEPriority").GetValue<Slider>().Value == 0
                ? null
                : ally;
        }

        public static Obj_AI_Hero GetBestWTarget()
        {
            var enemy =
                HeroManager.Enemies.Where(h => h.IsValidTarget(SpellManager.W.Range))
                    .MaxOrDefault(o => Champion.Menu.Item(o.ChampionName + "WPriority").GetValue<Slider>().Value);
            return enemy == null || Champion.Menu.Item(enemy.ChampionName + "WPriority").GetValue<Slider>().Value == 0
                ? null
                : enemy;
        }

        public static float GetPredictedHealthPercent(this Obj_AI_Hero hero)
        {
            var dmg = 0d;
            foreach (var skillshot in Evade.GetSkillshotsAboutToHit(hero, 400))
            {
                try
                {
                    dmg += skillshot.Unit.GetDamageSpell(hero, skillshot.SpellData.SpellName).CalculatedDamage;
                }
                catch {}
            }

            return (float) ((hero.Health - dmg) / hero.MaxHealth * 100);
        }
    }
}