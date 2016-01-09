using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Windows;

namespace LeBlanc
{

    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal static class Ad
    {
        private static Render.Sprite AdSprite;
        private static bool threadOpened;
        private static int adDisplayed;

        private static Vector2 AdPosition
        {
            get
            {
                return ScreenCenter - AdSize / 2;
            }
        }

        private static readonly Vector2 AdSize = new Vector2(1024, 1024);
        private static Vector2 ScreenCenter
        {
            get { return new Vector2(Drawing.Width / 2f, Drawing.Height / 2f);}
        }

        public static void Initialize()
        {
            AdSprite = new Render.Sprite(Properties.Resources.PopBlanc, AdPosition);
            AdSprite.Add();

            adDisplayed = LeagueSharp.Common.Utils.TickCount;

            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
        }


        private static void Game_OnUpdate(EventArgs args)
        {
            if (LeagueSharp.Common.Utils.TickCount - adDisplayed <= 7000)
            {
                return;
            }

            AdSprite.Dispose();
            Game.OnUpdate -= Game_OnUpdate;
            Game.OnWndProc -= Game_OnWndProc;
        }


        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (threadOpened || args.Msg != (uint) WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            var pos = LeagueSharp.Common.Utils.GetCursorPos();
            var adPos = AdPosition;

            if (!LeagueSharp.Common.Utils.IsUnderRectangle(pos, adPos.X, adPos.Y,  AdSize.X, AdSize.Y))
            {
                return;
            }
            
            System.Diagnostics.Process.Start("https://www.joduska.me/forum/topic/165735-");
            threadOpened = true;
        }

    }
}
