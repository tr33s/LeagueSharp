using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace TentacleBabeIllaoi
{
    internal class TentacleManager
    {
        public static List<Tentacle> TentacleList = new List<Tentacle>();
        public static int TentacleAutoAttackRange;

        public static void Initialize()
        {
            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            foreach (var tentacle in
                TentacleList.Where(
                    t => t.IsValid && t.IsVisible && !t.Spellbook.IsAutoAttacking && !t.IsWindingUp && t.CanAttack()))
            {
                var target = TargetSelector.GetTarget(
                    tentacle, TentacleAutoAttackRange, TargetSelector.DamageType.Physical);
                if (target == null || !target.IsValid)
                {
                    continue;
                }

                tentacle.IssueOrder(GameObjectOrder.AttackUnit, target);
            }
        }

        public static void OnAttack(Tentacle tentacle, Obj_AI_Base target)
        {
            if (SpellManager.W.IsReady() && SpellManager.W.IsActive() && Program.LastTarget != null &&
                Program.LastTarget.Equals(target))
            {
                SpellManager.W.Cast();
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && sender.IsValid && sender.IsMe && args.Slot.Equals(SpellSlot.W))
            {
                Orbwalking.ResetAutoAttackTimer();

                foreach (var tentacle in TentacleList)
                {
                    tentacle.ResetAutoAttackTimer();
                }
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var minion = sender as Obj_AI_Minion;
            if (minion == null || !minion.IsValid || !minion.IsAlly || !minion.Name.Equals("IllaoiMinion"))
            {
                return;
            }

            TentacleList.Add(new Tentacle(minion));
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            TentacleList.RemoveAll(t => t.NetworkId.Equals(sender.NetworkId));
        }
    }

    public class Tentacle : Obj_AI_Minion
    {
        private int LastAATick;

        public Tentacle(Obj_AI_Minion tentacle) : base((ushort) tentacle.Index, (uint) tentacle.NetworkId)
        {
            OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender != null && sender.IsValid && sender.Equals(this) && args.Target != null &&
                args.Target.IsValid<Obj_AI_Base>())
            {
                LastAATick = Utils.GameTimeTickCount - Game.Ping / 2;
            }
        }

        public new bool CanAttack()
        {
            return Utils.GameTimeTickCount + Game.Ping / 2f + 25 >= LastAATick + AttackDelay * 1000f;
        }

        public void ResetAutoAttackTimer()
        {
            LastAATick = 0;
        }
    }
}