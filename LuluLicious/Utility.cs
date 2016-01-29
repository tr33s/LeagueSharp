using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using TreeLib.Objects;
using TreeLib.SpellData;
using ItemData = LeagueSharp.Common.Data.ItemData;

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

        public static float GetComboDamage(this Obj_AI_Base unit)
        {
            var d = 0d;

            if (SpellManager.Q.IsReady())
            {
                d += SpellManager.Q.GetDamage(unit);
            }

            if (SpellManager.E.IsReady())
            {
                d += SpellManager.Q.GetDamage(unit);
            }

            d += (float) ObjectManager.Player.GetAutoAttackDamage(unit, true);

            var dl = ObjectManager.Player.GetMastery(MasteryData.Ferocity.DoubleEdgedSword);
            if (dl != null && dl.IsActive())
            {
                d *= 1.03f;
            }
            
            var assassin = ObjectManager.Player.GetMastery((MasteryData.Cunning) 83);
            if (assassin != null && assassin.IsActive() && ObjectManager.Player.CountAlliesInRange(800) == 0)
            {
                d *= 1.02f;
            }

            var ignite = TreeLib.Managers.SpellManager.Ignite;
            if (ignite != null && ignite.IsReady())
            {
                d += (float) ObjectManager.Player.GetSummonerSpellDamage(unit, Damage.SummonerSpell.Ignite);
            }

            var tl = ObjectManager.Player.GetMastery(MasteryData.Cunning.ThunderlordsDecree);
            if (tl != null && tl.IsActive() && !ObjectManager.Player.HasBuff("masterylordsdecreecooldown"))
            {
                d += 10 * ObjectManager.Player.Level + .3f * ObjectManager.Player.FlatPhysicalDamageMod +
                     .1f * ObjectManager.Player.TotalMagicalDamage;
            }

            if (ItemData.Ludens_Echo.GetItem() != null)
            {
                var b = ObjectManager.Player.GetBuff("itemmagicshankcharge");
                if (b != null && b.IsActive && b.Count >= 70)
                {
                    d += 100 + ObjectManager.Player.TotalMagicalDamage * .1f;
                }
            }

            return (float) d;
        }
    }
}