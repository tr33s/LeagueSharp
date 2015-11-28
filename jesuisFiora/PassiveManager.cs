namespace jesuisFiora
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    using SharpDX;

    using TreeLib.Extensions;

    using Color = System.Drawing.Color;

    internal static class PassiveManager
    {
        #region Static Fields

        public static Dictionary<Obj_AI_Hero, List<FioraPassive>> PassiveList =
            new Dictionary<Obj_AI_Hero, List<FioraPassive>>();

        private static readonly List<string> DirectionList = new List<string> { "NE", "NW", "SE", "SW" };

        #endregion

        #region Public Properties

        public static Menu Menu
        {
            get
            {
                return Program.Menu.SubMenu("Passive");
            }
        }

        #endregion

        #region Public Methods and Operators

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
                UpdatePassiveList();
                return null;
            }

            return list.Where(p => p.IsValid && p.IsVisible).MinOrDefault(obj => obj.OrbwalkPosition.DistanceToPlayer());
        }

        public static double GetPassiveDamage(this Obj_AI_Hero target, int? passiveCount = null)
        {
            return passiveCount
                   ?? target.CountPassive()
                   * (.03f
                      + Math.Min(
                          Math.Max(
                              .028f,
                              .027
                              + .001f * ObjectManager.Player.Level * ObjectManager.Player.FlatPhysicalDamageMod / 100f),
                          .45f)) * target.MaxHealth;
        }

        public static bool HasUltPassive(this Obj_AI_Hero target)
        {
            List<FioraPassive> list;
            PassiveList.TryGetValue(target, out list);
            return list != null && list.Count > 0
                   && list.Any(
                       passive =>
                       passive.IsValid && passive.IsVisible && passive.Type.Equals(FioraPassive.PassiveType.UltPassive));
        }

        public static void Initialize()
        {
            foreach (var enemy in HeroManager.Enemies)
            {
                PassiveList.Add(enemy, new List<FioraPassive>());
            }

            Game.OnUpdate += Game_OnUpdate;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public static bool IsFioraPassive(this Obj_GeneralParticleEmitter emitter)
        {
            return emitter.Name.Contains("Fiora_Base_R_Mark")
                   || (emitter.Name.Contains("Fiora_Base_R") && emitter.Name.Contains("Timeout"))
                   || (emitter.Name.Contains("Fiora_Base_Passive") && DirectionList.Any(emitter.Name.Contains));
        }

        public static void UpdatePassiveList()
        {
            foreach (var emitter in
                ObjectManager.Get<Obj_GeneralParticleEmitter>()
                    .Where(v => !PassiveList.Any(passiveList => passiveList.Value.Any(passive => passive.Equals(v)))))
            {
                var hero = HeroManager.Enemies.Where(h => h.IsValidTarget()).MinOrDefault(h => h.DistanceToPlayer());
                if (IsFioraPassive(emitter) && hero != null)
                {
                    PassiveList[hero].Add(new FioraPassive(emitter, hero));
                }
            }
        }

        #endregion

        #region Methods

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

        private static void Game_OnUpdate(EventArgs args)
        {
            foreach (var enemyPassiveList in PassiveList.Values)
            {
                enemyPassiveList.RemoveAll(p => !p.IsValid);
            }
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
            if (HeroManager.AllHeroes.Count(h => h.ChampionName.Equals("Fiora")) > 1
                && target.Distance(emitter.Position) > 30)
            {
                return;
            }

            if (IsFioraPassive(emitter))
            {
                PassiveList[target].Add(new FioraPassive(emitter, target));
            }
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            foreach (var enemy in PassiveList)
            {
                enemy.Value.RemoveAll(passive => passive.NetworkId.Equals(sender.NetworkId));
            }
        }

        #endregion
    }

    public class FioraPassive : Obj_GeneralParticleEmitter
    {
        #region Static Fields

        private static float LastPolygonAngle;

        private static float LastPolygonRadius;

        #endregion

        #region Fields

        public readonly Color Color;

        public readonly PassiveType Passive;

        public readonly Obj_AI_Hero Target;

        private readonly int PassiveDistance;

        private Geometry.Polygon _polygon;

        private Geometry.Polygon.Sector _simplePolygon;

        private Vector3 LastPolygonPosition;

        private Vector3 LastSimplePolygonPosition;

        #endregion

        #region Constructors and Destructors

        public FioraPassive(Obj_GeneralParticleEmitter emitter, Obj_AI_Hero enemy)
            : base((ushort)emitter.Index, (uint)emitter.NetworkId)
        {
            this.Target = enemy;

            if (emitter.Name.Contains("Base_R"))
            {
                //PassiveManager.PassiveList.RemoveAll(
                //    p => p.Target.Equals(Target) && !p.Type.Equals(PassiveType.UltPassive));
                this.Passive = PassiveType.UltPassive;
                this.Color = Color.White;
            }
            else if (emitter.Name.Contains("Warning"))
            {
                this.Passive = PassiveType.Prepassive;
                this.Color = Color.Blue;
            }
            else if (emitter.Name.Contains("Timeout"))
            {
                //PassiveManager.PassiveList.RemoveAll(p => p.Target.Equals(Target) && p.Type.Equals(PassiveType.Passive));
                this.Passive = PassiveType.PassiveTimeout;
                this.Color = Color.Red;
            }
            else
            {
                this.Passive = PassiveType.Passive;
                this.Color = Color.Green;
            }
            //Console.WriteLine("[PASSIVE] Type: {0} Target: {2} Name: {1}", Passive, Name, Target.Name);
            this.PassiveDistance = this.Passive.Equals(PassiveType.UltPassive) ? 400 : 200;
        }

        #endregion

        #region Enums

        public enum PassiveType
        {
            Prepassive,

            Passive,

            PassiveTimeout,

            UltPassive,

            None
        }

        #endregion

        #region Public Properties

        public Vector3 CastPosition
        {
            get
            {
                return
                    this.Polygon.Points.Where(
                        p => SpellManager.Q.IsInRange(p) && p.DistanceToPlayer() > 100 && p.Distance(this.Target) > 100)
                        .OrderBy(p => p.Distance(this.OrbwalkPosition))
                        .ThenByDescending(p => p.DistanceToPlayer())
                        .FirstOrDefault()
                        .To3D();
            }
        }

        public Vector3 OrbwalkPosition
        {
            get
            {
                return this.Polygon.CenterOfPolygone().To3D();
            }
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

                if (this.Target.ServerPosition.Equals(this.LastPolygonPosition)
                    && PolygonRadius.Equals(LastPolygonRadius) && PolygonAngle.Equals(LastPolygonAngle)
                    && this._polygon != null)
                {
                    return this._polygon;
                }

                this._polygon = this.GetFilledPolygon();
                this.LastPolygonPosition = this.Target.ServerPosition;
                LastPolygonAngle = PolygonAngle;
                LastPolygonRadius = PolygonRadius;
                return this._polygon;
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

                if (this.Target.ServerPosition.Equals(this.LastSimplePolygonPosition)
                    && PolygonRadius.Equals(LastPolygonRadius) && PolygonAngle.Equals(LastPolygonAngle)
                    && this._simplePolygon != null)
                {
                    return this._simplePolygon;
                }

                this._simplePolygon = this.GetSimplePolygon();
                this.LastSimplePolygonPosition = this.Target.ServerPosition;
                LastPolygonAngle = PolygonAngle;
                LastPolygonRadius = PolygonRadius;

                return this._simplePolygon;
            }
        }

        #endregion

        #region Properties

        private static float PolygonAngle
        {
            get
            {
                return PassiveManager.Menu.Item("SectorAngle").GetValue<Slider>().Value;
            }
        }

        private static float PolygonRadius
        {
            get
            {
                return PassiveManager.Menu.Item("SectorMaxRadius").GetValue<Slider>().Value;
            }
        }

        #endregion

        #region Public Methods and Operators

        public Vector3 GetPassiveOffset(bool orbwalk = false)
        {
            var d = this.PassiveDistance;
            var offset = Vector3.Zero;

            if (orbwalk)
            {
                //d -= 50;
                d -= this.Passive.Equals(PassiveType.UltPassive) ? 200 : 50;
            }

            if (this.Name.Contains("NE"))
            {
                offset = new Vector3(0, d, 0);
            }

            if (this.Name.Contains("SE"))
            {
                offset = new Vector3(-d, 0, 0);
            }

            if (this.Name.Contains("NW"))
            {
                offset = new Vector3(d, 0, 0);
            }

            if (this.Name.Contains("SW"))
            {
                offset = new Vector3(0, -d, 0);
            }

            return offset;
        }

        #endregion

        #region Methods

        private Geometry.Polygon GetFilledPolygon(bool predictPosition = false)
        {
            var basePos = predictPosition
                              ? SpellManager.Q.GetPrediction(this.Target).UnitPosition
                              : this.Target.ServerPosition;
            var pos = basePos + this.GetPassiveOffset();
            //var polygons = new List<Geometry.Polygon>();
            var list = new List<Vector2>();
            var r = this.Passive.Equals(PassiveType.UltPassive) ? 400 : PolygonRadius;
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

        private Geometry.Polygon.Sector GetSimplePolygon(bool predictPosition = false)
        {
            var basePos = predictPosition
                              ? SpellManager.Q.GetPrediction(this.Target).UnitPosition
                              : this.Target.ServerPosition;
            var pos = basePos + this.GetPassiveOffset();
            var r = this.Passive.Equals(PassiveType.UltPassive) ? 400 : PolygonRadius;
            var sector = new Geometry.Polygon.Sector(basePos, pos, Geometry.DegreeToRadian(PolygonAngle), r);
            sector.UpdatePolygon();
            return sector;
        }

        #endregion
    }

    public class QPosition
    {
        #region Fields

        public Geometry.Polygon Polygon;

        public Vector3 Position;

        public Geometry.Polygon SimplePolygon;

        public FioraPassive.PassiveType Type;

        #endregion

        #region Constructors and Destructors

        public QPosition(
            Vector3 position,
            FioraPassive.PassiveType type = FioraPassive.PassiveType.None,
            Geometry.Polygon polygon = null,
            Geometry.Polygon simplePolygon = null)
        {
            this.Position = position;
            this.Type = type;
            this.Polygon = polygon;
            this.SimplePolygon = simplePolygon;
        }

        #endregion
    }
}