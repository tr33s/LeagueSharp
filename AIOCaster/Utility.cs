using LeagueSharp.Common;
using LeagueSharp.SDK.Core.Enumerations;
using SkillshotType = LeagueSharp.Common.SkillshotType;

namespace AIOCaster
{
    internal static class Utility
    {
        public static bool IsSkillShot(this SpellType type)
        {
            return type.Equals(SpellType.SkillshotCircle) || type.Equals(SpellType.SkillshotCone) ||
                   type.Equals(SpellType.SkillshotMissileCone) || type.Equals(SpellType.SkillshotLine) ||
                   type.Equals(SpellType.SkillshotMissileLine);
        }

        public static SkillshotType GetSkillshotType(this SpellType type)
        {
            switch (type)
            {
                case SpellType.SkillshotCircle:
                    return SkillshotType.SkillshotCircle;
                case SpellType.SkillshotCone:
                case SpellType.SkillshotMissileCone:
                    return SkillshotType.SkillshotCone;
                case SpellType.SkillshotMissileLine:
                case SpellType.SkillshotLine:
                    return SkillshotType.SkillshotLine;
            }

            return SkillshotType.SkillshotLine;
        }

        public static bool IsComboMode(this Orbwalking.OrbwalkingMode mode)
        {
            return mode.Equals(Orbwalking.OrbwalkingMode.Combo) || mode.Equals(Orbwalking.OrbwalkingMode.Mixed);
        }
    }
}