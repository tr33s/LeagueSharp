using System;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace TreeLib.Core
{
    public class EvadeDisabler
    {
        public static int LastEvadeDisableT;
        public static int DisableDuration;
        private static bool WasEzEvadeActive;
        private static bool WasEvadeActive;

        static EvadeDisabler()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static Menu EzEvadeMenu
        {
            get { return Menu.GetMenu("ezEvade", "ezEvade"); }
        }

        private static Menu EvadeMenu
        {
            get { return Menu.GetMenu("Evade", "Evade"); }
        }

        private static MenuItem EzEvadeEnabled
        {
            get { return EzEvadeMenu.Item("DodgeSkillShots"); }
        }

        private static MenuItem EvadeEnabled
        {
            get { return EvadeMenu.Item("Enabled"); }
        }

        public static bool IsEzEvadeEnabled
        {
            get { return EzEvadeMenu != null && EzEvadeEnabled != null && EzEvadeEnabled.IsActive(); }
        }

        public static bool IsEvadeEnabled
        {
            get { return EvadeMenu != null && EvadeEnabled != null && EvadeEnabled.IsActive(); }
        }

        public static void DisableEvade(int duration = int.MaxValue)
        {
            if (IsEzEvadeEnabled)
            {
                WasEzEvadeActive = true;
                EzEvadeEnabled.SetValue(false);
            }

            if (IsEvadeEnabled)
            {
                WasEvadeActive = true;
                EvadeEnabled.SetValue(false);
            }

            DisableDuration = duration;
            LastEvadeDisableT = Utils.TickCount;
        }

        public static void EnableEvade()
        {
            if (WasEzEvadeActive && EzEvadeMenu != null && EzEvadeEnabled != null)
            {
                EzEvadeEnabled.SetValue(true);
            }

            if (WasEvadeActive && EvadeMenu != null && EvadeEnabled != null)
            {
                EvadeEnabled.SetValue(true);
            }

            WasEzEvadeActive = false;
            WasEvadeActive = false;
            DisableDuration = 0;
            LastEvadeDisableT = 0;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (DisableDuration == 0 && LastEvadeDisableT == 0)
            {
                return;
            }

            if (LastEvadeDisableT.TimeSince() >= DisableDuration)
            {
                EnableEvade();
            }
        }
    }
}