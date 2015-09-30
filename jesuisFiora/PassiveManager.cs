using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace jesuisFiora
{
    internal static class PassiveManager
    {
        public static List<FioraPassive> PassiveList = new List<FioraPassive>();
        private static readonly List<string> DirectionList = new List<string> { "NE", "NW", "SE", "SW" };

        static PassiveManager()
        {
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }

        public static int CountPassive(this Obj_AI_Base target)
        {
            return PassiveList.Count(obj => obj.Position.Distance(target.ServerPosition) <= 50);
        }

        public static FioraPassive GetNearestPassive(this Obj_AI_Base target)
        {
            return
                PassiveList.OrderBy(obj => obj.Position.Distance(target.ServerPosition))
                    .FirstOrDefault(obj => obj.IsValid && obj.IsVisible);
        }

        public static Vector3 GetPassivePosition(this Obj_AI_Base target)
        {
            // Console.WriteLine("SEARCH PASSIVE " + target.Name);
            var passive = target.GetNearestPassive();

            if (passive == null || target.Distance(passive.Position) == 0)
            {
                return Vector3.Zero;
            }
            
            var pos = Prediction.GetPrediction(target, Program.Q.Delay).UnitPosition.To2D();
            var d = passive.PassiveDistance;
            //var d = target.Distance(passive.Position) + 50;
            if (passive.Name.Contains("NE"))
            {
                pos.Y += d;
            }

            if (passive.Name.Contains("SE"))
            {
                pos.X -= d;
            }

            if (passive.Name.Contains("NW"))
            {
                pos.X += d;
            }

            if (passive.Name.Contains("SW"))
            {
                pos.Y -= d;
            }

            return pos.To3D();
        }

        public static double GetPassiveDamage(this Obj_AI_Base target)
        {
            var count = target.CountPassive();
            return count == 0 ? 0 : GetPassiveDamage(target, count);
        }

        public static double GetPassiveDamage(this Obj_AI_Base target, int passiveCount)
        {
            return passiveCount *
                   (.03f +
                    (Math.Min(
                        Math.Max(
                            .028f,
                            (.027 +
                             .001f * ObjectManager.Player.Level * ObjectManager.Player.FlatPhysicalDamageMod / 100f)),
                        .45f))) * target.MaxHealth;
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var emitter = sender as Obj_GeneralParticleEmitter;

            if (emitter == null || !emitter.IsValid)
            {
                return;
            }

            Console.WriteLine(emitter.Team);
            if (emitter.Name.Contains("Fiora_Base_Passive") && DirectionList.Any(emitter.Name.Contains) &&
                !emitter.Name.Contains("Warning"))
            {
                PassiveList.Add(new FioraPassive(emitter));
                Console.WriteLine("NEW PASSIVE DETECTED: " + emitter.Name);
            }

            if (emitter.Name.Contains("Fiora_Base_R_Mark") ||
                (emitter.Name.Contains("Fiora_Base_R") && emitter.Name.Contains("Timeout")))
            {
                PassiveList.Add(new FioraPassive(emitter, true));
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            PassiveList.RemoveAll(obj => obj.NetworkId.Equals(sender.NetworkId));
        }
    }

    public class FioraPassive : Obj_GeneralParticleEmitter
    {
        private readonly int MaxAliveTime;
        private readonly int SpawnTime;
        public bool IsUltPassive;
        public int PassiveDistance;

        public FioraPassive(Obj_GeneralParticleEmitter emitter, bool ultPassive = false)
            : base((ushort) emitter.Index, (uint) emitter.NetworkId)
        {
            SpawnTime = Utils.TickCount;
            IsUltPassive = ultPassive;
            MaxAliveTime = IsUltPassive ? 8000 : 15000;
            PassiveDistance = IsUltPassive ? 320 : 200;
        }

        private int VitalDuration
        {
            get { return Utils.TickCount - SpawnTime; }
        }

        private bool IsVitalIdentified
        {
            get { return VitalDuration > 500; }
        }

        private bool IsVitalTimedOut
        {
            get { return VitalDuration < MaxAliveTime; }
        }

        public bool IsActive
        {
            get { return IsValid && IsVisible && !IsVitalTimedOut; }
        }
    }
}