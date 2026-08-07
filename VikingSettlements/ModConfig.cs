using BepInEx.Configuration;
using Jotunn.Utils;

namespace VikingSettlements
{
    internal static class ModConfig
    {
        public static ConfigEntry<int> MeadowsVillages;
        public static ConfigEntry<int> ForestOutposts;
        public static ConfigEntry<int> PlainsSteadings;
        public static ConfigEntry<bool> SettlersDefendPlayers;
        public static ConfigEntry<bool> EnableTrader;
        public static ConfigEntry<bool> ChatterEnabled;
        public static ConfigEntry<float> ChatterInterval;

        public static void Init(ConfigFile config)
        {
            MeadowsVillages = config.Bind("Locations", "MeadowsVillages", 60,
                new ConfigDescription(
                    "How many meadows villages the world generator attempts to place. " +
                    "Only applies to newly generated worlds or unexplored areas of existing worlds. Set to 0 to disable.",
                    new AcceptableValueRange<int>(0, 500),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            ForestOutposts = config.Bind("Locations", "ForestOutposts", 80,
                new ConfigDescription(
                    "How many black forest outposts the world generator attempts to place. " +
                    "Only applies to newly generated worlds or unexplored areas of existing worlds. Set to 0 to disable.",
                    new AcceptableValueRange<int>(0, 500),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            PlainsSteadings = config.Bind("Locations", "PlainsSteadings", 50,
                new ConfigDescription(
                    "How many plains steadings the world generator attempts to place. " +
                    "Only applies to newly generated worlds or unexplored areas of existing worlds. Set to 0 to disable.",
                    new AcceptableValueRange<int>(0, 500),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            SettlersDefendPlayers = config.Bind("Settlers", "DefendPlayers", false,
                new ConfigDescription(
                    "If true, settlers use the player faction and actively fight alongside players. " +
                    "If false (default), settlers are neutral villagers that defend their home and " +
                    "turn hostile when attacked.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            EnableTrader = config.Bind("Settlers", "EnableTrader", true,
                new ConfigDescription(
                    "Whether meadows villages contain a trader with a small store.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            ChatterEnabled = config.Bind("Settlers", "Chatter", true,
                "Whether settlers occasionally greet and talk to players who come close. Client side, purely cosmetic.");

            ChatterInterval = config.Bind("Settlers", "ChatterIntervalSeconds", 25f,
                new ConfigDescription(
                    "Minimum seconds between chatter lines of a single settler.",
                    new AcceptableValueRange<float>(5f, 300f)));
        }
    }
}
