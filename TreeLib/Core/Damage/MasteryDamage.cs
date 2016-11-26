using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace TreeLib.Core.Damage
{
    public static class MasteryDamage
    {
        public static Mastery ThunderLords;
        public static Mastery Assasssin;

        public static Mastery DoubleEdgedSword;

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        public static void Initialize()
        {
            Assasssin = Player.GetMastery((MasteryData.Cunning) 83);
            DoubleEdgedSword = Player.GetMastery(MasteryData.Ferocity.DoubleEdgedSword);
            ThunderLords = Player.GetMastery(MasteryData.Cunning.ThunderlordsDecree);
        }

        public static float GetMasteryDamage()
        {
            var d = 0f;

            if (ThunderLords.IsValid() && !Player.HasBuff("masterylordsdecreecooldown"))
            {
                d += 10 * Player.Level + .3f * Player.FlatPhysicalDamageMod + .1f * Player.TotalMagicalDamage;
            }

            return d;
        }

        public static float GetMasteryModifier()
        {
            var modifier = 1f;

            if (DoubleEdgedSword.IsValid())
            {
                modifier += .03f;
            }

            if (Assasssin.IsValid() && Player.CountAlliesInRange(800) == 0)
            {
                modifier += .02f;
            }

            return modifier;
        }

        public static bool IsValid(this Mastery mastery)
        {
            return mastery != null && mastery.IsActive();
        }
    }
}