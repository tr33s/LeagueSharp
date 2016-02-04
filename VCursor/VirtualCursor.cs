using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using VCursor.Properties;

namespace VCursor
{
    internal static class VirtualCursor
    {
        public enum CursorIcon
        {
            Default,
            HoverEnemy,
            HoverAllyTurret,
            HoverShop,
            HoverUse,
            HoverFriendly,
            SingleTarget,
            SingleTargetAlly,
            SingleTargetEnemy,
            SingleTargetEnemyCannotAttack,
            DefaultColorblind,
            HoverEnemyColorblind,
            HoverUseColorblind,
            SingleTargetColorblind,
            SingleTargetEnemyColorblind,
            SingleTargetEnemyCannotAttackColorblind
        }

        private static readonly Render.Sprite CursorSprite;
        public static bool FollowCursor = true;
        private static CursorIcon _icon;

        static VirtualCursor()
        {
            CursorSprite = new Render.Sprite(Resources.Hand1, Vector2.Zero);
            //CurrentPath.Count > 0 ? CurrentPath.Dequeue() : Vector2.Zero;
            //FollowCursor ? Drawing.WorldToScreen(Game.CursorPos).GetRelativePosition() : Vector2.Zero

            _icon = CursorIcon.Default;
        }

        public static Vector2 Position => CursorSprite.Position;

        public static void Initialize()
        {
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            //  CursorSprite.Position = Cursor.ScreenPosition;
            var pos = Position;
            var set = false;
            if (pos.HoverShop())
            {
                SetIcon(CursorIcon.HoverShop);
                set = true;
            }

            if (pos.HoverAllyTurret())
            {
                SetIcon(CursorIcon.HoverAllyTurret);
                set = true;
            }

            if (pos.HoverEnemy())
            {
                Console.WriteLine("HOVWER ENEMY");
                SetIcon(CursorIcon.HoverEnemy);
                set = true;
            }

            if (!set)
            {
                Console.WriteLine("DEFAULT");
                SetIcon(CursorIcon.Default);
            }

            if (MouseManager.FollowingPath || Utils.TickCount - MouseManager.LastPath < 1000)
            {
                return;
            }

            SetPosition();
        }


        private static Vector2 GetRelativePosition(this Vector2 vector)
        {
            var offset = Vector2.Zero;

            if (_icon.Equals(CursorIcon.Default) || _icon.Equals(CursorIcon.DefaultColorblind))
            {
                offset = new Vector2(-8, -8);
            }
            else if (_icon.Equals(CursorIcon.HoverShop))
            {
                offset = new Vector2(-20, -15);
            }
            else if (_icon.Equals(CursorIcon.HoverEnemy))
            {
                offset = new Vector2(-2, -1);
            }

            return vector + offset;
        }

        public static void SetIcon(CursorIcon icon)
        {
            // Console.WriteLine("SET ICOn");
            Bitmap data;

            switch (icon)
            {
                case CursorIcon.Default:
                    data = Resources.Hand1;
                    break;
                case CursorIcon.HoverAllyTurret:
                    data = Resources.Hand2;
                    break;
                case CursorIcon.HoverEnemy:
                    data = Resources.HoverEnemy;
                    break;
                case CursorIcon.HoverEnemyColorblind:
                    data = Resources.HoverEnemy_Colorblind;
                    break;
                case CursorIcon.HoverFriendly:
                    data = Resources.HoverFriendly;
                    break;
                case CursorIcon.HoverShop:
                    data = Resources.HoverShop;
                    break;
                case CursorIcon.HoverUse:
                    data = Resources.HoverUse;
                    break;
                case CursorIcon.HoverUseColorblind:
                    data = Resources.HoverUse_Colorblind;
                    break;
                case CursorIcon.SingleTarget:
                    data = Resources.SingleTarget;
                    break;
                case CursorIcon.SingleTargetAlly:
                    data = Resources.SingleTargetAlly;
                    break;
                case CursorIcon.SingleTargetColorblind:
                    data = Resources.SingleTarget_Colorblind;
                    break;
                case CursorIcon.SingleTargetEnemy:
                    data = Resources.SingleTargetEnemy;
                    break;
                case CursorIcon.SingleTargetEnemyCannotAttack:
                    data = Resources.SingleTargetEnemyCannoyAttack;
                    break;
                case CursorIcon.SingleTargetEnemyCannotAttackColorblind:
                    data = Resources.SingleTargetEnemyCannoyAttack_Colorblind;
                    break;
                case CursorIcon.SingleTargetEnemyColorblind:
                    data = Resources.SingleTargetEnemy_Colorblind;
                    break;
                default:
                    data = Resources.Hand1;
                    break;
            }

            if (_icon == icon)
            {
                return;
            }

            Console.WriteLine("{0} => {1}", _icon, icon);
            _icon = icon;
            CursorSprite.UpdateTextureBitmap(data, Position);
        }

        public static void SetPosition(Vector2 position = new Vector2())
        {
            position = position.IsValid() ? position.GetRelativePosition() : Cursor.ScreenPosition.GetRelativePosition();

            if (Position.Distance(position) < 100)
            {
                CursorSprite.Position = position;
            }

            //MouseManager.StartPath(position);
        }

        public static void Draw()
        {
            CursorSprite.Add();
        }

        public static void Hide()
        {
            CursorSprite.Hide();
        }

        public static void Show()
        {
            CursorSprite.Show();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CursorSprite.Dispose();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            CursorSprite.Dispose();
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            CursorSprite.Dispose();
        }
    }
}