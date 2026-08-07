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
        public const string PluginVersion = "1.0.0";

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            ModConfig.Init(Config);
            AddLocalizations();

            PrefabManager.OnVanillaPrefabsAvailable += CreatePrefabs;
            ZoneManager.OnVanillaLocationsAvailable += RegisterLocations;
            CommandManager.Instance.AddConsoleCommand(new Commands.SpawnSettlementCommand());

            Jotunn.Logger.LogInfo($"{PluginName} v{PluginVersion} loaded - settlements appear in newly generated world areas");
        }

        private void CreatePrefabs()
        {
            Npcs.SettlerPrefabs.CreateAll();
            PrefabManager.OnVanillaPrefabsAvailable -= CreatePrefabs;
        }

        private void RegisterLocations()
        {
            World.SettlementLocations.RegisterAll();
        }

        private void AddLocalizations()
        {
            Localization.AddTranslation("English", new Dictionary<string, string>
            {
                { "vs_settler", "Settler" },
                { "vs_seer", "Village Seer" },
                { "vs_trader", "Sigvald the Trader" },
            });
        }
    }
}
