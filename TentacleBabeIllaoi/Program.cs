using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TreeLib.Core;
using TreeLib.Extensions;

namespace TentacleBabeIllaoi
{
    internal class Program
    {
        public static Orbwalking.Orbwalker Orbwalker;
        public static ColorBGRA ScriptColor = new ColorBGRA(0, 0, 255, 255);
        public static Menu Menu;
        public static Obj_AI_Hero LastTarget;

        public static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (!Player.IsChampion("Illaoi"))
            {
                return;
            }

            Bootstrap.Initialize();

            Menu = new Menu("TentacleBabeIllaoi", "TentacleBabeIllaoi", true);
            Menu.SetFontStyle(FontStyle.Regular, ScriptColor);

            Orbwalker = Menu.AddOrbwalker();

            var spells = Menu.AddMenu("Spells", "Spells");

            var q = spells.AddMenu("Q", "Q");
            q.AddBool("QCombo", "Use in Combo");
            q.AddBool("QHarass", "Use in Harass");

            var w = spells.AddMenu("W", "W");
            w.AddBool("WCombo", "Use in Combo");
            w.AddBool("WHarass", "Use in Harass");

            var e = spells.AddMenu("E", "E");
            e.AddBool("ECombo", "Use in Combo");
            e.AddBool("EHarass", "Use in Harass");

            var r = spells.AddMenu("R", "R");
            r.AddBool("RCombo", "Use in Combo");
            r.AddBool("RHarass", "Use in Harass");

            Menu.AddToMainMenu();

            TentacleManager.Initialize();
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            if (Player.IsDashing() || Player.IsWindingUp || Player.Spellbook.IsCastingSpell)
            {
                return;
            }

            var target = LockedTargetSelector.GetTarget(SpellManager.E.Range, TargetSelector.DamageType.Physical);

            if (target == null || !target.IsValidTarget(SpellManager.E.Range))
            {
                return;
            }

            if (!Orbwalker.ActiveMode.IsComboMode())
            {
                return;
            }

            var q = SpellManager.Q.IsReady() && SpellManager.Q.IsActive() && SpellManager.Q.IsInRange(target);
            var e = SpellManager.E.IsReady() && SpellManager.E.IsActive();
            var r = SpellManager.R.IsReady() && SpellManager.R.IsActive() && SpellManager.R.IsInRange(target);

            if (e && SpellManager.E.Cast(target).IsCasted())
            {
                return;
            }

            if (q && SpellManager.Q.Cast(target).IsCasted())
            {
                return;
            }

            if (r && SpellManager.R.Cast()) {}
        }

        private static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (SpellManager.W.IsReady() && SpellManager.W.IsActive())
            {
                SpellManager.W.Cast();
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && sender.IsValid && sender.IsMe && args.Slot.Equals(SpellSlot.W))
            {
                Orbwalking.ResetAutoAttackTimer();
            }
        }
    }
}