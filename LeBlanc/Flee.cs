using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace LeBlanc
{
    internal class Flee
    {
        private const string Name = "Flee";
        public static Menu LocalMenu;

        static Flee()
        {
            #region Menu

            var flee = new Menu(Name + " Settings", Name);

            var fleeW = flee.AddMenu("W", "W");
            fleeW.AddBool("FleeW", "Use W");

            var fleeE = flee.AddMenu("E", "E");
            fleeE.AddBool("FleeE", "Use E");
            fleeE.AddHitChance("FleeEHC", "Min HitChance", HitChance.Medium);

            var fleeR = flee.AddMenu("R", "R");
            fleeR.AddBool("FleeRW", "Use W Ult");
            flee.AddKeyBind("FleeKey", "Flee Key", (byte) 'T');

            #endregion

            LocalMenu = flee;

            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static Menu Menu
        {
            get { return Program.Menu; }
        }

        public static bool Enabled
        {
            get { return !Player.IsDead && Menu.Item(Name + "Key").GetValue<KeyBind>().Active; }
        }

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
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

        private static HitChance EHitChance
        {
            get { return Menu.Item("FleeEHC").GetHitChance(); }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Enabled)
            {
                return;
            }

            Program.Orbwalker.ActiveMode = Orbwalking.OrbwalkingMode.None;

            MoveTo();

            if (CastW())
            {
                return;
            }

            if (CastR())
            {
                return;
            }

            if (CastE(EHitChance))
            {
                MoveTo();
            }
        }

        private static void MoveTo()
        {
            if (Player.IsDashing())
            {
                return;
            }

            Utils.Troll();
            var d = Player.ServerPosition.Distance(Game.CursorPos);
            Player.IssueOrder(GameObjectOrder.MoveTo, Player.ServerPosition.Extend(Game.CursorPos, d + 250));
        }

        private static bool CastW()
        {
            return !Player.IsDashing() && CanCast("W") && W.IsReady(1) && W.Cast(GetCastPosition());
        }

        private static bool CastE(HitChance hc)
        {
            var target = Utils.GetTarget(E.Range);

            if (Player.IsDashing() || !CanCast("E") || !E.IsReady() || !target.IsValidTarget(E.Range))
            {
                return false;
            }

            var pred = E.GetPrediction(target);
            return pred.Hitchance >= hc && E.Cast(pred.CastPosition);
        }

        private static bool CastR()
        {
            var canCast = !Player.IsDashing() && CanCast("RW") && R.IsReady(SpellSlot.W);
            return canCast && R.Cast(SpellSlot.W, GetCastPosition());
        }

        public static Vector3 GetCastPosition()
        {
            return Player.ServerPosition.Extend(Game.CursorPos, W.Range + new Random().Next(100));
        }

        public static bool CanCast(string spell)
        {
            return Menu.Item(Name + spell).GetValue<bool>();
        }
    }
}