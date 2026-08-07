using System.Collections.Generic;
using Jotunn.Managers;
using VikingSettlements.Npcs;

namespace VikingSettlements.Raids
{
    /// <summary>
    /// Plugs a bandit raid into Valheim's native random event system
    /// (RandEventSystem). The event only triggers near player bases - which
    /// includes player settlements via the banner's PlayerBase effect area.
    /// The registration is re-applied whenever a new game session creates a
    /// fresh RandEventSystem instance.
    /// </summary>
    internal static class RaidEvents
    {
        public const string EventName = "vs_banditraid";

        private static bool _registered;

        /// <summary>Called from the plugin's Update loop.</summary>
        public static void EnsureRegistered()
        {
            var system = RandEventSystem.instance;
            if (system == null)
            {
                _registered = false;
                return;
            }
            if (_registered || !ModConfig.EnableRaids.Value)
            {
                return;
            }

            var raider = PrefabManager.Instance.GetPrefab(SettlerPrefabs.Raider);
            if (raider == null)
            {
                return; // prefabs not ready yet, retry next frame
            }

            foreach (var existing in system.m_events)
            {
                if (existing.m_name == EventName)
                {
                    _registered = true;
                    return;
                }
            }

            var biomes = Heightmap.Biome.Meadows | Heightmap.Biome.BlackForest | Heightmap.Biome.Plains;
            var randomEvent = new RandomEvent
            {
                m_name = EventName,
                m_enabled = true,
                m_random = true,
                m_duration = 120f,
                m_nearBaseOnly = true,
                m_pauseIfNoPlayerInArea = true,
                m_biome = biomes,
                m_startMessage = "$vs_raid_start",
                m_endMessage = "$vs_raid_end",
                m_spawn = new List<SpawnSystem.SpawnData>
                {
                    new SpawnSystem.SpawnData
                    {
                        m_name = "vs_raider",
                        m_enabled = true,
                        m_prefab = raider,
                        m_biome = biomes,
                        m_biomeArea = Heightmap.BiomeArea.Everything,
                        m_maxSpawned = 6,
                        m_spawnInterval = 10f,
                        m_spawnChance = 100f,
                        m_groupSizeMin = 2,
                        m_groupSizeMax = 3,
                        m_spawnAtDay = true,
                        m_spawnAtNight = true,
                        m_huntPlayer = true,
                        m_maxLevel = 2,
                        m_minLevel = 1,
                        m_groundOffset = 0.5f,
                    },
                },
            };
            if (ModConfig.RaidsAfterFirstBoss.Value)
            {
                randomEvent.m_requiredGlobalKeys ??= new List<string>();
                randomEvent.m_requiredGlobalKeys.Add("defeated_eikthyr");
            }
            // Clearing every counted clanless camp silences the native event.
            randomEvent.m_notRequiredGlobalKeys ??= new List<string>();
            randomEvent.m_notRequiredGlobalKeys.Add(CampTotem.AllCampsClearedKey);

            system.m_events.Add(randomEvent);
            _registered = true;
            Jotunn.Logger.LogInfo("Registered bandit raid with the native random event system");
        }
    }
}
