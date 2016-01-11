using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using TreeLib.Extensions;

namespace Staberina
{
    internal static class Utility
    {
        private static readonly ItemId[] WardIds =
        {
            ItemId.Warding_Totem_Trinket, ItemId.Greater_Stealth_Totem_Trinket,
            ItemId.Greater_Vision_Totem_Trinket, ItemId.Sightstone, ItemId.Ruby_Sightstone, ItemId.Vision_Ward,
            (ItemId) 3711, (ItemId) 1411, (ItemId) 1410, (ItemId) 1408, (ItemId) 1409
        };

        public static bool MoveRandomly()
        {
            var pos = ObjectManager.Player.ServerPosition.Randomize(10, 20);
            return ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, pos, false);
        }

        public static Obj_AI_Base GetClosestETarget(Vector3 position)
        {
            return
                GetETargets()
                    .OrderBy(t => t.Distance(position))
                    .ThenByDescending(t => t.DistanceToPlayer())
                    .FirstOrDefault();
        }

        public static IEnumerable<Obj_AI_Base> GetETargets(Vector3 position = new Vector3())
        {
            return
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(o => o.IsValidTarget(SpellManager.E.Range, false, position) && !o.IsMe);
        }

        public static bool IsRReady()
        {
            var slot = ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R);
            return SpellManager.R.IsReady() || (slot.Level > 0 && slot.State.Equals(SpellState.Surpressed));
        }

        public static SpellSlot GetReadyWard()
        {
            var ward =
                ObjectManager.Player.InventoryItems.Where(s => WardIds.Contains(s.Id) && s.SpellSlot.IsReady())
                    .Select(s => s.SpellSlot)
                    .ToList();

            if (ward.Count == 0)
            {
                return SpellSlot.Unknown;
            }

            return ward.Contains(SpellSlot.Trinket) ? SpellSlot.Trinket : ward.FirstOrDefault();
        }

        public static float GetComboDamage(this Obj_AI_Base unit,
            bool q,
            bool w,
            bool e,
            bool r,
            bool calculateUltTick = false)
        {
            if (!unit.IsValidTarget())
            {
                return 0;
            }

            var d = 0f;

            if (q)
            {
                d += SpellManager.Q.GetDamage(unit);

                if (unit.HasBuff("katarinaqmark"))
                {
                    d += SpellManager.Q.GetDamage(unit, 1);
                }
            }

            if (w)
            {
                d += SpellManager.W.GetDamage(unit);
            }

            if (e)
            {
                d += SpellManager.E.GetDamage(unit);
            }

            if (r)
            {
                if (!calculateUltTick)
                {
                    d += SpellManager.R.GetDamage(unit, 1);
                }
                else
                {
                    d += unit.GetCalculatedRDamage(Katarina.UltTicks);
                }
            }

            d += (float) ObjectManager.Player.GetAutoAttackDamage(unit, true);

            var dl = ObjectManager.Player.GetMastery(MasteryData.Ferocity.DoubleEdgedSword);
            if (dl.IsActive())
            {
                d *= 1.03f;
            }

            var assasin = ObjectManager.Player.GetMastery((MasteryData.Cunning) 83);
            if (assasin.IsActive() && ObjectManager.Player.CountAlliesInRange(800) == 0)
            {
                d *= 1.015f;
            }

            var ignite = TreeLib.Managers.SpellManager.Ignite;
            if (ignite != null && ignite.IsReady())
            {
                d += (float) ObjectManager.Player.GetSummonerSpellDamage(unit, Damage.SummonerSpell.Ignite);
            }

            var tl = ObjectManager.Player.GetMastery(MasteryData.Cunning.ThunderlordsDecree);
            if (tl.IsActive()) {}
            if (ItemManager.LudensEcho != null) {}

            return d;
        }

        public static float GetComboDamage(Obj_AI_Base unit)
        {
            return unit.GetComboDamage(
                SpellManager.Q.IsReady(), SpellManager.W.IsReady(), SpellManager.E.IsReady(), IsRReady(), true);
        }

        public static float GetTimeToUnit(Obj_AI_Base unit)
        {
            var path = ObjectManager.Player.GetPath(unit.ServerPosition);

            if (path.Length == 0)
            {
                return 0;
            }

            var d = 0f;
            var lastPoint = path.FirstOrDefault();
            foreach (var point in path)
            {
                d += lastPoint.Distance(point);
                lastPoint = point;
            }

            return d / ObjectManager.Player.MoveSpeed;
        }

        public static float GetCalculatedRDamage(this Obj_AI_Base target, int ticks)
        {
            var dmg = ObjectManager.Player.GetDamageSpell(target, SpellSlot.R).CalculatedDamage;
            return (float) (dmg * ticks);
        }
    }
}