using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Input;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Rectangle = SharpDX.Rectangle;

namespace AutoLeveler
{
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    internal class SpriteBox
    {
        private Vector2 clickOffset;
        public bool IsMovable = true;
        private bool isMoving;
        public Render.Sprite MainSprite;
        public List<SpriteObject> SpriteList = new List<SpriteObject>();

        public SpriteBox(Bitmap bitmap, Vector2 position)
        {
            MainSprite = new Render.Sprite(bitmap, position)
            {
                VisibleCondition = sender => Key.LeftShift.IsKeyPressed() || Key.RightShift.IsKeyPressed()
            };

            Game.OnUpdate += Game_OnGameUpdate;
            Game.OnWndProc += Game_OnWndProc;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public Vector2 Position
        {
            get { return MainSprite.Position; }
            set { MainSprite.Position = value; }
        }

        public Rectangle Rectangle
        {
            get { return new Rectangle((int) Position.X, (int) Position.Y, (int) Width, (int) Height); }
        }

        public float Width
        {
            get { return MainSprite.Width; }
        }

        public float Height
        {
            get { return MainSprite.Height; }
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            if (isMoving)
            {
                SetRelativePosition(Utils.GetCursorPos());
            }
        }

        public void SetRelativePosition(Vector2 position)
        {
            var pos = position - clickOffset;
            MainSprite.Position = pos;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Dispose();
        }

        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            Dispose();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Dispose();
        }

        private void Dispose()
        {
            foreach (var sprite in SpriteList)
            {
                sprite.Sprite.Dispose();
            }

            MainSprite.Dispose();
        }

        private void Game_OnWndProc(WndEventArgs args)
        {
            if (!IsMovable)
            {
                return;
            }

            if (isMoving && args.Msg == (uint) WindowsMessages.WM_LBUTTONUP)
            {
                isMoving = false;
                var menu = Program.Menu;
                Program.Menu.Item("X").SetValue(new Slider((int) Position.X, 0, Drawing.Width));
                Program.Menu.Item("Y").SetValue(new Slider((int) Position.Y, 0, Drawing.Height));
            }

            if (args.Msg != (uint) WindowsMessages.WM_LBUTTONDOWN)
            {
                return;
            }

            var pos = Utils.GetCursorPos();

            // main sprite move area
            if (pos.X > Rectangle.X && pos.X <= Rectangle.X + 41 && pos.Y > Rectangle.Y && pos.Y <= Rectangle.Y + 20)
            {
                isMoving = true;
                clickOffset = new Vector2(Math.Abs(pos.X - Position.X), Math.Abs(pos.Y - Position.Y));
                return;
            }

            // level buttons
            if (pos.X > Rectangle.X + 40 && pos.X <= Rectangle.X + 40 + 19 * 18 && pos.Y > Rectangle.Y &&
                pos.Y <= Rectangle.Y + 20)
            {
                var lvl = (int) Math.Floor((pos.X - Rectangle.X - 40) / 19);
                var sprite = SpriteList.FirstOrDefault(o => o.Level.Equals(lvl));

                if (sprite != null && (Program.EditingMode || ObjectManager.Player.Level <= lvl))
                {
                    sprite.IncreasePosition();
                }
            }
        }

        public void Draw()
        {
            MainSprite.Add(-1);
        }

        public void AddSprite(SpriteObject sprite)
        {
            sprite.Sprite.VisibleCondition = sender => MainSprite.Visible;
            sprite.Sprite.PositionUpdate = () => Position + sprite.Offset;
            sprite.Sprite.Add(1);
            SpriteList.Add(sprite);
        }
    }

    public class SpriteObject
    {
        public int Level;
        public Vector2 Offset;
        public Render.Sprite Sprite;

        public SpriteObject(Bitmap bitmap, Vector2 offset, int level)
        {
            Sprite = new Render.Sprite(bitmap, Vector2.Zero);
            Offset = offset;
            Level = level;
        }

        public void SetPosition(Vector2 position)
        {
            Sprite.Position = position + Offset;
        }

        public void IncreasePosition()
        {
            if (Offset.Y >= 22 + 19 * 3)
            {
                Offset.Y = 22;
            }
            else
            {
                Offset.Y += 19;
            }

            Program.UpdateSequence(Level, (int) (Offset.Y - 22) / 19);
        }
    }
}