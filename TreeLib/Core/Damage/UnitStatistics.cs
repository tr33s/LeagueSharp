using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace TreeLib.Core.Damage
{
    internal static class UnitStatistics
    {
        private static readonly Dictionary<int, StatisticProfile> StatisticDictionary =
            new Dictionary<int, StatisticProfile>();

        static UnitStatistics()
        {
            foreach (var obj in
                ObjectManager.Get<AttackableUnit>().Where(obj => obj.Team == GameObjectTeam.Neutral || obj.IsEnemy))
            {
                StatisticDictionary.Add(obj.NetworkId, new StatisticProfile(obj));
            }

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            GameObject.OnCreate += GameObject_OnCreate;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            StatisticProfile profile;

            if (!sender.IsMe || args.Target == null ||
                !StatisticDictionary.TryGetValue(args.Target.NetworkId, out profile))
            {
                return;
            }

            if (args.SData.IsAutoAttack())
            {
                profile.AutoAttacks++;
            }
            else
            {
                profile.TargetedSpells++;
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var obj = sender as AttackableUnit;
            if (obj != null && (obj.Team == GameObjectTeam.Neutral || obj.IsEnemy))
            {
                StatisticDictionary.Add(obj.NetworkId, new StatisticProfile(obj));
            }
        }

        public static StatisticProfile GetStatisticProfile(AttackableUnit unit)
        {
            StatisticProfile profile;
            return StatisticDictionary.TryGetValue(unit.NetworkId, out profile) ? profile : new StatisticProfile();
        }
    }

    public class StatisticProfile
    {
        public int AutoAttacks;
        public int TargetedSpells;
        public AttackableUnit Unit;

        public StatisticProfile() {}

        public StatisticProfile(AttackableUnit unit)
        {
            Unit = unit;
        }
    }
}