using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TreeLib.Extensions;

namespace jesuisFiora
{
    internal static class SpellBlock
    {
        private static readonly Dictionary<string, List<BlockedSpell>> BlockedSpells =
            new Dictionary<string, List<BlockedSpell>>();

        static SpellBlock()
        {
            const SpellSlot N48 = (SpellSlot) 48;

            var q = new BlockedSpell(SpellSlot.Q);
            var w = new BlockedSpell(SpellSlot.W);
            var e = new BlockedSpell(SpellSlot.E);
            var r = new BlockedSpell(SpellSlot.R);

            BlockedSpells.Add(
                "Aatrox",
                new List<BlockedSpell>
                {
                    new BlockedSpell
                    {
                        AutoAttackName = new[] { "AatroxWONHAttackPower", "AatroxWONHAttackLife" },
                        Name = "BloodThirst"
                    },
                    r
                });
            BlockedSpells.Add("Akali", new List<BlockedSpell> { q, w, r }); //
            BlockedSpells.Add("Alistar", new List<BlockedSpell> { q, w });
            BlockedSpells.Add("Anivia", new List<BlockedSpell> { e }); //
            BlockedSpells.Add("Annie", new List<BlockedSpell> { q }); //
            BlockedSpells.Add("Azir", new List<BlockedSpell> { r });
            BlockedSpells.Add("Bard", new List<BlockedSpell> { r });
            BlockedSpells.Add(
                "Blitzcrank",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "PowerFistAttack" }, Name = "Power Fist" }
                });
            BlockedSpells.Add("Brand", new List<BlockedSpell> { e, r });
            BlockedSpells.Add(
                "Braum",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "BraumBasicAttackPassiveOverride" }, Name = "Stun" }
                });
            BlockedSpells.Add(
                "Caitlyn",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "CaitlynHeadshotMissile" }, Name = "Headshot" },
                    r
                });
            BlockedSpells.Add("Chogath", new List<BlockedSpell> { r });
            BlockedSpells.Add(
                "Darius",
                new List<BlockedSpell>
                {
                    q,
                    new BlockedSpell { AutoAttackName = new[] { "DariusNoxianTacticsONHAttack" }, Name = "Empowered W" },
                    e,
                    r
                });
            BlockedSpells.Add(
                "Diana",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "DianaBasicAttack3" }, Name = "Moonsilver Blade" },
                    e,
                    r
                });
            BlockedSpells.Add(
                "DrMundo",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "MasochismAttack" }, Name = "Empowered E" }
                });
            BlockedSpells.Add(
                "Ekko",
                new List<BlockedSpell>
                {
                    new BlockedSpell
                    {
                        AutoAttackName = new[] { "EkkoEAttack", "ekkobasicattackp3" },
                        Name = "Empowered E"
                    }
                });
            BlockedSpells.Add("Elise", new List<BlockedSpell> { q });
            BlockedSpells.Add("Evelynn", new List<BlockedSpell> { e });
            BlockedSpells.Add("FiddleSticks", new List<BlockedSpell> { q, w, e });
            BlockedSpells.Add(
                "Fiora",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "FioraEAttack" }, Name = "Empowered First E" }
                });
            BlockedSpells.Add(
                "Fizz", new List<BlockedSpell> { q, new BlockedSpell(SpellSlot.E) { SpellName = "fizzjumptwo" } });
            BlockedSpells.Add(
                "Gangplank", new List<BlockedSpell> { q, new BlockedSpell((SpellSlot) 45) { Name = "Barrel Q" } });
            BlockedSpells.Add(
                "Garen",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "GarenQAttack" }, Name = "Empowered Q" },
                    r
                });
            BlockedSpells.Add(
                "Gnar",
                new List<BlockedSpell>
                {
                    new BlockedSpell
                    {
                        AutoAttackName = new[] { "GnarBasicAttack", "GnarBasicAttack2" },
                        AutoAttackBuff = "gnarwproc",
                        IsPlayerBuff = true,
                        Name = "Empowered W"
                    }
                });
            BlockedSpells.Add(
                "Gragas",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "DrunkenRage" }, Name = "Drunken Rage" }
                });
            BlockedSpells.Add(
                "Hecarim",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "hecarimrampattack" }, Name = "Empowered E" },
                    r
                });
            BlockedSpells.Add("Irelia", new List<BlockedSpell> { q, e });
            BlockedSpells.Add("Janna", new List<BlockedSpell> { w });
            BlockedSpells.Add(
                "JarvanIV",
                new List<BlockedSpell>
                {
                    new BlockedSpell
                    {
                        AutoAttackName = new[] { "JarvanIVMartialCadenceAttack" },
                        Name = "Martial Cadence"
                    },
                    r
                });
            BlockedSpells.Add(
                "Jax",
                new List<BlockedSpell>
                {
                    new BlockedSpell
                    {
                        AutoAttackName = new[] { "JaxBasicAttack", "JaxBasicAttack2" },
                        Name = "Empowered",
                        AutoAttackBuff = "JaxEmpowerTwo",
                        IsSelfBuff = true
                    },
                    q,
                    new BlockedSpell(SpellSlot.E) { AutoAttackBuff = "jaxpassive", IsSelfBuff = true }
                });
            BlockedSpells.Add(
                "Jayce",
                new List<BlockedSpell>
                {
                    new BlockedSpell(SpellSlot.Q) { SpellName = "JayceToTheSkies" },
                    new BlockedSpell(SpellSlot.E) { SpellName = "JayceThunderingBlow" }
                });
            BlockedSpells.Add(
                "Kassadin",
                new List<BlockedSpell>
                {
                    q,
                    new BlockedSpell { AutoAttackName = new[] { "KassadinBasicAttack3" }, Name = "Empowered W" }
                });
            BlockedSpells.Add("Katarina", new List<BlockedSpell> { e });
            BlockedSpells.Add("Kayle", new List<BlockedSpell> { q });
            BlockedSpells.Add(
                "Kennen",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "KennenMegaProc" }, Name = "Empowered" },
                    w
                });
            BlockedSpells.Add("Khazix", new List<BlockedSpell> { q });
            BlockedSpells.Add("Kindred", new List<BlockedSpell> { e });
            //new BlockedSpell((SpellSlot) 48) { SpellName = "kindredbasicattackoverridelightbombfinal", Name = "Empowered E" } });
            BlockedSpells.Add(
                "Leblanc",
                new List<BlockedSpell> { q, new BlockedSpell(SpellSlot.R) { SpellName = "LeblancChaosOrbM" } });
            BlockedSpells.Add(
                "LeeSin",
                new List<BlockedSpell>
                {
                    new BlockedSpell(SpellSlot.Q) { SpellName = "blindmonkqtwo" },
                    new BlockedSpell(SpellSlot.E) { SpellName = "BlindMonkEOne" },
                    r
                });
            BlockedSpells.Add(
                "Leona",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "LeonaShieldOfDaybreakAttack" }, Name = "Empowered Q" }
                });
            BlockedSpells.Add("Lissandra", new List<BlockedSpell> { new BlockedSpell(N48) { Name = "R" } });
            BlockedSpells.Add("Lulu", new List<BlockedSpell> { w });
            BlockedSpells.Add("Malphite", new List<BlockedSpell> { q, e });
            BlockedSpells.Add("Malzahar", new List<BlockedSpell> { e, r });
            BlockedSpells.Add("Maokai", new List<BlockedSpell> { w });
            BlockedSpells.Add(
                "MasterYi",
                new List<BlockedSpell>
                {
                    q,
                    new BlockedSpell { AutoAttackName = new[] { "MasterYiDoubleStrike" }, Name = "Empowered" }
                });
            BlockedSpells.Add("MissFortune", new List<BlockedSpell> { q });
            BlockedSpells.Add(
                "MonkeyKing",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "MonkeyKingQAttack" }, Name = "Empowered Q" },
                    e
                });
            BlockedSpells.Add(
                "Mordekaiser",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "mordekaiserqattack2" }, Name = "Empowered Q" },
                    r
                });
            BlockedSpells.Add("Nami", new List<BlockedSpell> { w });
            BlockedSpells.Add(
                "Nasus",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "NasusQAttack" }, Name = "Empowered Q" },
                    w
                });
            BlockedSpells.Add(
                "Nautilus",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "NautilusRavageStrikeAttack" }, Name = "Empowered" },
                    e,
                    r
                });
            BlockedSpells.Add(
                "Nidalee",
                new List<BlockedSpell>
                {
                    new BlockedSpell
                    {
                        AutoAttackName = new[] { "NidaleeTakedownAttack" },
                        ModelName = "nidalee_cougar",
                        Name = "Empowered Cougar Q"
                    },
                    new BlockedSpell(SpellSlot.W)
                    {
                        ModelName = "nidalee_cougar",
                        AutoAttackBuff = "nidaleepassivehunted",
                        IsPlayerBuff = true,
                        Name = "Cougar W"
                    }
                });
            BlockedSpells.Add("Nocturne", new List<BlockedSpell> { r });
            BlockedSpells.Add("Nunu", new List<BlockedSpell> { e, new BlockedSpell(SpellSlot.R) { SpellName = "" } });
            BlockedSpells.Add("Olaf", new List<BlockedSpell> { e });
            BlockedSpells.Add("Pantheon", new List<BlockedSpell> { q, w });
            BlockedSpells.Add(
                "Poppy",
                new List<BlockedSpell>
                {
                    new BlockedSpell
                    {
                        AutoAttackName = new[] { "PoppyBasicAttack", "PoppyBasicAttack2" },
                        Name = "Empowered Q",
                        AutoAttackBuff = "PoppyDevastatingBlow",
                        IsSelfBuff = true
                    },
                    e
                });
            BlockedSpells.Add(
                "Quinn",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "QuinnWEnhanced" }, Name = "Empowered" },
                    e
                });
            BlockedSpells.Add("Rammus", new List<BlockedSpell> { q, e });
            BlockedSpells.Add("Rek'Sai", new List<BlockedSpell> { w, e });
            BlockedSpells.Add(
                "Renekton",
                new List<BlockedSpell>
                {
                    q,
                    new BlockedSpell
                    {
                        AutoAttackName = new[] { "RekentonExecute", "RenektonSuperExecute" },
                        Name = "Empowered W"
                    }
                });
            BlockedSpells.Add(
                "Rengar",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "RengarBasicAttack" }, Name = "Empowered Q" }
                });
            BlockedSpells.Add("Riven", new List<BlockedSpell> { q }); //
            BlockedSpells.Add("Ryze", new List<BlockedSpell> { w, e });
            BlockedSpells.Add("Shaco", new List<BlockedSpell> { q, e });
            BlockedSpells.Add("Shen", new List<BlockedSpell> { q });
            BlockedSpells.Add(
                "Shyvana",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "ShyvanaDoubleAttackHit" }, Name = "Empowered Q" }
                });
            BlockedSpells.Add("Singed", new List<BlockedSpell> { e });
            BlockedSpells.Add("Sion", new List<BlockedSpell> { q, r });
            BlockedSpells.Add("Skarner", new List<BlockedSpell> { r });
            BlockedSpells.Add("Syndra", new List<BlockedSpell> { r });
            BlockedSpells.Add("Swain", new List<BlockedSpell> { q, e });
            BlockedSpells.Add("TahmKench", new List<BlockedSpell> { w });
            /*BlockedSpells.Add(
                "Talon",
                new List<BlockedSpell> { new BlockedSpell { AutoAttackName = new[] { "-1" }, Name = "Empowered Q" }, e });*/
            BlockedSpells.Add("Taric", new List<BlockedSpell> { e });
            BlockedSpells.Add("Teemo", new List<BlockedSpell> { q });
            BlockedSpells.Add("Tristana", new List<BlockedSpell> { e, r });
            BlockedSpells.Add(
                "Trundle",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "TrundleQ" }, Name = "Empowered Q" },
                    r
                });
            BlockedSpells.Add(
                "TwistedFate",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "goldcardpreattack" }, Name = "Gold Card" }
                });
            BlockedSpells.Add(
                "Udyr",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "UdyrBearAttack" }, Name = "Bear" }
                });
            BlockedSpells.Add("Urgot", new List<BlockedSpell> { r });
            BlockedSpells.Add(
                "Vayne",
                new List<BlockedSpell>
                {
                    //new BlockedSpell { AutoAttackName = new[] { "-1" }, Name = "Silver Bolts" },
                    e
                });
            BlockedSpells.Add("Veigar", new List<BlockedSpell> { r });
            /*BlockedSpells.Add(
                "Vi",
                new List<BlockedSpell> { new BlockedSpell { AutoAttackName = new[] { "-1" }, Name = "Empowered E" }, r });*/
            /*BlockedSpells.Add(
                "Viktor",
                new List<BlockedSpell> { new BlockedSpell { AutoAttackName = new[] { "-1" }, Name = "Empowered Q" } });*/
            BlockedSpells.Add("Vladimir", new List<BlockedSpell> { r });
            BlockedSpells.Add(
                "Volibear",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "VolibearQAttack" }, Name = "Empowered Q" },
                    w
                });
            BlockedSpells.Add("Warick", new List<BlockedSpell> { q });
            BlockedSpells.Add(
                "XinZhao",
                new List<BlockedSpell>
                {
                    new BlockedSpell { AutoAttackName = new[] { "XenZhaoThrust3" }, Name = "Empowered Q" },
                    e,
                    r
                });
            BlockedSpells.Add("Yasuo", new List<BlockedSpell> { e });
           /* BlockedSpells.Add(
                "Yorick",
                new List<BlockedSpell> { new BlockedSpell { AutoAttackName = new[] { "-1" }, Name = "Empowered Q" }, e });*/
            BlockedSpells.Add("Zac", new List<BlockedSpell> { w, r });
            BlockedSpells.Add("Zilean", new List<BlockedSpell> { e });
        }

        public static void Initialize(Menu menu)
        {
            var enemies = HeroManager.Enemies;

            if (enemies.Any(o => o.ChampionName.Equals("Kalista")))
            {
                menu.AddBool("Oathsworn", "Block Oathsworn Knockup (Kalista R)");
            }

            foreach (var unit in
                enemies)
            {
                if (!BlockedSpells.ContainsKey(unit.ChampionName))
                {
                    continue;
                }

                var name = unit.ChampionName.Equals("MonkeyKing") ? "Wukong" : unit.ChampionName;
                var blockedMenu = menu.AddMenu(unit.ChampionName, name);
                foreach (var spell in BlockedSpells[unit.ChampionName])
                {
                    var slot = spell.Slot.Equals(48) ? SpellSlot.R : spell.Slot;
                    var menuName = unit.ChampionName;
                    var display = "Block ";

                    if (!string.IsNullOrWhiteSpace(spell.Name))
                    {
                        if (spell.IsAutoAttack)
                        {
                            menuName += "AA";
                            display += spell.Name + " AA";
                        }
                        else
                        {
                            menuName += slot;
                            display += spell.Name;
                        }
                    }
                    else
                    {
                        menuName += slot;
                        display += spell.Slot;
                    }

                    blockedMenu.AddBool(menuName, display);
                }
            }
        }

        public static bool Contains(Obj_AI_Hero unit, GameObjectProcessSpellCastEventArgs args)
        {
            var name = unit.ChampionName;
            var slot = args.Slot.Equals(48) ? SpellSlot.R : args.Slot;

            if (args.SData.Name.Equals("KalistaRAllyDash") && Program.Menu.Item("Oathsworn").IsActive())
            {
                return true;
            }

            if (!BlockedSpells.ContainsKey(name))
            {
                return false;
            }

            foreach (var spell in
                BlockedSpells[unit.ChampionName])
            {
                if (spell.HasModelCondition && !spell.CorrectModel(unit))
                {
                    continue;
                }

                if (spell.HasSpellCondition && !spell.SpellName.Equals(args.SData.Name))
                {
                    continue;
                }
                if (spell.HasBuffCondition)
                {
                    if (spell.IsSelfBuff && !unit.HasBuff(spell.AutoAttackBuff))
                    {
                        continue;
                    }

                    if (spell.IsPlayerBuff && !ObjectManager.Player.HasBuff(spell.AutoAttackBuff))
                    {
                        continue;
                    }
                }

                if (spell.IsAutoAttack)
                {
                    if (!args.SData.IsAutoAttack())
                    {
                        continue;
                    }

                    if (spell.AutoAttackName.Any(s => s.Equals("-1")))
                    {
                        Console.WriteLine(args.SData.Name);
                    }

                    var condition = spell.AutoAttackName.Any(s => s.Equals(args.SData.Name));

                    if (unit.ChampionName.Equals("Gnar"))
                    {
                        var buff = ObjectManager.Player.Buffs.FirstOrDefault(b => b.Name.Equals("gnarwproc"));
                        condition = condition && buff != null && buff.Count == 2;
                    }
                    if (unit.ChampionName.Equals("Rengar"))
                    {
                        condition = condition && unit.Mana.Equals(5);
                    }

                    condition = condition && Program.Menu.Item(name + "AA") != null &&
                                Program.Menu.Item(name + "AA").IsActive();

                    // Console.WriteLine("CC: " + condition);
                    if (condition)
                    {
                        return true;
                    }

                    continue;
                }

                if (Program.Menu.Item(name + slot) == null || !Program.Menu.Item(name + slot).IsActive() ||
                    !spell.Slot.Equals(slot))
                {
                    continue;
                }
                Console.WriteLine("{0} {1}", args.Slot, args.SData.Name);
                // is the buff not always applied? =_//
                if (name.Equals("Riven"))
                {
                    var buff = unit.Buffs.FirstOrDefault(b => b.Name.Equals("RivenTriCleave"));
                    if (buff != null && buff.Count == 3)
                    {
                        return true;
                    }
                }

                // Console.WriteLine(slot + " " + args.SData.Name);
                return true;
            }
            return false;
        }
    }

    public class BlockedSpell
    {
        public string AutoAttackBuff;
        public string[] AutoAttackName = { };
        public string CustomDisplayName;
        public bool Enabled;
        public bool IsPlayerBuff;
        public bool IsSelfBuff;
        public string ModelName;
        public string Name;
        public SpellSlot Slot;
        public string SpellName;
        public BlockedSpell() {}

        public BlockedSpell(SpellSlot slot)
        {
            Slot = slot;
        }

        public BlockedSpell(string name, SpellSlot slot, bool enabled = true)
        {
            Name = name;
            Slot = slot;
            Enabled = enabled;
        }

        public bool IsAutoAttack => AutoAttackName.Any(s => !string.IsNullOrWhiteSpace(s));
        public bool HasBuffCondition => !string.IsNullOrWhiteSpace(AutoAttackBuff);
        public bool HasModelCondition => !string.IsNullOrWhiteSpace(ModelName);
        public bool HasSpellCondition => !string.IsNullOrWhiteSpace(SpellName);

        public bool CorrectModel(Obj_AI_Hero hero)
        {
            return hero.CharData.BaseSkinName.Equals(ModelName);
        }
    }
}