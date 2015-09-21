using System;
using LeagueSharp;

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
            if (sender == null || !sender.IsValid || !sender.IsMe ||
                !ObjectManager.Player.GetSpellSlot(args).Equals(SpellSlot.R))
            {
                return;
            }

            Target = args.Target as Obj_AI_Hero;
            CastTime = Environment.TickCount;
            EndTime = CastTime + 8000;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if ((Target != null && Target.IsValid) && (Target.IsDead || Environment.TickCount - EndTime > 0))
            {
                Target = null;
            }
        }
    }
}