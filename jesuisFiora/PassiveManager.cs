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
        private static readonly List<FioraPassive> PassiveList = new List<FioraPassive>();
        private static readonly List<string> DirectionList = new List<string> { "NE", "NW", "SE", "SW" };

        private static int _fioraCount;

        private static IEnumerable<Obj_GeneralParticleEmitter> VitalList
        {
            get { return ObjectManager.Get<Obj_GeneralParticleEmitter>().Where(IsFioraPassive); }
        }

        public static Menu Menu
        {
            get { return Program.Menu.SubMenu("Passive"); }
        }

        public static void Initialize()
        {
            _fioraCount = HeroManager.AllHeroes.Count(h => h.ChampionName == "Fiora");

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
            {
                return;
            }

            UpdatePassiveList();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Menu.Item("DrawPolygon").IsActive() && !Menu.Item("DrawCenter").IsActive())
            {
                return;
            }

            try
            {
                foreach (var passive in
                    PassiveList.Where(
                        p =>
                            p.IsValid && p.IsVisible && p.Target.IsValid && p.Target.IsVisible &&
                            p.Target.IsHPBarRendered))
                {
                    if (Menu.Item("DrawPolygon").IsActive() && passive.SimplePolygon != null)
                    {
                        passive.SimplePolygon.Draw(passive.Color);
                    }

                    if (Menu.Item("DrawCenter").IsActive())
                    {
                        Render.Circle.DrawCircle(passive.OrbwalkPosition, 50, passive.Color);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static int CountPassive(this Obj_AI_Hero target)
        {
            return PassiveList.Count(p => p.Target.NetworkId == target.NetworkId);
        }

        public static FioraPassive GetNearestPassive(this Obj_AI_Hero target)
        {
            var list = PassiveList.Where(p => p.Target.NetworkId == target.NetworkId);
            var fioraPassives = list as FioraPassive[] ?? list.ToArray();
            return !fioraPassives.Any()
                ? null
                : fioraPassives.Where(p => p.IsValid && p.IsVisible)
                    .MinOrDefault(obj => obj.OrbwalkPosition.DistanceToPlayer());
        }

        public static FioraPassive GetFurthestPassive(this Obj_AI_Hero target)
        {
            var list = PassiveList.Where(p => p.Target.NetworkId == target.NetworkId);
            var fioraPassives = list as FioraPassive[] ?? list.ToArray();
            return !fioraPassives.Any()
                ? null
                : fioraPassives.Where(p => p.IsValid && p.IsVisible)
                    .MaxOrDefault(obj => obj.OrbwalkPosition.DistanceToPlayer());
        }

        public static bool HasUltPassive(this Obj_AI_Hero target)
        {
            return target.GetUltPassiveCount() > 0;
        }

        public static int GetUltPassiveCount(this Obj_AI_Hero target)
        {
            var passive = PassiveList.Where(p => p.Target.NetworkId == target.NetworkId);
            var fioraPassives = passive as FioraPassive[] ?? passive.ToArray();

            if (!fioraPassives.Any())
            {
                return 0;
            }

            return fioraPassives.Count(
                p => p.IsValid && p.IsVisible && p.Passive == FioraPassive.PassiveType.UltPassive);
        }

        public static double GetPassiveDamage(this Obj_AI_Hero target, int? passiveCount = null)
        {
            var modifier = (.03f +
                            Math.Min(
                                Math.Max(
                                    .028f,
                                    .027 +
                                    .001f * ObjectManager.Player.Level * ObjectManager.Player.FlatPhysicalDamageMod /
                                    100f), .45f)) * target.MaxHealth;
            return passiveCount * modifier ?? target.CountPassive() * modifier;
        }

        public static void UpdatePassiveList()
        {
            PassiveList.Clear();
            foreach (var vital in
                VitalList)
            {
                var vital1 = vital;
                var hero =
                    HeroManager.Enemies.Where(h => h.IsValidTarget()).MinOrDefault(h => h.Distance(vital1.Position));
                if (hero != null)
                {
                    PassiveList.Add(new FioraPassive(vital, hero));
                }
            }
        }

        public static bool IsFioraPassive(this Obj_GeneralParticleEmitter emitter)
        {
            return emitter != null && emitter.IsValid &&
                   (emitter.Name.Contains("Fiora_Base_R_Mark") ||
                    (emitter.Name.Contains("Fiora_Base_R") && emitter.Name.Contains("Timeout")) ||
                    (emitter.Name.Contains("Fiora_Base_Passive") && DirectionList.Any(emitter.Name.Contains)));
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
        private Vector3 _polygonCenter;
        private Geometry.Polygon.Sector _simplePolygon;
        private Vector3 LastPolygonPosition;
        private Vector3 LastSimplePolygonPosition;
        public FioraPassive() {}

        public FioraPassive(Obj_GeneralParticleEmitter emitter, Obj_AI_Hero enemy)
            : base((ushort) emitter.Index, (uint) emitter.NetworkId)
        {
            Target = enemy;

            if (emitter.Name.Contains("Base_R"))
            {
                //PassiveManager.PassiveList.RemoveAll(
                //    p => p.Target.Equals(Target) && !p.PassiveType.Equals(PassiveType.UltPassive));
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
                //PassiveManager.PassiveList.RemoveAll(p => p.Target.Equals(Target) && p.PassiveType.Equals(PassiveType.Passive));
                Passive = PassiveType.PassiveTimeout;
                Color = Color.Red;
            }
            else
            {
                Passive = PassiveType.Passive;
                Color = Color.Green;
            }
            //Console.WriteLine("[PASSIVE] PassiveType: {0} Target: {2} Name: {1}", Passive, Name, Target.Name);
            PassiveDistance = Passive == PassiveType.UltPassive ? 400 : 200;
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

                if (LastPolygonPosition != Vector3.Zero && Target.ServerPosition == LastPolygonPosition &&
                    PolygonRadius == LastPolygonRadius && PolygonAngle == LastPolygonAngle && _polygon != null)
                {
                    return _polygon;
                }

                _polygon = GetFilledPolygon();
                LastPolygonPosition = Target.ServerPosition;
                LastPolygonAngle = PolygonAngle;
                LastPolygonRadius = PolygonRadius;
                _polygonCenter = _polygon.CenterOfPolygone().To3D();
                return _polygon;
            }
        }

        public Geometry.Polygon.Sector SimplePolygon
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

                if (LastSimplePolygonPosition != Vector3.Zero && Target.ServerPosition == LastSimplePolygonPosition &&
                    PolygonRadius == LastPolygonRadius && PolygonAngle == LastPolygonAngle && _simplePolygon != null)
                {
                    return _simplePolygon;
                }

                _simplePolygon = GetSimplePolygon();
                LastSimplePolygonPosition = Target.ServerPosition;
                LastPolygonAngle = PolygonAngle;
                LastPolygonRadius = PolygonRadius;

                return _simplePolygon;
            }
        }

        public Vector3 OrbwalkPosition
        {
            get
            {
                return _polygonCenter == Vector3.Zero ? Vector3.Zero : Target.ServerPosition.Extend(_polygonCenter, 150);
            }
        }

        public Vector3 CastPosition
        {
            get
            {
                return
                    Polygon.Points.Where(
                        p => SpellManager.Q.IsInRange(p) && p.DistanceToPlayer() > 100 && p.Distance(Target) > 50)
                        .OrderBy(p => p.Distance(Target))
                        .ThenByDescending(p => p.DistanceToPlayer())
                        .FirstOrDefault()
                        .To3D();
            }
        }

        private Geometry.Polygon.Sector GetSimplePolygon(bool predictPosition = false)
        {
            var basePos = predictPosition ? SpellManager.Q.GetPrediction(Target).UnitPosition : Target.ServerPosition;
            var pos = basePos + GetPassiveOffset();
            var r = Passive == PassiveType.UltPassive ? 400 : PolygonRadius;
            var sector = new Geometry.Polygon.Sector(basePos, pos, Geometry.DegreeToRadian(PolygonAngle), r);
            sector.UpdatePolygon();
            return sector;
        }

        private Geometry.Polygon GetFilledPolygon(bool predictPosition = false)
        {
            var basePos = predictPosition ? SpellManager.Q.GetPrediction(Target).UnitPosition : Target.ServerPosition;
            var pos = basePos + GetPassiveOffset();
            //var polygons = new List<Geometry.Polygon>();
            var list = new List<Vector2>();
            var r = Passive == PassiveType.UltPassive ? 400 : PolygonRadius;
            var angle = Geometry.DegreeToRadian(PolygonAngle);

            for (var i = 100; i < r; i += 10)
            {
                if (i > r)
                {
                    break;
                }

                var sector = new Geometry.Polygon.Sector(basePos, pos, angle, i);
                sector.UpdatePolygon();
                list.AddRange(sector.Points);
                //polygons.Add(sector);
            }

            return new Geometry.Polygon { Points = list.Distinct().ToList() };
            //return polygons.JoinPolygons().FirstOrDefault();
        }

        public Vector3 GetPassiveOffset(bool orbwalk = false)
        {
            var d = PassiveDistance;
            var offset = Vector3.Zero;

            if (orbwalk)
            {
                d -= 50;
                //d -= Passive.Equals(PassiveType.UltPassive) ? 200 : 50;
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
        public FioraPassive.PassiveType PassiveType;
        public Geometry.Polygon Polygon;
        public Vector3 Position;
        public Geometry.Polygon SimplePolygon;

        public QPosition(Vector3 position,
            FioraPassive.PassiveType passiveType = FioraPassive.PassiveType.None,
            Geometry.Polygon polygon = null,
            Geometry.Polygon simplePolygon = null)
        {
            Position = position;
            PassiveType = passiveType;
            Polygon = polygon;
            SimplePolygon = simplePolygon;
        }
    }
}