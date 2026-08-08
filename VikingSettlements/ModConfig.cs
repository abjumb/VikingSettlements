using BepInEx.Configuration;
using Jotunn.Utils;
using UnityEngine;

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
        public static ConfigEntry<bool> VeterancyEnabled;
        public static ConfigEntry<int> XpPerStar;
        public static ConfigEntry<bool> ReputationEnabled;
        public static ConfigEntry<int> DonationCostCoins;
        public static ConfigEntry<int> DonationReputation;
        public static ConfigEntry<int> MaxPartySize;
        public static ConfigEntry<bool> PartyAutoFallback;
        public static ConfigEntry<float> PartyRegenPerSecond;
        public static ConfigEntry<KeyboardShortcut> PartyStanceKey;
        public static ConfigEntry<KeyboardShortcut> PartyFallbackKey;
        public static ConfigEntry<KeyboardShortcut> TalkHotkey;
        public static ConfigEntry<bool> HomesMatter;
        public static ConfigEntry<bool> MoraleEnabled;
        public static ConfigEntry<float> CourierRange;
        public static ConfigEntry<float> CourierAmbushChance;
        public static ConfigEntry<bool> TiersEnabled;
        public static ConfigEntry<bool> WarlordEnabled;
        public static ConfigEntry<float> WarlordChance;
        public static ConfigEntry<int> WarlordPeaceDays;

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

            VeterancyEnabled = config.Bind("Veterancy", "VeterancyEnabled", true,
                new ConfigDescription(
                    "Settlers earn experience (1 XP per in-game day of assigned service, " +
                    "2 XP per battle survived) and rise through star levels with vanilla " +
                    "stat scaling.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            XpPerStar = config.Bind("Veterancy", "XpPerStar", 20,
                new ConfigDescription(
                    "Experience required for a settler's first star. The second star costs " +
                    "three times as much.",
                    new AcceptableValueRange<int>(5, 200),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            ReputationEnabled = config.Bind("Reputation", "ReputationEnabled", true,
                new ConfigDescription(
                    "Wild villages track a shared standing toward players: defending " +
                    "villagers and donating coins raise it, attacking them lowers it. " +
                    "Standing scales recruit costs; hated villages refuse recruiting.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            DonationCostCoins = config.Bind("Reputation", "DonationCostCoins", 10,
                new ConfigDescription(
                    "Coins given per donation (Shift+E on a wild settler).",
                    new AcceptableValueRange<int>(1, 1000),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            DonationReputation = config.Bind("Reputation", "DonationReputation", 5,
                new ConfigDescription(
                    "Reputation gained per donation.",
                    new AcceptableValueRange<int>(1, 50),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            MaxPartySize = config.Bind("Party", "MaxPartySize", 4,
                new ConfigDescription(
                    "Maximum recruited villagers that can travel with a player at once. " +
                    "The party is strong because bringing it is a real bet - raising " +
                    "this dilutes the stakes the feature is built on.",
                    new AcceptableValueRange<int>(1, 4),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            PartyAutoFallback = config.Bind("Party", "AutoFallbackWhenGravelyWounded", true,
                new ConfigDescription(
                    "A party member that drops below a quarter health automatically stops " +
                    "fighting and retreats to you. Disabling this removes the telegraphed " +
                    "safety net in front of every companion death.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            PartyRegenPerSecond = config.Bind("Party", "OutOfCombatRegenPerSecond", 2f,
                new ConfigDescription(
                    "Health a party member recovers per second after 10 seconds without " +
                    "taking damage. Keeps losses a moment-to-moment stake instead of an " +
                    "attrition tax between fights. 0 disables.",
                    new AcceptableValueRange<float>(0f, 20f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            PartyStanceKey = config.Bind("Party", "StanceHotkey",
                new KeyboardShortcut(KeyCode.G),
                "Toggles the whole party between following you and holding position. Client-side.");

            PartyFallbackKey = config.Bind("Party", "FallbackHotkey",
                new KeyboardShortcut(KeyCode.H),
                "Orders the whole party to fall back: they stop fighting, run to you and " +
                "take greatly reduced damage. Press again to resume following. Client-side.");

            HomesMatter = config.Bind("Economy", "HomesMatter", true,
                new ConfigDescription(
                    "Settlers without an assigned home work at half speed. Assign homes " +
                    "by pressing the talk key on a door inside the settlement.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            MoraleEnabled = config.Bind("Economy", "MoraleEnabled", true,
                new ConfigDescription(
                    "Settlers track morale from housing, food, company and raids. " +
                    "Cheerful settlers produce extra, miserable ones slow down, and a " +
                    "settler at rock bottom leaves the settlement.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            CourierRange = config.Bind("Trade", "CourierRange", 300f,
                new ConfigDescription(
                    "Maximum distance in meters to a partner settlement for the Courier job.",
                    new AcceptableValueRange<float>(50f, 2000f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            CourierAmbushChance = config.Bind("Trade", "CourierAmbushChance", 0.02f,
                new ConfigDescription(
                    "Chance every few seconds that a courier on the open road draws a " +
                    "clanless ambush. 0 disables ambushes.",
                    new AcceptableValueRange<float>(0f, 1f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            TiersEnabled = config.Bind("Progression", "TiersEnabled", true,
                new ConfigDescription(
                    "Settlements progress Hamlet -> Village -> Town, with tier-scaled " +
                    "settler caps and tier-gated blueprints. Disabled, every settlement " +
                    "behaves as a Village.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            WarlordEnabled = config.Bind("Progression", "WarlordEnabled", true,
                new ConfigDescription(
                    "Once three or more clanless camps have been cleared, rival raids " +
                    "can bring a warlord. Killing him grants the settlement days of " +
                    "raid peace.",
                    null,
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            WarlordChance = config.Bind("Progression", "WarlordChance", 0.25f,
                new ConfigDescription(
                    "Chance that a rival raid includes the warlord (after 3+ camps cleared).",
                    new AcceptableValueRange<float>(0f, 1f),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            WarlordPeaceDays = config.Bind("Progression", "WarlordPeaceDays", 10,
                new ConfigDescription(
                    "In-game days without rival raids for the settlement that fells a warlord.",
                    new AcceptableValueRange<int>(1, 100),
                    new ConfigurationManagerAttributes { IsAdminOnly = true }));

            TalkHotkey = config.Bind("Settlers", "TalkHotkey",
                new KeyboardShortcut(KeyCode.T),
                "Talk to the settler you are looking at (or the nearest within 5 m): opens " +
                "a panel with their health, hunger and everything their job still needs " +
                "before they will work. Client-side.");
        }
    }
}
