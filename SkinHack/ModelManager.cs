using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using LeagueSharp;
using LeagueSharp.Common;

namespace SkinHack
{
    internal static class ModelManager
    {
        private const string DataDragonBase = "http://ddragon.leagueoflegends.com/";
        private static readonly string GameVersion;
        private static readonly Dictionary<int, Model> ObjectList = new Dictionary<int, Model>();

        #region ModelList

        public static List<string> ModelList = new List<string>
        {
            "Aatrox",
            "Ahri",
            "Akali",
            "Alistar",
            "Amumu",
            "AncientGolem",
            "Anivia",
            "AniviaEgg",
            "AniviaIceblock",
            "Annie",
            "AnnieTibbers",
            "ARAMChaosNexus",
            "ARAMChaosTurretFront",
            "ARAMChaosTurretInhib",
            "ARAMChaosTurretNexus",
            "ARAMChaosTurretShrine",
            "ARAMOrderNexus",
            "ARAMOrderTurretFront",
            "ARAMOrderTurretInhib",
            "ARAMOrderTurretNexus",
            "ARAMOrderTurretShrine",
            "AramSpeedShrine",
            "AscRelic",
            "AscWarpIcon",
            "AscXerath",
            "Ashe",
            "Azir",
            "AzirSoldier",
            "AzirSunDisc",
            "AzirTowerClicker",
            "AzirUltSoldier",
            "Bard",
            "BardFollower",
            "BardHealthShrine",
            "BardPickup",
            "BardPickupNoIcon",
            "BardPortalClickable",
            "BilgeLaneCannon_Chaos",
            "BilgeLaneCannon_Order",
            "BilgeLaneMelee_Chaos",
            "BilgeLaneMelee_Order",
            "BilgeLaneRanged_Chaos",
            "BilgeLaneRanged_Order",
            "Blitzcrank",
            "BlueTrinket",
            "Blue_Minion_Basic",
            "Blue_Minion_MechCannon",
            "Blue_Minion_MechMelee",
            "Blue_Minion_Wizard",
            "Brand",
            "Braum",
            "brush_A_SR",
            "brush_BW_A",
            "brush_BW_B",
            "brush_BW_C",
            "brush_BW_D",
            "brush_BW_E",
            "brush_BW_F",
            "brush_BW_G",
            "brush_BW_H",
            "brush_BW_I",
            "brush_BW_J",
            "brush_B_SR",
            "brush_CS_A",
            "brush_CS_B",
            "brush_CS_C",
            "brush_CS_D",
            "brush_CS_E",
            "brush_CS_F",
            "brush_CS_G",
            "brush_CS_H",
            "brush_CS_I",
            "brush_CS_J",
            "brush_C_SR",
            "brush_D_SR",
            "brush_E_SR",
            "brush_F_SR",
            "brush_HA_A",
            "brush_HA_B",
            "brush_HA_C",
            "brush_HA_D",
            "brush_HA_E",
            "brush_HA_F",
            "brush_HA_G",
            "brush_HA_H",
            "brush_HA_I",
            "brush_HA_J",
            "brush_SRU_A",
            "brush_SRU_B",
            "brush_SRU_C",
            "brush_SRU_D",
            "brush_SRU_E",
            "brush_SRU_F",
            "brush_SRU_G",
            "brush_SRU_H",
            "brush_SRU_I",
            "brush_SRU_J",
            "brush_TT_A",
            "brush_TT_B",
            "brush_TT_C",
            "brush_TT_D",
            "brush_TT_E",
            "brush_TT_F",
            "brush_TT_G",
            "brush_TT_H",
            "brush_TT_I",
            "brush_TT_J",
            "brush_TT_K",
            "brush_TT_L",
            "brush_TT_M",
            "brush_TT_N",
            "brush_TT_O",
            "brush_TT_P",
            "brush_TT_Q",
            "brush_TT_R",
            "brush_TT_S",
            "brush_TT_T",
            "brush_TT_U",
            "BW_anclantern",
            "BW_AP_Bubbs",
            "BW_AP_ChaosTurret",
            "BW_AP_ChaosTurret2",
            "BW_AP_ChaosTurret3",
            "BW_AP_ChaosTurretRubble",
            "BW_AP_Finn",
            "BW_AP_OrderTurret",
            "BW_AP_OrderTurret2",
            "BW_AP_OrderTurret3",
            "BW_AP_OrderTurretRubble",
            "BW_boat",
            "Bw_Bottle",
            "bw_brdgdoor",
            "BW_dblrope",
            "BW_fishhk",
            "Bw_Hcannon",
            "BW_Ironback",
            "BW_Lantern",
            "BW_Ocklepod",
            "BW_Plundercrab",
            "BW_Razorfin",
            "BW_seagull",
            "BW_shadowshark",
            "BW_shortrope",
            "BW_shrkhk",
            "BW_signa",
            "BW_signc",
            "Bw_Squid",
            "bw_statlant",
            "bw_tooth",
            "BW_vship",
            "Caitlyn",
            "CaitlynTrap",
            "Cassiopeia",
            "Cassiopeia_Death",
            "ChaosInhibitor",
            "ChaosInhibitor_D",
            "ChaosNexus",
            "ChaosTurretGiant",
            "ChaosTurretNormal",
            "ChaosTurretShrine",
            "ChaosTurretTutorial",
            "ChaosTurretWorm",
            "ChaosTurretWorm2",
            "Chogath",
            "Corki",
            "crystal_platform",
            "Darius",
            "DestroyedInhibitor",
            "DestroyedNexus",
            "DestroyedTower",
            "Diana",
            "Dragon",
            "Draven",
            "DrMundo",
            "Ekko",
            "Elise",
            "EliseSpider",
            "EliseSpiderling",
            "Evelynn",
            "Ezreal",
            "Ezreal_cyber_1",
            "Ezreal_cyber_2",
            "Ezreal_cyber_3",
            "FiddleSticks",
            "Fiora",
            "Fizz",
            "FizzBait",
            "FizzShark",
            "Galio",
            "Gangplank",
            "GangplankBarrel",
            "Garen",
            "GhostWard",
            "GiantWolf",
            "Gnar",
            "GnarBig",
            "Golem",
            "GolemODIN",
            "Gragas",
            "Graves",
            "GreatWraith",
            "HABW_banner",
            "HABW_Lantern",
            "HA_AP_BannerMidBridge",
            "HA_AP_BridgeLaneStatue",
            "HA_AP_Chains",
            "HA_AP_Chains_Long",
            "HA_AP_ChaosTurret",
            "HA_AP_ChaosTurret2",
            "HA_AP_ChaosTurret3",
            "HA_AP_ChaosTurretRubble",
            "HA_AP_ChaosTurretShrine",
            "HA_AP_ChaosTurretTutorial",
            "HA_AP_Cutaway",
            "HA_AP_HealthRelic",
            "HA_AP_Hermit",
            "HA_AP_Hermit_Robot",
            "HA_AP_HeroTower",
            "HA_AP_OrderCloth",
            "HA_AP_OrderShrineTurret",
            "HA_AP_OrderTurret",
            "HA_AP_OrderTurret2",
            "HA_AP_OrderTurret3",
            "HA_AP_OrderTurretRubble",
            "HA_AP_OrderTurretTutorial",
            "HA_AP_PeriphBridge",
            "HA_AP_Poro",
            "HA_AP_PoroSpawner",
            "HA_AP_ShpNorth",
            "HA_AP_ShpSouth",
            "HA_AP_Viking",
            "HA_ChaosMinionMelee",
            "HA_ChaosMinionRanged",
            "HA_ChaosMinionSiege",
            "HA_ChaosMinionSuper",
            "HA_FB_HealthRelic",
            "HA_OrderMinionMelee",
            "HA_OrderMinionRanged",
            "HA_OrderMinionSiege",
            "HA_OrderMinionSuper",
            "Hecarim",
            "Heimerdinger",
            "HeimerTBlue",
            "HeimerTYellow",
            "HeroSpawnOffsets.inibin",
            "Irelia",
            "Janna",
            "JarvanIV",
            "JarvanIVStandard",
            "JarvanIVWall",
            "Jax",
            "Jayce",
            "Jinx",
            "JinxMine",
            "Kalista",
            "KalistaAltar",
            "KalistaSpawn",
            "Karma",
            "Karthus",
            "Kassadin",
            "Katarina",
            "Kayle",
            "Kennen",
            "Khazix",
            "Kindred",
            "KindredJungleBountyMinion",
            "KindredWolf",
            "KingPoro",
            "KINGPORO_HiddenUnit",
            "KINGPORO_PoroFollower",
            "KogMaw",
            "KogMawDead",
            "Leblanc",
            "LeeSin",
            "Leona",
            "LesserWraith",
            "Lissandra",
            "Lizard",
            "LizardElder",
            "Lucian",
            "Lulu",
            "LuluCupcake",
            "LuluDragon",
            "LuluFaerie",
            "LuluKitty",
            "LuluLadybug",
            "LuluPig",
            "LuluSeal",
            "LuluSnowman",
            "LuluSquill",
            "Lux",
            "Malphite",
            "Malzahar",
            "MalzaharVoidling",
            "Maokai",
            "MaokaiSproutling",
            "MasterYi",
            "MissFortune",
            "MonkeyKing",
            "MonkeyKingClone",
            "MonkeyKingFlying",
            "Mordekaiser",
            "Morgana",
            "Nami",
            "Nasus",
            "NasusUlt",
            "Nautilus",
            "Nidalee",
            "Nidalee_Cougar",
            "Nidalee_Spear",
            "Nocturne",
            "Nunu",
            "OdinBlueSuperminion",
            "OdinCenterRelic",
            "OdinChaosTurretShrine",
            "OdinClaw",
            "OdinCrane",
            "OdinMinionGraveyardPortal",
            "OdinMinionSpawnPortal",
            "OdinNeutralGuardian",
            "OdinOpeningBarrier",
            "OdinOrderTurretShrine",
            "OdinQuestBuff",
            "OdinQuestIndicator",
            "OdinRedSuperminion",
            "OdinRockSaw",
            "OdinShieldRelic",
            "OdinSpeedShrine",
            "OdinTestCubeRender",
            "Odin_Blue_Minion_Caster",
            "Odin_Drill",
            "Odin_Lifts_Buckets",
            "Odin_Lifts_Crystal",
            "Odin_Minecart",
            "Odin_Red_Minion_Caster",
            "Odin_skeleton",
            "Odin_SoG_Chaos",
            "Odin_SOG_Chaos_Crystal",
            "Odin_SoG_Order",
            "Odin_SOG_Order_Crystal",
            "Odin_Windmill_Gears",
            "Odin_Windmill_Propellers",
            "Olaf",
            "OlafAxe",
            "OrderInhibitor",
            "OrderInhibitor_D",
            "OrderNexus",
            "OrderTurretAngel",
            "OrderTurretDragon",
            "OrderTurretNormal",
            "OrderTurretNormal2",
            "OrderTurretShrine",
            "OrderTurretTutorial",
            "Orianna",
            "OriannaBall",
            "OriannaNoBall",
            "Pantheon",
            "Poppy",
            "Quinn",
            "QuinnValor",
            "Rammus",
            "RammusDBC",
            "RammusPB",
            "redDragon",
            "Red_Minion_Basic",
            "Red_Minion_MechCannon",
            "Red_Minion_MechMelee",
            "Red_Minion_Wizard",
            "RekSai",
            "RekSaiTunnel",
            "Renekton",
            "Rengar",
            "Riven",
            "Rumble",
            "Ryze",
            "Sejuani",
            "Shaco",
            "ShacoBox",
            "Shen",
            "Shop",
            "ShopKeeper",
            "ShopMale",
            "Shyvana",
            "ShyvanaDragon",
            "SightWard",
            "Singed",
            "Sion",
            "Sivir",
            "Skarner",
            "SkarnerPassiveCrystal",
            "SmallGolem",
            "Sona",
            "SonaDJGenre01",
            "SonaDJGenre02",
            "SonaDJGenre03",
            "Soraka",
            "SpellBook1",
            "SRUAP_Building",
            "SRUAP_ChaosInhibitor",
            "sruap_chaosinhibitor_rubble",
            "SRUAP_ChaosNexus",
            "Sruap_Chaosnexus_Rubble",
            "Sruap_Esports_Banner",
            "sruap_flag",
            "SRUAP_MageCrystal",
            "sruap_mage_vines",
            "SRUAP_OrderInhibitor",
            "sruap_orderinhibitor_rubble",
            "SRUAP_OrderNexus",
            "Sruap_Ordernexus_Rubble",
            "Sruap_Pali_Statue_Banner",
            "SRUAP_Turret_Chaos1",
            "sruap_turret_chaos1_rubble",
            "SRUAP_Turret_Chaos2",
            "SRUAP_Turret_Chaos3",
            "SRUAP_Turret_Chaos3_Test",
            "SRUAP_Turret_Chaos4",
            "SRUAP_Turret_Chaos5",
            "SRUAP_Turret_Order1",
            "SRUAP_Turret_Order1_Rubble",
            "SRUAP_Turret_Order2",
            "SRUAP_Turret_Order3",
            "SRUAP_Turret_Order3_Test",
            "SRUAP_Turret_Order4",
            "SRUAP_Turret_Order5",
            "Sru_Antlermouse",
            "SRU_Baron",
            "SRU_BaronSpawn",
            "SRU_Bird",
            "SRU_Blue",
            "SRU_BlueMini",
            "SRU_BlueMini2",
            "Sru_Butterfly",
            "SRU_ChaosMinionMelee",
            "SRU_ChaosMinionRanged",
            "SRU_ChaosMinionSiege",
            "SRU_ChaosMinionSuper",
            "Sru_Crab",
            "Sru_CrabWard",
            "SRU_Dragon",
            "Sru_Dragonfly",
            "sru_dragon_prop",
            "Sru_Duck",
            "Sru_Duckie",
            "SRU_Es_Banner",
            "Sru_Es_Bannerplatform_Chaos",
            "Sru_Es_Bannerplatform_Order",
            "Sru_Es_Bannerwall_Chaos",
            "Sru_Es_Bannerwall_Order",
            "SRU_Gromp",
            "Sru_Gromp_Prop",
            "SRU_Krug",
            "SRU_KrugMini",
            "Sru_Lizard",
            "SRU_Murkwolf",
            "SRU_MurkwolfMini",
            "SRU_OrderMinionMelee",
            "SRU_OrderMinionRanged",
            "SRU_OrderMinionSiege",
            "SRU_OrderMinionSuper",
            "Sru_Porowl",
            "SRU_Razorbeak",
            "SRU_RazorbeakMini",
            "SRU_Red",
            "SRU_RedMini",
            "SRU_RiverDummy",
            "Sru_Snail",
            "SRU_SnailSpawner",
            "SRU_Spiritwolf",
            "sru_storekeepernorth",
            "sru_storekeepersouth",
            "SRU_WallVisionBearer",
            "SummonerBeacon",
            "Summoner_Rider_Chaos",
            "Summoner_Rider_Order",
            "Swain",
            "SwainBeam",
            "SwainNoBird",
            "SwainRaven",
            "Syndra",
            "SyndraOrbs",
            "SyndraSphere",
            "TahmKench",
            "Talon",
            "Taric",
            "Teemo",
            "TeemoMushroom",
            "TempMovableChar",
            "TestCube",
            "TestCubeRender",
            "TestCubeRender10Vision",
            "TestCubeRenderwCollision",
            "Thresh",
            "ThreshLantern",
            "Tristana",
            "Trundle",
            "TrundleWall",
            "Tryndamere",
            "TT_Brazier",
            "TT_Buffplat_L",
            "TT_Buffplat_R",
            "TT_Chains_Bot_Lane",
            "TT_Chains_Order_Base",
            "TT_Chains_Order_Periph",
            "TT_Chains_Xaos_Base",
            "TT_ChaosInhibitor",
            "TT_ChaosInhibitor_D",
            "TT_ChaosTurret1",
            "TT_ChaosTurret2",
            "TT_ChaosTurret3",
            "TT_ChaosTurret4",
            "TT_ChaosTurret5",
            "TT_DummyPusher",
            "TT_Flytrap_A",
            "TT_Nexus_Gears",
            "TT_NGolem",
            "TT_NGolem2",
            "TT_NWolf",
            "TT_NWolf2",
            "TT_NWraith",
            "TT_NWraith2",
            "TT_OrderInhibitor",
            "TT_OrderInhibitor_D",
            "TT_OrderTurret1",
            "TT_OrderTurret2",
            "TT_OrderTurret3",
            "TT_OrderTurret4",
            "TT_OrderTurret5",
            "TT_Relic",
            "TT_Shopkeeper",
            "TT_Shroom_A",
            "TT_SpeedShrine",
            "TT_Speedshrine_Gears",
            "TT_Spiderboss",
            "TT_SpiderLayer_Web",
            "TT_Tree1",
            "TT_Tree_A",
            "Tutorial_Blue_Minion_Basic",
            "Tutorial_Blue_Minion_Wizard",
            "Tutorial_Red_Minion_Basic",
            "Tutorial_Red_Minion_Wizard",
            "TwistedFate",
            "Twitch",
            "Udyr",
            "UdyrPhoenix",
            "UdyrPhoenixUlt",
            "UdyrTiger",
            "UdyrTigerUlt",
            "UdyrTurtle",
            "UdyrTurtleUlt",
            "UdyrUlt",
            "Urf",
            "Urgot",
            "Varus",
            "Vayne",
            "Veigar",
            "Velkoz",
            "Vi",
            "Viktor",
            "ViktorSingularity",
            "VisionWard",
            "Vladimir",
            "VoidGate",
            "VoidSpawn",
            "VoidSpawnTracer",
            "Volibear",
            "Warwick",
            "wolf",
            "Worm",
            "Wraith",
            "Xerath",
            "XerathArcaneBarrageLauncher",
            "XinZhao",
            "Yasuo",
            "YellowTrinket",
            "YellowTrinketUpgrade",
            "Yonkey",
            "Yorick",
            "YorickDecayedGhoul",
            "YorickRavenousGhoul",
            "YorickSpectralGhoul",
            "YoungLizard",
            "Zac",
            "ZacRebirthBloblet",
            "Zed",
            "ZedShadow",
            "Ziggs",
            "Zilean",
            "Zyra",
            "ZyraGraspingPlant",
            "ZyraPassive",
            "ZyraSeed",
            "ZyraThornPlant"
        };

        #endregion

        static ModelManager()
        {
            var versionJson = new WebClient().DownloadString(DataDragonBase + "realms/na.json");
            GameVersion =
                (string)
                    ((Dictionary<string, object>)
                        new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(versionJson)["n"])["champion"
                        ];

            //if (Game.MapId.Equals(GameMapId.SummonersRift))
            {
                GameObject.OnCreate += GameObject_OnCreate;
            }
            //GameObject.OnDelete += GameObject_OnDelete;
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Base;

            if (unit == null || !unit.IsValid)
            {
                return;
            }

            if (ObjectList.ContainsKey(unit.NetworkId))
            {
                var obj = ObjectList[unit.NetworkId];
                if (!unit.CharData.BaseSkinName.Equals(obj.Name) || !unit.BaseSkinId.Equals(obj.SkinId))
                {
                    Console.WriteLine(
                        "[DELETE] {0} {1} => {2}  {3} => {4}", unit.Name, unit.CharData.BaseSkinName, obj.Name,
                        unit.BaseSkinId, obj.SkinId);
                    unit.SetSkin(obj.Name, obj.SkinId);
                }
                ObjectList.Remove(unit.NetworkId);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            var unit = sender as Obj_AI_Minion;

            if (unit == null || !unit.IsValid)
            {
                return;
            }

            if (Program.Config.Item("Minions").IsActive() && MinionManager.IsMinion(unit))
            {
                unit.SetSkin(unit.CharData.BaseSkinName, 2, 100);
                return;
            }

            var name = unit.CharData.BaseSkinName.ToLower();
            if (Program.Config.Item("Ward").IsActive() && name.Contains("ward") || name.Equals("yellowtrinket"))
            {
                var index = Convert.ToInt32(Program.Config.Item("WardIndex").GetValue<StringList>().SelectedValue);
                Utility.DelayAction.Add(
                    50, () =>
                    {
                        if (Program.Config.Item("WardOwn").IsActive() &&
                            !unit.Buffs.Any(b => b.SourceName.Equals(ObjectManager.Player.ChampionName)))
                        {
                            return;
                        }

                        unit.SetSkin(unit.CharData.BaseSkinName, index);
                    });
            }
            //   ObjectList.RemoveAll(obj => !obj.IsValid);
            /*var unit = sender as Obj_AI_Base;

            if (unit == null || !unit.IsValid)
            {
                return;
            }
            ObjectList.Add(unit.NetworkId, new Model(unit.CharData.BaseSkinName, unit.BaseSkinId));
        
             */
        }

        public static bool IsValidModel(this string model)
        {
            return !string.IsNullOrWhiteSpace(model) && ModelList.Contains(model);
        }

        public static string GetValidModel(this string model)
        {
            var index = ModelList.FindIndex(x => x.Equals(model, StringComparison.OrdinalIgnoreCase));
            return index == -1 ? string.Empty : ModelList[index];
        }

        public static ArrayList GetSkins(string model)
        {
            var champJson =
                new WebClient().DownloadString(
                    DataDragonBase + "cdn/" + GameVersion + "/data/en_US/champion/" + model + ".json");
            return
                (ArrayList)
                    ((Dictionary<string, object>)
                        ((Dictionary<string, object>)
                            new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(champJson)["data"])[model
                            ])["skins"];
        }

        public static void SetSkin(this Obj_AI_Base unit, string model, int index, int delay)
        {
            //unit.SetSkin(model, index);
            Utility.DelayAction.Add(delay, () => unit.SetSkin(model, index));
        }
    }

    internal class Model
    {
        public string Name;
        public int SkinId;

        public Model(string model, int skin)
        {
            Name = model;
            SkinId = skin;
        }
    }
}