using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
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
        public const string PluginVersion = "1.11.0";

        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private Harmony _harmony;

        private void Awake()
        {
            ModConfig.Init(Config);
            AddLocalizations();
            _harmony = new Harmony(PluginGUID);
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            PrefabManager.OnVanillaPrefabsAvailable += CreatePrefabs;
            ZoneManager.OnVanillaLocationsAvailable += RegisterLocations;
            CommandManager.Instance.AddConsoleCommand(new Commands.SpawnSettlementCommand());
            CommandManager.Instance.AddConsoleCommand(new Commands.FindSettlementCommand());
            CommandManager.Instance.AddConsoleCommand(new Commands.PartyCommand());

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
            Party.PartySystem.OnUpdate();
            Npcs.SettlerTalkPanel.OnUpdate();
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
                { "vs_manage", "Manage" },
                { "vs_rep", "Village standing" },
                { "vs_rep_honored", "Honored" },
                { "vs_rep_friendly", "Friendly" },
                { "vs_rep_neutral", "Neutral" },
                { "vs_rep_distrusted", "Distrusted" },
                { "vs_rep_hated", "Hated" },
                { "vs_rep_refuse", "They refuse to deal with you" },
                { "vs_donate", "Donate" },
                { "vs_donated", "The village appreciates your gift" },
                { "vs_rename", "Rename" },
                { "vs_rename_topic", "Name your settlement" },
                { "vs_close", "Close" },
                { "vs_nosettlers", "No settlers assigned yet — recruit villagers and press E near the banner" },
                { "vs_col_settler", "SETTLER" },
                { "vs_col_job", "JOB" },
                { "vs_col_status", "STATUS" },
                { "vs_rank_settler", "Settler" },
                { "vs_status_working", "Working" },
                { "vs_status_hungry", "Hungry" },
                { "vs_panel_hint", "Jobs deposit into chests inside the settlement radius" },
                { "vs_raid_start", "The clanless are raiding!" },
                { "vs_raid_end", "The clanless retreat" },
                { "vs_camp_totem", "Clanless War Totem" },
                { "vs_camp_totem_hint", "Destroy it to weaken the clanless raids" },
                { "vs_camp_cleared", "A clanless camp is broken! Their raids weaken" },
                { "vs_party_member", "Party" },
                { "vs_party_full", "Your party is full" },
                { "vs_party_waitcmd", "Wait here" },
                { "vs_party_followcmd", "With me" },
                { "vs_party_waits", "waits here" },
                { "vs_party_comes", "follows you" },
                { "vs_party_stance_follow", "Following" },
                { "vs_party_stance_hold", "Holding" },
                { "vs_party_stance_fallback", "Falling back" },
                { "vs_party_cmd_follow", "Party: with me!" },
                { "vs_party_cmd_hold", "Party: hold here!" },
                { "vs_party_cmd_fallback", "Party: fall back!" },
                { "vs_party_wounded", "is wounded!" },
                { "vs_party_grave", "is gravely wounded!" },
                { "vs_party_retreats", "is gravely wounded and retreats to you!" },
                { "vs_party_fallen", "has fallen" },
                { "vs_party_aboard", "Your party travels with you" },
                { "vs_party_ashore", "Your party regroups around you" },
                { "vs_joined_hint", "They fight at your side — settle them at your banner (E) to put them to work" },
                { "vs_talk_wild", "A free villager" },
                { "vs_talk_party", "In your party" },
                { "vs_talk_party_hint", "Settle them at your settlement banner to give them a job" },
                { "vs_talk_health", "Health" },
                { "vs_talk_fed", "Well fed" },
                { "vs_talk_hungry", "Hungry — nothing to eat! Put food in a settlement chest" },
                { "vs_talk_nextmeal", "next meal in" },
                { "vs_talk_needs", "What I need" },
                { "vs_talk_villager_none", "No duties — press E on me to assign a job" },
                { "vs_talk_guard_none", "I keep watch. I need nothing more" },
                { "vs_need_chest", "A chest with room in the settlement" },
                { "vs_need_food", "Food in a settlement chest" },
                { "vs_need_workbench", "A workbench in the settlement" },
                { "vs_need_forge", "A forge in the settlement" },
                { "vs_need_cookstation", "A cooking station in the settlement" },
                { "vs_need_fermenter", "A fermenter in the settlement" },
                { "vs_need_beehive", "A beehive, for honey" },
                { "vs_need_ore", "Copper/tin ore or iron scraps in one chest" },
                { "vs_need_rawfood", "Raw meat or fish in one chest" },
                { "vs_need_brewing", "2 honey or 2 barley together in one chest" },
                { "vs_need_damage", "Damaged structures to repair" },
                { "vs_talk_g1", "What do you need, chief?" },
                { "vs_talk_g2", "The fire is warm tonight." },
                { "vs_talk_g3", "Odin watches over us." },
                { "vs_talk_g4", "Good hunting lately, eh?" },
                { "vs_buildchest", "Builders' Supply Chest" },
                { "vs_buildchest_desc", "Builders draw construction materials from this chest for their assigned projects. Lumberjacks and miners refill it when an active project runs low." },
                { "vs_site", "Construction site" },
                { "vs_bp_cabin", "Cabin" },
                { "vs_bp_longhouse", "Longhouse" },
                { "vs_bp_watchtower", "Watchtower" },
                { "vs_bp_started", "The builders mark out a construction site" },
                { "vs_bp_busy", "The settlement already has an active construction site" },
                { "vs_bp_outside", "Too far from the settlement banner — stand where the building should go" },
                { "vs_bp_cancel", "Cancel project" },
                { "vs_bp_canceled", "Construction canceled" },
                { "vs_bp_complete", "The builders have finished" },
                { "vs_bp_needsbuilder", "Waiting for a builder" },
                { "vs_supply_low", "The builders are out of materials! Fill the supply chest" },
                { "vs_talk_build", "What should we build?" },
                { "vs_talk_project", "Current project" },
                { "vs_talk_home", "Has a home" },
                { "vs_talk_homeless", "No home — working slower (talk key on a door)" },
                { "vs_need_supplies", "Materials in the supply chest" },
                { "vs_home_title", "Who lives here?" },
                { "vs_home_occupant", "lives here" },
                { "vs_home_assign", "Move in" },
                { "vs_home_unassign", "Move out" },
                { "vs_home_nosettlement", "This door is not inside a settlement" },
                { "vs_tier1", "Hamlet" },
                { "vs_tier2", "Village" },
                { "vs_tier3", "Town" },
                { "vs_promoted", "has grown into a" },
                { "vs_bp_greathall", "Stone Great-Hall" },
                { "vs_bp_locked", "Higher settlement tiers unlock more blueprints" },
                { "vs_warlord", "Clanless Warlord" },
                { "vs_warlord_comes", "A clanless warlord marches on your settlement!" },
                { "vs_warlord_slain", "The warlord has fallen! The clanless grant this land peace" },
                { "vs_gear", "Equipment" },
                { "vs_gear_give", "Give from your inventory" },
                { "vs_gear_givebtn", "Give" },
                { "vs_gear_return", "Take back" },
                { "vs_gear_none", "—" },
                { "vs_gear_nothing", "Nothing equippable in your inventory" },
                { "vs_inv_full", "No room in your inventory" },
                { "vs_slot_weapon", "Weapon" },
                { "vs_slot_shield", "Shield" },
                { "vs_slot_helmet", "Helmet" },
                { "vs_slot_chest", "Chest" },
                { "vs_slot_legs", "Legs" },
                { "vs_job_courier", "Courier" },
                { "vs_job_herder", "Herder" },
                { "vs_bp_pen", "Livestock Pen" },
                { "vs_talk_mood", "Mood" },
                { "vs_mood_cheerful", "Cheerful — working extra hard" },
                { "vs_mood_content", "Content" },
                { "vs_mood_unhappy", "Unhappy" },
                { "vs_mood_miserable", "Miserable — barely working" },
                { "vs_mood_left", "has had enough and left the settlement!" },
                { "vs_need_dest", "Another settlement within courier range" },
                { "vs_need_surplus", "Surplus goods to haul (more than 10 of something)" },
                { "vs_need_animals", "Tamed animals in the settlement" },
                { "vs_need_feed", "Carrots or turnips in a chest, for feed" },
                { "vs_courier_ambush", "A courier is ambushed on the road!" },
                { "vs_job_engineer", "Engineer" },
                { "vs_ballista", "Settlement Ballista" },
                { "vs_bp_palisade", "Palisade Ring" },
                { "vs_bp_ballista", "Ballista Tower" },
                { "vs_need_ballista", "A ballista tower to keep loaded" },
                { "vs_need_boltwood", "Wood in a chest, for fletching bolts" },
                { "vs_clan0", "The Clanless" },
                { "vs_clan1", "The Ashwolves" },
                { "vs_clan2", "The Saltborn" },
                { "vs_clan3", "The Blood Ravens" },
                { "vs_clan4", "The Grey Hounds" },
                { "vs_clan5", "The Oathbreakers" },
                { "vs_clan6", "The Night Axes" },
                { "vs_clan7", "The Rime Serpents" },
                { "vs_clan8", "The Broken Shields" },
                { "vs_clan_attack", "are attacking the settlement!" },
                { "vs_clan_shattered", "are broken — their raids on this land are over" },
                { "vs_clan_broken_note", "clan broken" },
                { "vs_abducted", "has been carried off by the clanless! Destroy their camp's war totem to free them" },
                { "vs_captive", "Captive" },
                { "vs_captive_days", "days to save them" },
                { "vs_rescued", "is free and comes home!" },
                { "vs_captive_lost", "will never come home. The settlement mourns" },
            });
        }
    }
}
