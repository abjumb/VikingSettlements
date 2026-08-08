using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// Creates the buildable Settlement Banner piece. It is cloned from the
    /// ward (guard stone), stripped of its ward logic, and given the
    /// PlayerSettlement behavior plus a PlayerBase effect area so Valheim's
    /// native random events treat the settlement as a raid-able base.
    /// </summary>
    internal static class SettlementPieces
    {
        public const string Banner = "VS_SettlementBanner";
        public const string SupplyChest = "VS_BuildChest";
        public const string BuildSite = "VS_BuildSite";

        private static bool _created;

        public static void CreateAll()
        {
            if (_created)
            {
                return;
            }
            _created = true;

            CreateSupplyChest();
            CreateBuildSite();

            if (PrefabManager.Instance.GetPrefab("guard_stone") == null)
            {
                Jotunn.Logger.LogWarning("Could not create VS_SettlementBanner: guard_stone prefab not found");
                return;
            }

            var clone = PrefabManager.Instance.CreateClonedPrefab(Banner, "guard_stone");

            var privateArea = clone.GetComponent<PrivateArea>();
            if (privateArea != null)
            {
                Object.DestroyImmediate(privateArea);
            }

            clone.AddComponent<PlayerSettlement>();
            AddPlayerBaseArea(clone);

            var piece = new CustomPiece(clone, false, new PieceConfig
            {
                Name = "$vs_banner",
                Description = "$vs_banner_desc",
                PieceTable = "Hammer",
                Category = "Misc",
                CraftingStation = "piece_workbench",
                Requirements = new[]
                {
                    new RequirementConfig("Wood", 10, 0, true),
                    new RequirementConfig("FineWood", 4, 0, true),
                    new RequirementConfig("Coins", 20, 0, true),
                },
            });
            PieceManager.Instance.AddPiece(piece);
            Jotunn.Logger.LogInfo("Created buildable piece VS_SettlementBanner");
        }

        private static void CreateSupplyChest()
        {
            if (PrefabManager.Instance.GetPrefab("piece_chest_wood") == null)
            {
                Jotunn.Logger.LogWarning("Could not create VS_BuildChest: piece_chest_wood prefab not found");
                return;
            }
            var clone = PrefabManager.Instance.CreateClonedPrefab(SupplyChest, "piece_chest_wood");
            clone.AddComponent<BuildChest>();
            var container = clone.GetComponent<Container>();
            if (container != null)
            {
                container.m_name = "$vs_buildchest";
            }
            PieceManager.Instance.AddPiece(new CustomPiece(clone, false, new PieceConfig
            {
                Name = "$vs_buildchest",
                Description = "$vs_buildchest_desc",
                PieceTable = "Hammer",
                Category = "Misc",
                CraftingStation = "piece_workbench",
                Requirements = new[]
                {
                    new RequirementConfig("Wood", 10, 0, true),
                },
            }));
            Jotunn.Logger.LogInfo("Created buildable piece VS_BuildChest");
        }

        // The construction site marker is spawned by code (via a builder's
        // talk menu), not built with the hammer, so it is a plain prefab.
        private static void CreateBuildSite()
        {
            if (PrefabManager.Instance.GetPrefab("wood_stack") == null)
            {
                Jotunn.Logger.LogWarning("Could not create VS_BuildSite: wood_stack prefab not found");
                return;
            }
            var clone = PrefabManager.Instance.CreateClonedPrefab(BuildSite, "wood_stack");
            var piece = clone.GetComponent<Piece>();
            if (piece != null)
            {
                Object.DestroyImmediate(piece);
            }
            clone.AddComponent<ConstructionSite>();
            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
            Jotunn.Logger.LogInfo("Created prefab VS_BuildSite");
        }

        // Native raid events check for player-base effect areas around the
        // player; the banner provides one covering the settlement.
        private static void AddPlayerBaseArea(GameObject prefab)
        {
            var area = new GameObject("VS_PlayerBaseArea");
            area.transform.SetParent(prefab.transform, false);

            var layer = LayerMask.NameToLayer("character_trigger");
            if (layer >= 0)
            {
                area.layer = layer;
            }

            var collider = area.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = ModConfig.SettlementRadius.Value;

            var effectArea = area.AddComponent<EffectArea>();
            effectArea.m_type = EffectArea.Type.PlayerBase;
        }
    }
}
