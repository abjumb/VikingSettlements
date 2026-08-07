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
        public static ConfigEntry<int> RecruitCostCoins;
        public static ConfigEntry<int> MaxSettlersPerSettlement;
        public static ConfigEntry<float> SettlementRadius;
        public static ConfigEntry<float> WorkIntervalSeconds;
        public static ConfigEntry<bool> EnableRaids;
        public static ConfigEntry<bool> RaidsAfterFirstBoss;
        public static ConfigEntry<float> RivalRaidChancePerDay;
        public static ConfigEntry<int> ClanlessCamps;
        public static ConfigEntry<bool> ScaleRaids;
        public static ConfigEntry<float> CampClearRaidReduction;
        public static ConfigEntry<bool> FoodUpkeep;
        public static ConfigEntry<float> MealIntervalSeconds;
        public static ConfigEntry<bool> GrowthEnabled;
        public static ConfigEntry<float> GrowthChancePerDay;
        public static ConfigEntry<int> GrowthFoodCost;
        public static ConfigEntry<bool> RequireWorkstations;

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

            RecruitCostCoins = config.Bind("Recruiting", "RecruitCostCoins", 50,
                new ConfigDescription(
                    "Coins required to recruit a settler from a wild settlement.",
                    new AcceptableValueRange<int>(0, 10000),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            MaxSettlersPerSettlement = config.Bind("Settlement", "MaxSettlers", 10,
                new ConfigDescription(
                    "Maximum settlers that can be assigned to one settlement banner.",
                    new AcceptableValueRange<int>(1, 50),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            SettlementRadius = config.Bind("Settlement", "SettlementRadius", 32f,
                new ConfigDescription(
                    "Radius in meters around a settlement banner that counts as the settlement " +
                    "(job work area, assignment range, raid target area).",
                    new AcceptableValueRange<float>(10f, 64f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            WorkIntervalSeconds = config.Bind("Settlement", "WorkIntervalSeconds", 60f,
                new ConfigDescription(
                    "Seconds between work ticks of an assigned settler (production, repairs, smelting).",
                    new AcceptableValueRange<float>(10f, 3600f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            EnableRaids = config.Bind("Raids", "EnableRaids", true,
                new ConfigDescription(
                    "Register the bandit raid with Valheim's native random event system and " +
                    "allow rival clans to raid player settlements.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            RaidsAfterFirstBoss = config.Bind("Raids", "RaidsAfterFirstBoss", true,
                new ConfigDescription(
                    "Bandit raids only start after Eikthyr has been defeated.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            RivalRaidChancePerDay = config.Bind("Raids", "RivalRaidChancePerDay", 0.15f,
                new ConfigDescription(
                    "Chance per in-game day that a rival clan raids a player settlement (rolled " +
                    "each night per settlement banner while its area is loaded).",
                    new AcceptableValueRange<float>(0f, 1f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            ClanlessCamps = config.Bind("Raids", "ClanlessCamps", 60,
                new ConfigDescription(
                    "How many clanless bandit camps the world generator attempts to place " +
                    "(new worlds / unexplored areas only). Destroying a camp's war totem " +
                    "permanently reduces rival raid chance. Set to 0 to disable.",
                    new AcceptableValueRange<int>(0, 500),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            ScaleRaids = config.Bind("Raids", "ScaleRaids", true,
                new ConfigDescription(
                    "Rival war parties grow with the target settlement's population and gain " +
                    "star levels as bosses fall (1-star after The Elder, 2-star after Bonemass).",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            CampClearRaidReduction = config.Bind("Raids", "CampClearRaidReduction", 0.05f,
                new ConfigDescription(
                    "Relative reduction of the rival raid chance per cleared clanless camp " +
                    "(destroyed war totem), up to 10 camps. Clearing 10 camps also disables " +
                    "the native bandit raid event.",
                    new AcceptableValueRange<float>(0f, 0.1f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            FoodUpkeep = config.Bind("Economy", "FoodUpkeep", true,
                new ConfigDescription(
                    "Assigned settlers periodically eat one food item from settlement chests, " +
                    "cheapest first. A settler that finds no food goes hungry and stops working " +
                    "until its next meal.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            MealIntervalSeconds = config.Bind("Economy", "MealIntervalSeconds", 1800f,
                new ConfigDescription(
                    "In-game seconds between meals of an assigned settler. The default of 1800 " +
                    "is roughly one meal per in-game day.",
                    new AcceptableValueRange<float>(120f, 7200f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            GrowthEnabled = config.Bind("Economy", "GrowthEnabled", true,
                new ConfigDescription(
                    "Settlements below their settler cap can attract a new settler: each night " +
                    "there is a chance a newcomer arrives, provided the settlement has a spare " +
                    "unclaimed bed and enough food in its chests.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            GrowthChancePerDay = config.Bind("Economy", "GrowthChancePerDay", 0.35f,
                new ConfigDescription(
                    "Nightly chance that a new settler arrives when the growth conditions are met.",
                    new AcceptableValueRange<float>(0f, 1f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            GrowthFoodCost = config.Bind("Economy", "GrowthFoodCost", 3,
                new ConfigDescription(
                    "Food items consumed from settlement chests when a new settler arrives.",
                    new AcceptableValueRange<int>(0, 20),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            RequireWorkstations = config.Bind("Economy", "RequireWorkstations", true,
                new ConfigDescription(
                    "Jobs need their workstation inside the settlement: blacksmiths a forge, " +
                    "builders a workbench, and farmers a beehive for honey. Disable to restore " +
                    "the ungated 1.1 behavior.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));
        }
    }
}
