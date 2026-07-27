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

        private static bool _created;

        public static void CreateAll()
        {
            if (_created)
            {
                return;
            }
            _created = true;

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
