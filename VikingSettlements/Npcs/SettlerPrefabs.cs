using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Creates the settlement NPC prefabs by cloning vanilla humanoids and
    /// re-purposing them as friendly villagers. All prefab lookups are
    /// defensive: a missing base prefab logs a warning instead of breaking
    /// the whole mod after a game update.
    /// </summary>
    internal static class SettlerPrefabs
    {
        public const string Settler = "VS_Settler";
        public const string Seer = "VS_Seer";
        public const string Trader = "VS_Trader";
        public const string Flatten = "VS_Flatten";

        private static bool _created;

        public static void CreateAll()
        {
            if (_created)
            {
                return;
            }
            _created = true;

            CreateSettler(Settler, new[] { "Dverger", "DvergerMageSupport", "DvergerMage" }, "$vs_settler");
            CreateSettler(Seer, new[] { "DvergerMageSupport", "DvergerMage", "Dverger" }, "$vs_seer");
            CreateTrader();
            CreateFlatten();
        }

        private static GameObject CloneFirstAvailable(string newName, IEnumerable<string> baseCandidates)
        {
            foreach (var candidate in baseCandidates)
            {
                if (PrefabManager.Instance.GetPrefab(candidate) != null)
                {
                    return PrefabManager.Instance.CreateClonedPrefab(newName, candidate);
                }
            }
            return null;
        }

        private static void CreateSettler(string name, string[] baseCandidates, string nameToken)
        {
            var clone = CloneFirstAvailable(name, baseCandidates);
            if (clone == null)
            {
                Jotunn.Logger.LogWarning($"Could not create {name}: no base prefab found ({string.Join(", ", baseCandidates)})");
                return;
            }

            var humanoid = clone.GetComponent<Humanoid>();
            if (humanoid != null)
            {
                humanoid.m_name = nameToken;
                humanoid.m_group = "vs_settlement";
                humanoid.m_boss = false;
                humanoid.m_faction = ModConfig.SettlersDefendPlayers.Value
                    ? Character.Faction.Players
                    : Character.Faction.Dverger;
            }

            // Settlers should not be farmable for their base creature's biome loot.
            var characterDrop = clone.GetComponent<CharacterDrop>();
            if (characterDrop != null)
            {
                characterDrop.m_drops.Clear();
                var coins = PrefabManager.Instance.GetPrefab("Coins");
                if (coins != null)
                {
                    characterDrop.m_drops.Add(new CharacterDrop.Drop
                    {
                        m_prefab = coins,
                        m_amountMin = 1,
                        m_amountMax = 8,
                        m_chance = 0.6f,
                    });
                }
            }

            // Replace any vanilla idle talk with our own villager chatter.
            var npcTalk = clone.GetComponent<NpcTalk>();
            if (npcTalk != null)
            {
                Object.DestroyImmediate(npcTalk);
            }

            clone.AddComponent<SettlerIdentity>();
            clone.AddComponent<SettlerChatter>();
            clone.AddComponent<SettlerHome>();

            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
            Jotunn.Logger.LogInfo($"Created settlement NPC prefab {name}");
        }

        private static void CreateTrader()
        {
            if (!ModConfig.EnableTrader.Value)
            {
                return;
            }

            var clone = CloneFirstAvailable(Trader, new[] { "Haldor" });
            if (clone == null)
            {
                Jotunn.Logger.LogWarning("Could not create VS_Trader: Haldor prefab not found");
                return;
            }

            var trader = clone.GetComponent<global::Trader>();
            if (trader != null)
            {
                trader.m_name = "$vs_trader";
                trader.m_items.Clear();
                AddTradeItem(trader, "Honey", 20, 5);
                AddTradeItem(trader, "Flint", 10, 5);
                AddTradeItem(trader, "DeerHide", 15, 5);
                AddTradeItem(trader, "Resin", 8, 10);
                AddTradeItem(trader, "FishRaw", 25, 3);
                AddTradeItem(trader, "Bread", 60, 3);
                AddTradeItem(trader, "MeadHealthMinor", 50, 1);
            }

            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
            Jotunn.Logger.LogInfo("Created settlement trader prefab VS_Trader");
        }

        private static void AddTradeItem(global::Trader trader, string itemName, int price, int stack)
        {
            var prefab = PrefabManager.Instance.GetPrefab(itemName);
            var itemDrop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
            if (itemDrop == null)
            {
                Jotunn.Logger.LogWarning($"Trade item '{itemName}' not found, skipped");
                return;
            }
            trader.m_items.Add(new global::Trader.TradeItem
            {
                m_prefab = itemDrop,
                m_price = price,
                m_stack = stack,
            });
        }

        /// <summary>
        /// A one-shot terrain op that levels the ground under a settlement when
        /// its location spawns, cloned from a vanilla terrain-modifying piece.
        /// </summary>
        private static void CreateFlatten()
        {
            var clone = CloneFirstAvailable(Flatten, new[] { "mud_road_v2", "path_v2", "mud_road", "path" });
            if (clone == null)
            {
                Jotunn.Logger.LogWarning("Could not create VS_Flatten: no terrain op base prefab found");
                return;
            }

            var terrainOp = clone.GetComponent<TerrainOp>();
            if (terrainOp != null)
            {
                terrainOp.m_settings.m_level = true;
                terrainOp.m_settings.m_levelRadius = 13f;
                terrainOp.m_settings.m_levelOffset = 0f;
                terrainOp.m_settings.m_smooth = true;
                terrainOp.m_settings.m_smoothRadius = 18f;
                terrainOp.m_settings.m_smoothPower = 3f;
                terrainOp.m_settings.m_paintCleared = true;
                terrainOp.m_settings.m_paintType = TerrainModifier.PaintType.Dirt;
                terrainOp.m_settings.m_paintRadius = 6f;
            }
            else
            {
                var terrainModifier = clone.GetComponent<TerrainModifier>();
                if (terrainModifier != null)
                {
                    terrainModifier.m_level = true;
                    terrainModifier.m_levelRadius = 13f;
                    terrainModifier.m_smooth = true;
                    terrainModifier.m_smoothRadius = 18f;
                    terrainModifier.m_smoothPower = 3f;
                    terrainModifier.m_paintCleared = true;
                    terrainModifier.m_paintType = TerrainModifier.PaintType.Dirt;
                    terrainModifier.m_paintRadius = 6f;
                }
            }

            // Not a buildable piece anymore, just a location helper.
            var piece = clone.GetComponent<Piece>();
            if (piece != null)
            {
                Object.DestroyImmediate(piece);
            }

            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
        }
    }
}
