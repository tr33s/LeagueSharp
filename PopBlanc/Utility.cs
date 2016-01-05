using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace PopBlanc
{
    internal static class Utility
    {
        public static bool HasEBuff(this Obj_AI_Base unit)
        {
            return unit.HasBuff("leblancsoulshackle");
        }

        public static bool HasQBuff(this Obj_AI_Base unit)
        {
            return unit.HasBuff("leblancchaosorb");
        }

        public static bool HasQRBuff(this Obj_AI_Base unit)
        {
            return unit.HasBuff("leblancchaosorbm");
        }
    }

    public class WBackPosition
    {
        public static List<WBackPosition> Positions = new List<WBackPosition>();
        private readonly GameObject obj;
        private readonly int spawnTime;
        public int EndTime;

        public WBackPosition(GameObject obj)
        {
            this.obj = obj;
            spawnTime = Utils.TickCount;
            EndTime = spawnTime + 4200;
            Positions.Add(this);
        }

        public static void Initialize()
        {
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var w in Positions.Where(p => p.obj.IsValid))
            {
                var pos = Drawing.WorldToScreen(w.obj.Position);
                var time = (w.EndTime - Utils.TickCount) / 1000f;
                Drawing.DrawText(pos.X - 10, pos.Y - 100, Color.Red, string.Format("{0:0.0}", time));
                Render.Circle.DrawCircle(w.obj.Position, 150, Color.Red);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || sender.DistanceToPlayer() > SpellManager.W.Range ||
                !sender.Name.Contains("return_indicator"))
            {
                return;
            }

            Console.WriteLine("ADD " + sender.Name);
            new WBackPosition(sender);
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            Positions.RemoveAll(p => p.obj.NetworkId == sender.NetworkId);
        }
    }
}