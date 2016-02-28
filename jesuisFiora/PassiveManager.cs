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
        public static Dictionary<Obj_AI_Hero, List<FioraPassive>> PassiveList =
            new Dictionary<Obj_AI_Hero, List<FioraPassive>>();

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
            foreach (var enemy in HeroManager.Enemies)
            {
                PassiveList.Add(enemy, new List<FioraPassive>());
            }

            _fioraCount = HeroManager.AllHeroes.Count(h => h.ChampionName == "Fiora");

            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            AttackableUnit.OnEnterVisiblityClient += AttackableUnit_OnEnterVisiblityClient;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            foreach (var enemyPassiveList in PassiveList.Values)
            {
                enemyPassiveList.RemoveAll(p => !p.IsValid);
            }
        }

        private static void AttackableUnit_OnEnterVisiblityClient(AttackableUnit sender, EventArgs args)
        {
            if (sender != null && sender.IsValid<Obj_AI_Hero>() && sender.IsEnemy && sender.DistanceToPlayer() < 1000)
            {
                UpdatePassiveList();
            }
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
                    PassiveList.Where(kvp => kvp.Key.IsValid && kvp.Key.IsVisible && kvp.Key.IsHPBarRendered)
                        .SelectMany(keyValue => keyValue.Value.Where(p => p.IsValid && p.IsVisible)))
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
            List<FioraPassive> list;
            PassiveList.TryGetValue(target, out list);
            return list == null || !target.IsValidTarget() ? 0 : list.Count(obj => obj.IsValid && obj.IsVisible);
        }

        public static FioraPassive GetNearestPassive(this Obj_AI_Hero target)
        {
            List<FioraPassive> list;
            PassiveList.TryGetValue(target, out list);

            if (list == null || list.Count == 0)
            {
                //UpdatePassiveList();
                return null;
            }

            return list.Where(p => p.IsValid && p.IsVisible).MinOrDefault(obj => obj.OrbwalkPosition.DistanceToPlayer());
        }

        public static bool HasUltPassive(this Obj_AI_Hero target)
        {
            List<FioraPassive> list;
            PassiveList.TryGetValue(target, out list);
            return list != null && list.Count > 0 &&
                   list.Any(
                       passive =>
                           passive.IsValid && passive.IsVisible &&
                           passive.Type.Equals(FioraPassive.PassiveType.UltPassive));
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
            foreach (var vital in
                VitalList.Where(
                    v => !PassiveList.Any(passiveList => passiveList.Value.Any(passive => passive.Equals(v)))))
            {
                var vital1 = vital;
                var hero =
                    HeroManager.Enemies.Where(h => h.IsValidTarget()).MinOrDefault(h => h.Distance(vital1.Position));
                if (hero != null)
                {
                    PassiveList[hero].Add(new FioraPassive(vital, hero));
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

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var emitter = sender as Obj_GeneralParticleEmitter;

            if (!IsFioraPassive(emitter))
            {
                return;
            }

            var target = HeroManager.Enemies.MinOrDefault(enemy => enemy.Distance(emitter.Position));

            // 2 fioras?
            if (_fioraCount > 1 && target.Distance(emitter.Position) > 30)
            {
                return;
            }

            PassiveList[target].Add(new FioraPassive(emitter, target));
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            foreach (var enemy in PassiveList)
            {
                enemy.Value.RemoveAll(passive => passive.NetworkId.Equals(sender.NetworkId));
            }
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
                //PassiveManager.PassiveList.RemoveAll(p => p.Target.Equals(Target) && p.Type.Equals(PassiveType.Passive));
                Passive = PassiveType.PassiveTimeout;
                Color = Color.Red;
            }
            else
            {
                Passive = PassiveType.Passive;
                Color = Color.Green;
            }
            //Console.WriteLine("[PASSIVE] Type: {0} Target: {2} Name: {1}", Passive, Name, Target.Name);
            PassiveDistance = Passive.Equals(PassiveType.UltPassive) ? 400 : 200;
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

                if (Target.ServerPosition.Equals(LastSimplePolygonPosition) && PolygonRadius.Equals(LastPolygonRadius) &&
                    PolygonAngle.Equals(LastPolygonAngle) && _simplePolygon != null)
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
            get { return Target.ServerPosition.Extend(Polygon.CenterOfPolygone().To3D(), 150); }
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
            var r = Passive.Equals(PassiveType.UltPassive) ? 400 : PolygonRadius;
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
            var r = Passive.Equals(PassiveType.UltPassive) ? 400 : PolygonRadius;
            for (var i = 100; i < r; i += 10)
            {
                if (i > r)
                {
                    break;
                }

                var calcRads = PolygonAngle;
                var sector = new Geometry.Polygon.Sector(basePos, pos, Geometry.DegreeToRadian(calcRads), i, 30);
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
        public Geometry.Polygon Polygon;
        public Vector3 Position;
        public Geometry.Polygon SimplePolygon;
        public FioraPassive.PassiveType Type;

        public QPosition(Vector3 position,
            FioraPassive.PassiveType type = FioraPassive.PassiveType.None,
            Geometry.Polygon polygon = null,
            Geometry.Polygon simplePolygon = null)
        {
            Position = position;
            Type = type;
            Polygon = polygon;
            SimplePolygon = simplePolygon;
        }
    }
}