using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Damage;

namespace LeBlanc
{
    internal class KillSteal
    {
        private const string Name = "KS";
        public static Menu LocalMenu;

        static KillSteal()
        {
            #region Menu

            var ks = new Menu(Name + " Settings", Name);
            ks.AddBool("KSEnabled", "Enable KS");
            ks.AddBool("KSIgnite", "KS with Ignite");
            ks.AddBool("KSSpells", "KS with Spells");

            #endregion

            LocalMenu = ks;

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static Menu Menu
        {
            get { return Program.Menu; }
        }

        public static bool Enabled
        {
            get { return !Player.IsDead && Menu.Item("KSEnabled").GetValue<bool>(); }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static Spell Q
        {
            get { return Spells.Q; }
        }

        private static Spell W
        {
            get { return Spells.W; }
        }

        private static Spell E
        {
            get { return Spells.E; }
        }

        private static Spell R
        {
            get { return Spells.R; }
        }

        private static SpellDataInst Ignite
        {
            get { return Player.Spellbook.GetSpell(Player.GetSpellSlot("summonerdot")); }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Enabled)
            {
                return;
            }

            if (CastIgnite())
            {
                return;
            }

            if (CanCast("Spells"))
            {
                if (CastQ())
                {
                    return;
                }

                switch (R.GetSpellSlot())
                {
                    case SpellSlot.Q:
                        if (CastQ(true))
                        {
                            return;
                        }
                        break;
                    case SpellSlot.W:
                        if (CastW(true))
                        {
                            return;
                        }
                        break;
                    case SpellSlot.E:
                        if (CastE(true))
                        {
                            return;
                        }
                        break;
                }

                if (CastW())
                {
                    return;
                }

                if (CastE()) {}
            }
        }

        private static bool CastQ(bool ult = false)
        {
            var canCast = (ult && R.IsReady(SpellSlot.Q)) || Q.IsReady();
            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj => obj.IsValidTarget(Q.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.Q));

            return canCast && unit.IsValidTarget(Q.Range) &&
                   (ult ? R.Cast(SpellSlot.Q, unit).IsCasted() : Q.Cast(unit).IsCasted());
        }

        private static bool CastW(bool ult = false)
        {
            var canCast = (ult && R.IsReady(SpellSlot.W)) || (W.IsReady(1));
            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj => obj.IsValidTarget(W.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.W));

            return canCast && unit.IsValidTarget(W.Range) &&
                   (ult ? R.Cast(SpellSlot.W, unit).IsCasted() : W.Cast(unit).IsCasted());
        }

        private static bool CastE(bool ult = false)
        {
            var canCast = (ult && R.IsReady(SpellSlot.E)) || E.IsSkillshot;
            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj => obj.IsValidTarget(E.Range) && obj.Health < Player.GetSpellDamage(obj, SpellSlot.E));

            if (!canCast || !unit.IsValidTarget(E.Range))
            {
                return false;
            }

            var pred = E.GetPrediction(unit);
            return pred.Hitchance >= HitChance.High && ult
                ? R.Cast(SpellSlot.E, pred.CastPosition)
                : E.Cast(pred.CastPosition);
        }

        private static bool CastIgnite()
        {
            var canCast = Menu.Item("KSIgnite").GetValue<bool>() && Ignite != null && Ignite.Slot != SpellSlot.Unknown &&
                          Ignite.Slot.IsReady();
            var unit =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        obj =>
                            obj.IsValidTarget(600) &&
                            obj.Health < Player.GetSummonerSpellDamage(obj, SummonerSpell.Ignite));

            return canCast && unit.IsValidTarget(600) && Player.Spellbook.CastSpell(Ignite.Slot, unit);
        }

        public static bool CanCast(string spell)
        {
            return LocalMenu.Item(Name + spell).GetValue<bool>();
        }
    }
}