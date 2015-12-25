using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace jesuisFiora
{
    internal static class UltTarget
    {
        public static int CastTime;
        public static int EndTime;
        public static Obj_AI_Hero Target;

        static UltTarget()
        {
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.IsMe || !args.Slot.Equals(SpellSlot.R))
            {
                return;
            }

            Target = args.Target as Obj_AI_Hero;
            CastTime = Utils.TickCount;
            EndTime = CastTime + 8000;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Target != null && Target.IsValid && (Target.IsDead || !Target.HasUltPassive()))
            {
                Target = null;
            }
        }
    }
}