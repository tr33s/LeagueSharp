using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace VCursor
{
    internal static class MouseManager
    {
        private static MousePath _currentPath;

        static MouseManager()
        {
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnIssueOrder += Obj_AI_Base_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        public static int LastPath { get; private set; }

        public static bool FollowingPath
        {
            get { return _currentPath != null && !_currentPath.Finished; }
        }

        private static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender == null || !sender.HasSpellCaster || !sender.Owner.IsMe)
            {
                return;
            }

            var target = args.Target as Obj_AI_Base;

            // target offscreen
            if (target != null)
            {
                if (!target.IsHPBarRendered)
                {
                    return;
                }

                StartPathWorld(target.ServerPosition);
                return;
            }


            StartPathWorld(args.StartPosition);
        }

        private static void Obj_AI_Base_OnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.IsMe)
            {
                return;
            }

            var target = args.Target as Obj_AI_Base;

            // target offscreen
            if (target != null && !target.IsHPBarRendered)
            {
                return;
            }

            StartPathWorld(args.TargetPosition);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!FollowingPath || !_currentPath.CanMove)
            {
                return;
            }

            var point = _currentPath.NextPoint;

            if (point == Vector2.Zero)
            {
                return;
            }

            /*if (_currentPath.Finished)
            {
                VirtualCursor.SetIcon(_currentPath.FinalIcon);
            }*/

            if (point.HoverShop())
            {
                VirtualCursor.SetIcon(VirtualCursor.CursorIcon.HoverShop);
            }
            else if (point.HoverAllyTurret())
            {
                VirtualCursor.SetIcon(VirtualCursor.CursorIcon.HoverAllyTurret);
            }
            else if (point.HoverEnemy())
            {
                VirtualCursor.SetIcon(VirtualCursor.CursorIcon.HoverEnemy);
            }

            VirtualCursor.SetPosition(point);
        }

        public static void StartPath(Vector2 start, Vector2 end)
        {
            if (Utils.TickCount - LastPath < 200)
            {
                return;
            }


            if (FollowingPath)
            {
                CancelPath();
            }

            VirtualCursor.SetIcon(VirtualCursor.CursorIcon.Default);
            LastPath = Utils.TickCount;
            _currentPath = new MousePath(start, end);
        }

        public static void StartPath(Vector2 end)
        {
            _currentPath = new MousePath(VirtualCursor.Position, end);
        }

        public static void StartPathWorld(Vector3 end)
        {
            _currentPath = new MousePath(VirtualCursor.Position, end.ToScreenPoint());
        }

        public static void CancelPath()
        {
            _currentPath = null;
        }

        internal class MousePath
        {
            private readonly Queue<MousePoint> _path;
            private readonly int _startTime;
            public VirtualCursor.CursorIcon FinalIcon;

            public MousePath(Vector2 start, Vector2 end)
            {
                _path = GeneratePath(start, end);
                _startTime = Utils.TickCount;

                if (end.HoverShop())
                {
                    FinalIcon = VirtualCursor.CursorIcon.HoverShop;
                }
                else if (end.HoverAllyTurret())
                {
                    FinalIcon = VirtualCursor.CursorIcon.HoverAllyTurret;
                }
                else if (end.HoverEnemy())
                {
                    FinalIcon = VirtualCursor.CursorIcon.HoverEnemy;
                }
            }

            private MousePoint NextPointPeek
            {
                get { return _path.Peek(); }
            }

            public Vector2 NextPoint
            {
                get { return _path.Dequeue().Point; }
            }

            public bool Finished
            {
                get { return _path.Count == 0; }
            }

            public bool CanMove
            {
                get { return !Finished && Utils.TickCount - _startTime > NextPointPeek.Time; }
            }

            private static Queue<MousePoint> GeneratePath(Vector2 start, Vector2 end)
            {
                //return PathGenerator.GeneratePath(start.ToWorldPoint().To2D(), end.ToWorldPoint().To2D());
                var d = start.Distance(end);

                if (d < 75)
                {
                    return new Queue<MousePoint>();
                }

                const int t = 25;
                var path = new Queue<MousePoint>();
                var pause = 0;
                for (var i = 0; i < d; i += (int) d / 20)
                {
                    if (i > d)
                    {
                        break;
                    }

                    var count = path.Count;
                    if (count > 0 && count % 15 == 0)
                    {
                        pause = 10;
                    }
                    else
                    {
                        pause = 1;
                    }

                    var point = start.Extend(end, i);
                    if (count % 10 == 0)
                    {
                        point.Randomize(10, 50);
                    }

                    path.Enqueue(new MousePoint(point, pause));
                }

                path.Enqueue(new MousePoint(end, 0));
                return path;
            }
        }

        internal class MousePoint
        {
            public Vector2 Point;
            public int Time;

            public MousePoint(Vector2 point, int time)
            {
                Point = point;
                Time = time;
            }
        }
    }
}