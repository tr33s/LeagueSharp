using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using TreeLib.Extensions;
using Color = System.Drawing.Color;

namespace jesuisFiora
{
    internal static class PassiveManager
    {
        public static List<FioraPassive> PassiveList = new List<FioraPassive>();
        private static readonly List<string> DirectionList = new List<string> { "NE", "NW", "SE", "SW" };

        public static Menu Menu
        {
            get { return Program.Menu.SubMenu("Passive"); }
        }

        public static void Initialize()
        {
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Drawing.OnDraw += args =>
            {
                if (!Menu.Item("DrawPolygon").IsActive() && !Menu.Item("DrawCenter").IsActive())
                {
                    return;
                }

                foreach (var passive in PassiveList)
                {
                    var p1 = passive.Polygon;

                    if (Menu.Item("DrawPolygon").IsActive())
                    {
                        p1.Draw(passive.Color, 5);
                    }

                    if (Menu.Item("DrawCenter").IsActive())
                    {
                        Render.Circle.DrawCircle(p1.CenterOfPolygone().To3D(), 50, passive.Color);
                    }
                }
            };
        }

        public static int CountPassive(this Obj_AI_Base target)
        {
            return PassiveList.Count(obj => obj.IsValid && obj.Target.Equals(target));
        }

        public static FioraPassive GetNearestPassive(this Obj_AI_Base target)
        {
            return
                PassiveList.Where(obj => obj.Target.Equals(target))
                    .MinOrDefault(obj => obj.OrbwalkPosition.DistanceToPlayer());
        }

        public static double GetPassiveDamage(this Obj_AI_Base target, int? passiveCount = null)
        {
            return passiveCount ??
                   target.CountPassive() *
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

            var target = HeroManager.Enemies.MinOrDefault(enemy => enemy.Distance(emitter.Position));

            // 2 fioras?
            if (HeroManager.AllHeroes.Count(h => h.ChampionName.Equals("Fiora")) > 1 &&
                target.Distance(emitter.Position) > 30)
            {
                return;
            }

            if (emitter.Name.Contains("Fiora_Base_R_Mark") ||
                (emitter.Name.Contains("Fiora_Base_R") && emitter.Name.Contains("Timeout")) ||
                (emitter.Name.Contains("Fiora_Base_Passive") && DirectionList.Any(emitter.Name.Contains)))
            {
                Console.WriteLine(emitter.Name);
                PassiveList.Add(new FioraPassive(emitter, target));
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            PassiveList.RemoveAll(obj => obj.NetworkId.Equals(sender.NetworkId));
        }
    }

    public class FioraPassive : Obj_GeneralParticleEmitter
    {
        public enum PassiveType
        {
            Prepassive,
            Passive,
            PassiveTimeout,
            UltPassive,
            None
        }

        private static float LastPolygonRadius;
        private static float LastPolygonAngle;
        public readonly Color Color;
        public readonly PassiveType Passive;
        private readonly int PassiveDistance;
        public readonly Obj_AI_Hero Target;
        private Geometry.Polygon _polygon;
        private Vector3 LastPolygonPosition;

        public FioraPassive(Obj_GeneralParticleEmitter emitter, Obj_AI_Hero enemy)
            : base((ushort) emitter.Index, (uint) emitter.NetworkId)
        {
            Target = enemy;

            if (emitter.Name.Contains("Base_R"))
            {
                //PassiveManager.PassiveList.RemoveAll(
                //    p => p.Target.Equals(Target) && !p.Type.Equals(PassiveType.UltPassive));
                Passive = PassiveType.UltPassive;
                Color = Color.White;
            }
            else if (emitter.Name.Contains("Warning"))
            {
                Passive = PassiveType.Prepassive;
                Color = Color.Blue;
            }
            else if (emitter.Name.Contains("Timeout"))
            {
                PassiveManager.PassiveList.RemoveAll(p => p.Target.Equals(Target) && p.Type.Equals(PassiveType.Passive));
                Passive = PassiveType.PassiveTimeout;
                Color = Color.Red;
            }
            else
            {
                Passive = PassiveType.Passive;
                Color = Color.Green;
            }
            //Console.WriteLine("[PASSIVE] Type: {0} Target: {2} Name: {1}", Passive, Name, Target.Name);
            PassiveDistance = Passive.Equals(PassiveType.UltPassive) ? 320 : 200;
        }

        private static float PolygonAngle
        {
            get { return PassiveManager.Menu.Item("SectorAngle").GetValue<Slider>().Value; }
        }

        private static float PolygonRadius
        {
            get { return PassiveManager.Menu.Item("SectorMaxRadius").GetValue<Slider>().Value; }
        }

        public Geometry.Polygon Polygon
        {
            get
            {
                if (LastPolygonRadius == 0)
                {
                    LastPolygonRadius = PolygonRadius;
                }

                if (LastPolygonAngle == 0)
                {
                    LastPolygonAngle = PolygonAngle;
                }

                if (Target.ServerPosition.Equals(LastPolygonPosition) && PolygonRadius.Equals(LastPolygonRadius) &&
                    PolygonAngle.Equals(LastPolygonAngle) && _polygon != null)
                {
                    return _polygon;
                }

                _polygon = GetFilledPolygon();
                LastPolygonPosition = Target.ServerPosition;
                LastPolygonAngle = PolygonAngle;
                LastPolygonRadius = PolygonRadius;
                return _polygon;
            }
        }

        public Vector3 OrbwalkPosition
        {
            get { return Polygon.CenterOfPolygone().To3D(); }
        }

        public Vector3 CastPosition
        {
            get
            {
                return
                    Polygon.Points.Where(p => SpellManager.Q.IsInRange(p) && p.DistanceToPlayer() > 150)
                        .OrderByDescending(p => p.DistanceToPlayer())
                        .ThenBy(p => p.Distance(Target.ServerPosition))
                        .FirstOrDefault()
                        .To3D();
            }
        }

        private Geometry.Polygon GetFilledPolygon(bool predictPosition = false)
        {
            var basePos = predictPosition ? SpellManager.Q.GetPrediction(Target).UnitPosition : Target.ServerPosition;
            var pos = basePos + GetPassiveOffset();
            var points = new List<Vector2>();
            for (var i = 100; i < PolygonRadius; i += 10)
            {
                if (i > PolygonRadius)
                {
                    break;
                }

                var calcRads = PolygonAngle; //PolygonRadius - i < 50 ? PolygonAngle - 20 : PolygonAngle;
                var sector = new Geometry.Polygon.Sector(basePos, pos, Geometry.DegreeToRadian(calcRads), i, 30);
                sector.UpdatePolygon();
                points.AddRange(sector.Points);
            }

            points.RemoveAll(p => p.Distance(basePos) < 100);
            return new Geometry.Polygon { Points = points.Distinct().ToList() };
        }

        public Vector3 GetPassiveOffset(bool orbwalk = false)
        {
            var d = PassiveDistance;
            var offset = Vector3.Zero;

            if (orbwalk)
            {
                d -= 50;
            }

            if (Name.Contains("NE"))
            {
                offset = new Vector3(0, d, 0);
            }

            if (Name.Contains("SE"))
            {
                offset = new Vector3(-d, 0, 0);
            }

            if (Name.Contains("NW"))
            {
                offset = new Vector3(d, 0, 0);
            }

            if (Name.Contains("SW"))
            {
                offset = new Vector3(0, -d, 0);
            }

            return offset;
        }
    }

    public class QPosition
    {
        public Geometry.Polygon Polygon;
        public Vector3 Position;
        public FioraPassive.PassiveType Type;

        public QPosition(Vector3 position,
            FioraPassive.PassiveType type = FioraPassive.PassiveType.None,
            Geometry.Polygon polygon = null)
        {
            Position = position;
            Type = type;
            Polygon = polygon;
        }
    }
}