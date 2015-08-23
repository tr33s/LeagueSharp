using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace jesuisFiora
{
    internal static class SpellBlock
    {
        public static List<SpellBlockObject> BlockedSpells = new List<SpellBlockObject>();

        static SpellBlock()
        {
            const SpellSlot q = SpellSlot.Q;
            const SpellSlot w = SpellSlot.W;
            const SpellSlot e = SpellSlot.E;
            const SpellSlot r = SpellSlot.R;
            const SpellSlot N48 = (SpellSlot) 48;

            Add("Anivia", e);
            Add("Annie", q);
            Add("Alistar", w);
            Add("Azir", r);
            Add("Blitzcrank", e, "PowerFistAttack");
            Add("Brand", r);
            Add("Chogath", r);
            Add("Darius", r);
            Add("Fiddlesticks", q);
            Add("Gangplank", q);
            Add("Garen", q, "GarenQAttack");
            Add("Garen", r);
            Add("Hecarim", e, "hecarimrampattack");
            Add("Irelia", e);
            Add("Jayce", e);
            Add("LeBlanc", r);
            Add("LeeSin", r);
            Add("Leona", q, "LeonaShieldOfDaybreakAttack");
            Add("Lissandra", N48);
            Add("Lulu", w);
            Add("Maokai", w);
            Add("MonkeyKing", q, "MonkeyKingQAttack");
            Add("Mordekaiser", r);
            Add("Nasus", q, "NasusQAttack");
            Add("Nasus", w);
            Add("Nunu", e);
            Add("Malzahar", r);
            Add("Pantheon", w);
            Add("Poppy", q, "PoppyDevastatingBlow", true);
            Add("Poppy", e);
            Add("Quinn", e);
            Add("Rammus", e);
            Add("Renekton", w, "RenektonExecute");
            Add("Renekton", w, "RenektonSuperExecute");
            Add("Ryze", w);
            Add("Singed", e);
            Add("Skarner", r);
            Add("Syndra", r);
            Add("Swain", e);
            Add("TahmKench", w);
            Add("Talon", e);
            Add("Taric", e);
            Add("Teemo", q);
            Add("Tristana", r);
            Add("Trundle", q, "TrundleQ");
            Add("Trundle", r);
            Add("TwistedFate", w, "goldcardpreattack");
            Add("Udyr", e, "UdyrBearAttack");
            Add("Urgot", r);
            Add("Vayne", e);
            Add("Veigar", r);
            Add("Vi", r);
            Add("Volibear", q, "VolibearQAttack");
            Add("Volibear", w);
            Add("XinZhao", q, "XenZhaoThrust3");
            Add("XinZhao", r);
            Add("Zed", r);
        }

        public static void Initialize(Menu menu)
        {
            foreach (var unit in
                ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValid && hero.IsEnemy))
            {
                var unit1 = unit;
                foreach (var spell in BlockedSpells.Where(o => o.Name.Equals(unit1.ChampionName)))
                {
                    var name = unit.ChampionName.Equals("MonkeyKing") ? "Wukong" : unit.ChampionName;
                    var slot = spell.Slot.Equals(48) ? SpellSlot.R : spell.Slot;

                    if (spell.IsAutoAttack)
                    {
                        menu.AddBool(unit.ChampionName + "AA", "Block " + name + " " + slot + " AA");
                    }
                    else
                    {
                        menu.AddBool(unit.ChampionName, "Block " + name + " " + slot);
                    }
                }
            }
        }

        public static void Add(string name, SpellSlot slot)
        {
            BlockedSpells.Add(new SpellBlockObject(name, slot));
        }

        public static void Add(string name, SpellSlot slot, string autoAttack, bool isBuff = false)
        {
            BlockedSpells.Add(new SpellBlockObject(name, slot, autoAttack, isBuff));
        }

        public static bool Contains(Obj_AI_Hero unit, GameObjectProcessSpellCastEventArgs args)
        {
            var name = unit.ChampionName;
            var slot = unit.GetSpellSlot(args);

            foreach (var spell in BlockedSpells.Where(o => o.Name.Equals(name)))
            {
                if (!spell.IsAutoAttack || !args.SData.IsAutoAttack())
                {
                    return Program.Menu.Item(name) != null && Program.Menu.Item(name).IsActive() &&
                           spell.Slot.Equals(slot);
                }

                return Program.Menu.Item(name + "AA") != null && Program.Menu.Item(name + "AA").IsActive() &&
                       (spell.IsBuff && unit.HasBuff(spell.AutoAttackBuff)) ||
                       (spell.AutoAttackName.Equals(args.SData.Name));
            }

            return false;
        }
    }

    public class SpellBlockObject
    {
        public string AutoAttackBuff;
        public string AutoAttackName;
        public bool IsAutoAttack;
        public string Name;
        public SpellSlot Slot;

        public SpellBlockObject(string name, SpellSlot slot)
        {
            Name = name;
            Slot = slot;
        }

        public SpellBlockObject(string name, SpellSlot slot, string autoAttack, bool isBuff = false)
        {
            Name = name;
            Slot = slot;
            IsAutoAttack = true;

            if (isBuff)
            {
                AutoAttackBuff = autoAttack;
                return;
            }

            AutoAttackName = autoAttack;
        }

        public bool IsBuff
        {
            get { return !string.IsNullOrWhiteSpace(AutoAttackBuff); }
        }
    }
}