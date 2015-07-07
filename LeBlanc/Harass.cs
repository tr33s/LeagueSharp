using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace LeBlanc
{
    internal class Harass
    {
        private const string Name = "Harass";
        public static Menu LocalMenu;
        private static Obj_AI_Hero CurrentTarget;

        static Harass()
        {
            #region Menu

            var harass = new Menu(Name + " Settings", Name);
            var harassQ = harass.AddMenu("Q", "Q");
            harassQ.AddBool("HarassQ", "Use Q");
            harassQ.AddSlider("HarassQMana", "Min Mana %", 40);

            var harassW = harass.AddMenu("W", "W");
            harassW.AddBool("HarassW", "Use W");
            harassW.AddBool("HarassW2", "Use Second W");
            harassW.AddList("HarassW2Mode", "Second W Setting", new[] { "Auto", "After E" });
            harassW.AddSlider("HarassWMana", "Min Mana %", 40);

            var harassE = harass.AddMenu("E", "E");
            harassE.AddBool("HarassE", "Use E");
            harassE.AddHitChance("HarassEHC", "Min HitChance", HitChance.Medium);
            harassE.AddSlider("HarassEMana", "Min Mana %", 40);

            //  harass.AddItem(new MenuItem("HarassCombo", "W->Q->E->W Combo").SetValue(true));

            /* var harassR = harass.AddSubMenu(new Menu("R", "R"));
            harassR.AddItem(new MenuItem("HarassR", "Use R").SetValue(true));
            */

            harass.AddKeyBind("HarassKey", "Harass Key", (byte) 'C');

            #endregion

            LocalMenu = harass;

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static Menu Menu
        {
            get { return Program.Menu; }
        }

        public static bool Enabled
        {
            get { return !Player.IsDead && Program.Menu.Item("HarassKey").GetValue<KeyBind>().Active; }
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

        private static void Game_OnGameUpdate(EventArgs args)
        {
            CurrentTarget = Utils.GetTarget(E.Range);

            if (!Enabled || !CurrentTarget.IsValidTarget(Q.Range))
            {
                return;
            }

            if (CastQ())
            {
                return;
            }

            if (CastE(HitChance.High))
            {
                return;
            }

            if (CastW())
            {
                return;
            }

            if (CastE())
            {
                return;
            }

            if (CastSecondW()) {}
        }

        private static bool CastQ()
        {
            return CanCast("Q") && Q.IsReady() && Q.CanCast(CurrentTarget) && Q.Cast(CurrentTarget).IsCasted();
        }

        private static bool CastW()
        {
            var canCast = CanCast("W") && W.IsReady(1);
            var qwRange = CurrentTarget.IsValidTarget(Q.Range + W.Range);
            var wRange = CurrentTarget.IsValidTarget(W.Range);

            if (!canCast)
            {
                return false;
            }

            if (wRange)
            {
                return W.Cast(CurrentTarget).IsCasted();
            }

            if (qwRange)
            {
                return W.Cast(Player.ServerPosition.Extend(CurrentTarget.ServerPosition, W.Range));
            }

            return false;
        }

        private static bool CastSecondW()
        {
            var canCast = LocalMenu.Item("HarassW2").GetValue<bool>() && W.IsReady(2);

            if (!canCast)
            {
                return false;
            }

            var mode = Menu.Item("HarassW2Mode").GetValue<StringList>().SelectedIndex;

            return mode == 0 ? W.Cast() : CurrentTarget.HasEBuff() && W.Cast();
        }

        private static bool CastE(HitChance hc = HitChance.Low)
        {
            if (!CanCast("E") || !E.IsReady() || !E.CanCast(CurrentTarget) || Player.IsDashing())
            {
                return false;
            }

            var chance = hc == HitChance.Low ? Menu.Item("HarassEHC").GetHitChance() : hc;
            var pred = E.GetPrediction(CurrentTarget);
            return pred.Hitchance >= chance && E.Cast(pred.CastPosition);
        }

        public static bool CanCast(string spell)
        {
            var cast = Menu.Item(Name + spell).GetValue<bool>();
            var lowMana = Player.ManaPercent < Menu.Item(Name + spell + "Mana").GetValue<Slider>().Value;
            return cast && !lowMana;
        }
    }
}