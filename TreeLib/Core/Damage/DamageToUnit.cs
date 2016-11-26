using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace TreeLib.Core.Damage
{
    internal static class DamageToUnit
    {
        private static readonly Dictionary<int, List<UnitDamage>> DamageDictionary =
            new Dictionary<int, List<UnitDamage>>();

        private static int _expirationTime = 15000;

        static DamageToUnit()
        {
            foreach (var enemy in HeroManager.Enemies)
            {
                DamageDictionary.Add(enemy.NetworkId, new List<UnitDamage>());
            }

            Game.OnUpdate += Game_OnUpdate;
            AttackableUnit.OnDamage += AttackableUnit_OnDamage;
        }

        public static void SetExpirationTime(int time)
        {
            _expirationTime = time;
        }

        public static float GetDamage(Obj_AI_Hero unit, int timeInMs = 2500)
        {
            if (!DamageDictionary.ContainsKey(unit.NetworkId))
            {
                return 0;
            }

            var currentTime = Utils.TickCount;
            return
                DamageDictionary[unit.NetworkId].Where(dmg => currentTime - dmg.Time <= timeInMs).Sum(dmg => dmg.Damage);
        }

        public static float GetPercentDamage(Obj_AI_Hero unit, int timeInMs = 2500)
        {
            return GetDamage(unit, timeInMs) / unit.MaxHealth * 100f;
        }

        public static Obj_AI_Hero GetTarget(float range,
            int timeInMs = 2500,
            TargetSelector.DamageType type = TargetSelector.DamageType.Physical)
        {
            Obj_AI_Hero target = null;
            var damage = 0f;

            foreach (var enemy in HeroManager.Enemies.Where(e => e.IsValidTarget(range)))
            {
                var dmg = GetPercentDamage(enemy, timeInMs);

                if (dmg <= damage)
                {
                    continue;
                }

                target = enemy;
                damage = dmg;
            }

            return damage == 0 ? TargetSelector.GetTarget(range, type) : target;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            var time = Utils.TickCount;
            foreach (var damage in DamageDictionary.Values)
            {
                damage.RemoveAll(d => time - d.Time >= _expirationTime);
            }
        }

        private static void AttackableUnit_OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            if (args.SourceNetworkId != ObjectManager.Player.NetworkId ||
                !DamageDictionary.ContainsKey(args.TargetNetworkId))
            {
                return;
            }

            DamageDictionary[args.TargetNetworkId].Add(new UnitDamage(args.Damage, args.Type));
        }

        public class UnitDamage
        {
            public float Damage;
            public int Time;
            public DamageType Type;

            public UnitDamage(float damage, DamageType type)
            {
                Damage = damage;
                Type = type;
                Time = Utils.TickCount;
            }
        }
    }
}