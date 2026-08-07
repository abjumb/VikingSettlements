using System.Collections.Generic;
using BepInEx;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace VikingSettlements
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class VikingSettlements : BaseUnityPlugin
    {
        public const string PluginGUID = "com.abjumb.vikingsettlements";
        public const string PluginName = "VikingSettlements";
        public const string PluginVersion = "1.5.0";

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            ModConfig.Init(Config);
            AddLocalizations();

            PrefabManager.OnVanillaPrefabsAvailable += CreatePrefabs;
            ZoneManager.OnVanillaLocationsAvailable += RegisterLocations;
            CommandManager.Instance.AddConsoleCommand(new Commands.SpawnSettlementCommand());
            CommandManager.Instance.AddConsoleCommand(new Commands.FindSettlementCommand());

            Jotunn.Logger.LogInfo($"{PluginName} v{PluginVersion} loaded - settlements appear in newly generated world areas");
        }

        private void CreatePrefabs()
        {
            Npcs.SettlerPrefabs.CreateAll();
            Settlements.SettlementPieces.CreateAll();
            PrefabManager.OnVanillaPrefabsAvailable -= CreatePrefabs;
        }

        private void RegisterLocations()
        {
            World.SettlementLocations.RegisterAll();
        }

        private void Update()
        {
            // The random event system is recreated per game session; keep the
            // bandit raid registered in it.
            Raids.RaidEvents.EnsureRegistered();
        }

        private void AddLocalizations()
        {
            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                { "vs_settler", "Settler" },
                { "vs_seer", "Village Seer" },
                { "vs_trader", "Sigvald the Trader" },
                { "vs_raider", "Clanless Bandit" },
                { "vs_banner", "Settlement Banner" },
                { "vs_banner_desc", "Founds a settlement. Recruit settlers from wild villages and assign them here to work jobs. Beware: settlements attract raids." },
                { "vs_settlers", "Settlers" },
                { "vs_recruit", "Recruit" },
                { "vs_assign", "Assign to settlement" },
                { "vs_dismiss", "Dismiss" },
                { "vs_changejob", "Change job" },
                { "vs_unassign", "Unassign" },
                { "vs_following", "Following" },
                { "vs_joined", "joins you!" },
                { "vs_dismissed", "stays behind" },
                { "vs_assigned", "settles here!" },
                { "vs_needcoins", "Not enough coins" },
                { "vs_nosettlement", "No settlement banner nearby" },
                { "vs_settlementfull", "This settlement is full" },
                { "vs_job_villager", "Villager" },
                { "vs_job_lumberjack", "Lumberjack" },
                { "vs_job_farmer", "Farmer" },
                { "vs_job_builder", "Builder" },
                { "vs_job_blacksmith", "Blacksmith" },
                { "vs_job_guard", "Guard" },
                { "vs_job_cook", "Cook" },
                { "vs_job_miner", "Miner" },
                { "vs_job_hunter", "Hunter" },
                { "vs_job_brewer", "Brewer" },
                { "vs_hungry", "Hungry" },
                { "vs_veteran", "Veteran" },
                { "vs_elite", "Elite" },
                { "vs_levelup", "has grown stronger!" },
                { "vs_raid_start", "The clanless are raiding!" },
                { "vs_raid_end", "The clanless retreat" },
                { "vs_camp_totem", "Clanless War Totem" },
                { "vs_camp_totem_hint", "Destroy it to weaken the clanless raids" },
                { "vs_camp_cleared", "A clanless camp is broken! Their raids weaken" },
            });
        }
    }
}
