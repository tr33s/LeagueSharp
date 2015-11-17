using System;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace TreeLib.Core
{
    public class EvadeDisabler
    {
        private static int LastEvadeDisableT;
        private static int DisableDuration;
        private static bool WasEzEvadeActive;
        private static bool WasEvadeActive;

        static EvadeDisabler()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        public static bool EvadeDisabled
        {
            get { return DisableDuration != 0 && LastEvadeDisableT != 0; }
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
                var bind = EzEvadeEnabled.GetValue<KeyBind>();
                bind.Active = false;
                EzEvadeEnabled.SetValue(bind);
                WasEzEvadeActive = true;
            }

            if (IsEvadeEnabled)
            {
                var evadeBind = EvadeEnabled.GetValue<KeyBind>();
                evadeBind.Active = false;
                EvadeEnabled.SetValue(evadeBind);
                WasEvadeActive = true;
            }

            DisableDuration = duration;
            LastEvadeDisableT = Utils.TickCount;
        }

        public static void EnableEvade()
        {
            if (!EvadeDisabled)
            {
                return;
            }

            if (WasEzEvadeActive && EzEvadeMenu != null && EzEvadeEnabled != null)
            {
                var bind = EzEvadeEnabled.GetValue<KeyBind>();
                bind.Active = true;
                EzEvadeEnabled.SetValue(bind);
            }

            if (WasEvadeActive && EvadeMenu != null && EvadeEnabled != null)
            {
                var evadeBind = EvadeEnabled.GetValue<KeyBind>();
                evadeBind.Active = true;
                EvadeEnabled.SetValue(evadeBind);
            }

            WasEzEvadeActive = false;
            WasEvadeActive = false;
            DisableDuration = 0;
            LastEvadeDisableT = 0;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!EvadeDisabled)
            {
                return;
            }

            if (LastEvadeDisableT.HasTimePassed(DisableDuration))
            {
                EnableEvade();
            }
        }
    }
}