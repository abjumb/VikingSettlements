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
        public const string FlattenVillage = "VS_FlattenVillage";
        public const string FlattenSteading = "VS_FlattenSteading";
        public const string FlattenOutpost = "VS_FlattenOutpost";
        public const string FlattenCamp = "VS_FlattenCamp";
        public const string Raider = "VS_Raider";
        public const string Warlord = "VS_Warlord";
        public const string CampTotem = "VS_CampTotem";
        public const string Heart = "VS_VillageHeart";

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
            CreateRaider();
            CreateWarlord();
            CreateCampTotem();
            CreateVillageHeart();
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
            clone.AddComponent<SettlerRecruitable>();
            clone.AddComponent<SettlerWork>();
            clone.AddComponent<SettlerVeterancy>();
            clone.AddComponent<SettlerReputation>();
            clone.AddComponent<Party.PartyMember>();
            clone.AddComponent<SettlerEquipment>();

            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
            Jotunn.Logger.LogInfo($"Created settlement NPC prefab {name}");
        }

        /// <summary>
        /// The rival-clan bandit used by raids: hostile to players and to
        /// settlers alike, with a modest coin purse as loot.
        /// </summary>
        private static void CreateRaider()
        {
            var clone = CloneFirstAvailable(Raider, new[] { "Dverger", "DvergerMage" });
            if (clone == null)
            {
                Jotunn.Logger.LogWarning("Could not create VS_Raider: no base prefab found");
                return;
            }

            var humanoid = clone.GetComponent<Humanoid>();
            if (humanoid != null)
            {
                humanoid.m_name = "$vs_raider";
                humanoid.m_group = "vs_raiders";
                humanoid.m_boss = false;
                humanoid.m_faction = Character.Faction.Undead;
            }

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
                        m_amountMin = 5,
                        m_amountMax = 20,
                        m_chance = 1f,
                    });
                }
            }

            var npcTalk = clone.GetComponent<NpcTalk>();
            if (npcTalk != null)
            {
                Object.DestroyImmediate(npcTalk);
            }

            clone.AddComponent<RaiderDespawn>();

            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
            Jotunn.Logger.LogInfo("Created raider prefab VS_Raider");
        }

        /// <summary>
        /// The clanless warlord: a mini-boss that joins rival raids once
        /// enough camps have been cleared. Health and stars are scaled at
        /// spawn time by boss progression; killing him grants the settlement
        /// days of raid peace (see WarlordFall).
        /// </summary>
        private static void CreateWarlord()
        {
            var clone = CloneFirstAvailable(Warlord, new[] { "GoblinBrute", "Goblin", "Draugr", "Dverger" });
            if (clone == null)
            {
                Jotunn.Logger.LogWarning("Could not create VS_Warlord: no base prefab found");
                return;
            }

            var humanoid = clone.GetComponent<Humanoid>();
            if (humanoid != null)
            {
                humanoid.m_name = "$vs_warlord";
                humanoid.m_group = "vs_raiders";
                humanoid.m_boss = false;
                humanoid.m_faction = Character.Faction.Undead;
            }

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
                        m_amountMin = 80,
                        m_amountMax = 200,
                        m_chance = 1f,
                    });
                }
            }

            clone.AddComponent<RaiderDespawn>();
            clone.AddComponent<Raids.WarlordFall>();

            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
            Jotunn.Logger.LogInfo("Created warlord prefab VS_Warlord");
        }

        /// <summary>
        /// The destructible war totem of a clanless camp. Destroying it clears
        /// the camp and weakens future raids (see CampTotem).
        /// </summary>
        private static void CreateCampTotem()
        {
            var clone = CloneFirstAvailable(CampTotem, new[] { "fuling_totempole", "guard_stone", "piece_maypole" });
            if (clone == null)
            {
                Jotunn.Logger.LogWarning("Could not create VS_CampTotem: no base prefab found");
                return;
            }

            // The ward base carries ward logic we don't want on a totem.
            var privateArea = clone.GetComponent<PrivateArea>();
            if (privateArea != null)
            {
                Object.DestroyImmediate(privateArea);
            }

            clone.AddComponent<Raids.CampTotem>();

            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
            Jotunn.Logger.LogInfo("Created camp totem prefab VS_CampTotem");
        }

        /// <summary>
        /// The invisible reputation anchor of a wild village: a networked
        /// prefab stripped down to nothing but its ZNetView plus the
        /// VillageHeart behavior.
        /// </summary>
        private static void CreateVillageHeart()
        {
            var clone = CloneFirstAvailable(Heart, new[] { "guard_stone" });
            if (clone == null)
            {
                Jotunn.Logger.LogWarning("Could not create VS_VillageHeart: guard_stone prefab not found");
                return;
            }

            // Strip everything visible and interactive; only the network
            // identity remains.
            foreach (var componentType in new[] { typeof(PrivateArea), typeof(WearNTear), typeof(Piece) })
            {
                var component = clone.GetComponent(componentType);
                if (component != null)
                {
                    Object.DestroyImmediate(component);
                }
            }
            for (var i = clone.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(clone.transform.GetChild(i).gameObject);
            }
            foreach (var renderer in clone.GetComponents<Renderer>())
            {
                Object.DestroyImmediate(renderer);
            }
            foreach (var collider in clone.GetComponents<Collider>())
            {
                Object.DestroyImmediate(collider);
            }

            clone.AddComponent<VillageHeart>();

            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
            Jotunn.Logger.LogInfo("Created village heart prefab VS_VillageHeart");
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
        /// One-shot terrain ops that level the ground under a settlement when
        /// its location spawns, cloned from a vanilla terrain-modifying piece.
        /// Each settlement uses a single op sized to its whole footprint:
        /// overlapping ops fight each other - a later op's smoothing ring
        /// re-slopes ground an earlier op already leveled, terracing the site
        /// and leaving buildings buried in mounds or hovering over pits.
        /// </summary>
        private static void CreateFlatten()
        {
            // The original small op stays for the console/prefab-spawn uses.
            CreateFlattenVariant(Flatten, 13f, 18f, 6f);
            CreateFlattenVariant(FlattenVillage, 18f, 24f, 8f);
            CreateFlattenVariant(FlattenSteading, 17f, 23f, 7f);
            CreateFlattenVariant(FlattenOutpost, 11f, 16f, 6f);
            CreateFlattenVariant(FlattenCamp, 10f, 15f, 6f);
        }

        private static void CreateFlattenVariant(string name, float levelRadius, float smoothRadius, float paintRadius)
        {
            var clone = CloneFirstAvailable(name, new[] { "mud_road_v2", "path_v2", "mud_road", "path" });
            if (clone == null)
            {
                Jotunn.Logger.LogWarning($"Could not create {name}: no terrain op base prefab found");
                return;
            }

            var terrainOp = clone.GetComponent<TerrainOp>();
            if (terrainOp != null)
            {
                terrainOp.m_settings.m_level = true;
                terrainOp.m_settings.m_levelRadius = levelRadius;
                terrainOp.m_settings.m_levelOffset = 0f;
                terrainOp.m_settings.m_smooth = true;
                terrainOp.m_settings.m_smoothRadius = smoothRadius;
                terrainOp.m_settings.m_smoothPower = 3f;
                terrainOp.m_settings.m_paintCleared = true;
                terrainOp.m_settings.m_paintType = TerrainModifier.PaintType.Dirt;
                terrainOp.m_settings.m_paintRadius = paintRadius;
            }
            else
            {
                var terrainModifier = clone.GetComponent<TerrainModifier>();
                if (terrainModifier != null)
                {
                    terrainModifier.m_level = true;
                    terrainModifier.m_levelRadius = levelRadius;
                    terrainModifier.m_smooth = true;
                    terrainModifier.m_smoothRadius = smoothRadius;
                    terrainModifier.m_smoothPower = 3f;
                    terrainModifier.m_paintCleared = true;
                    terrainModifier.m_paintType = TerrainModifier.PaintType.Dirt;
                    terrainModifier.m_paintRadius = paintRadius;
                }
            }

            // Not a buildable piece, just a location helper.
            var piece = clone.GetComponent<Piece>();
            if (piece != null)
            {
                Object.DestroyImmediate(piece);
            }

            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
        }
    }
}
