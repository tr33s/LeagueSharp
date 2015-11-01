using LeagueSharp.Common;
using TreeLib.Managers;
using TreeLib.SpellData;

namespace TreeLib.Core
{
    public static class Bootstrap
    {
        public static Menu Menu;
        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            Menu = new Menu("TreeLib", "TreeLib", true);
            Menu.AddToMainMenu();
            SpellManager.Initialize();
            CustomEvents.Game.OnGameLoad += args => { Evade.Init(); };
        }
    }
}