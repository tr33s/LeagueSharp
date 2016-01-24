using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.SDK;
using SharpDX;
using SkillshotType = LeagueSharp.SDK.SkillshotType;
using Spell = LeagueSharp.SDK.Spell;

namespace AIOCaster
{
    internal static class Utility
    {
        public static bool IsSkillShot(this CastType[] castTypes)
        {
            var types = new List<CastType> { CastType.Direction, CastType.Position };
            return castTypes != null && types.Any(castTypes.Contains);
        }

        public static bool IsSkillShot(this SpellType type)
        {
            return type.Equals(SpellType.SkillshotCircle) || type.Equals(SpellType.SkillshotCone) ||
                   type.Equals(SpellType.SkillshotMissileCircle) || type.Equals(SpellType.SkillshotLine) ||
                   type.Equals(SpellType.SkillshotMissileLine);
        }

        public static SkillshotType GetSkillshotType(this SpellType type)
        {
            switch (type)
            {
                case SpellType.SkillshotCircle:
                case SpellType.SkillshotMissileCircle:
                    return SkillshotType.SkillshotCircle;
                case SpellType.SkillshotCone:
                    return SkillshotType.SkillshotCone;
                case SpellType.SkillshotMissileLine:
                case SpellType.SkillshotLine:
                    return SkillshotType.SkillshotLine;
            }

            throw new Exception("SpellType prediction not found!");
        }

        public static bool IsCurrentSpell(this SpellDatabaseEntry entry)
        {
            var spell = ObjectManager.Player.Spellbook.GetSpell(entry.Slot);
            return string.Equals(spell.Name, entry.SpellName, StringComparison.CurrentCultureIgnoreCase);
        }

        public static Spell CreateSpell(this SpellDatabaseEntry entry)
        {
            try
            {
                var s = new Spell(entry.Slot, entry.Range);
                var collision = entry.CollisionObjects.Length > 1;
                var type = entry.SpellType.GetSkillshotType();

                s.SetSkillshot(entry.Delay, entry.Width, entry.MissileSpeed, collision, type);
                return s;
            }
            catch
            {
                return new Spell(SpellSlot.Unknown);
            }
        }

        public static bool IsComboMode(this OrbwalkingMode mode)
        {
            return mode == OrbwalkingMode.Combo || mode == OrbwalkingMode.Hybrid;
        }

        public static void RenderCircle(Vector3 position, float radius, Color color)
        {
            Render.Circle.DrawCircle(position, radius, color.ToSystemColor());
        }
    }
}