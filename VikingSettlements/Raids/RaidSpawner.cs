using Jotunn.Managers;
using UnityEngine;
using VikingSettlements.Npcs;
using VikingSettlements.Settlements;

namespace VikingSettlements.Raids
{
    /// <summary>
    /// Spawns "rival clan" raiding parties that assault a player settlement:
    /// a group of bandits appears at the edge of the settlement and hunts the
    /// inhabitants. Settlers (player faction) fight them natively.
    /// </summary>
    internal static class RaidSpawner
    {
        /// <summary>
        /// Rival raid chance after the world-wide reduction earned by
        /// clearing clanless camps (capped at 50% total reduction).
        /// </summary>
        public static float EffectiveRaidChance()
        {
            var reduction = ModConfig.CampClearRaidReduction.Value * CampTotem.ClearedCampCount();
            return ModConfig.RivalRaidChancePerDay.Value * Mathf.Max(0.5f, 1f - reduction);
        }

        public static void SpawnRivalRaid(PlayerSettlement settlement)
        {
            var raiderPrefab = PrefabManager.Instance.GetPrefab(SettlerPrefabs.Raider);
            if (raiderPrefab == null)
            {
                return;
            }

            var center = settlement.transform.position;
            var count = Random.Range(3, 6);
            var maxLevel = 1;
            if (ModConfig.ScaleRaids.Value)
            {
                // Bigger settlements draw bigger war parties.
                count = Mathf.Clamp(3 + settlement.CountAssignedSettlers() / 3, 3, 8);
                // Raiders gain stars as the world's bosses fall.
                if (ZoneSystem.instance != null)
                {
                    if (ZoneSystem.instance.GetGlobalKey("defeated_bonemass"))
                    {
                        maxLevel = 3;
                    }
                    else if (ZoneSystem.instance.GetGlobalKey("defeated_gdking"))
                    {
                        maxLevel = 2;
                    }
                }
            }

            var angle = Random.value * 360f;
            var distance = ModConfig.SettlementRadius.Value + 12f;

            for (var i = 0; i < count; i++)
            {
                var offsetAngle = (angle + Random.Range(-20f, 20f)) * Mathf.Deg2Rad;
                var position = center + new Vector3(
                    Mathf.Sin(offsetAngle) * distance,
                    0f,
                    Mathf.Cos(offsetAngle) * distance);
                position.y = GroundHeight(position);

                var toCenter = center - position;
                toCenter.y = 0f;
                var raider = Object.Instantiate(raiderPrefab, position,
                    Quaternion.LookRotation(toCenter.normalized));

                var view = raider.GetComponent<ZNetView>();
                if (view != null && view.IsValid())
                {
                    view.GetZDO().Set(Npcs.RaiderDespawn.WarPartyKey, true);
                }
                var character = raider.GetComponent<Character>();
                if (character != null && maxLevel > 1 && Random.value < 0.2f)
                {
                    character.SetLevel(Random.Range(2, maxLevel + 1));
                }
                var ai = raider.GetComponent<MonsterAI>();
                if (ai != null)
                {
                    ai.SetHuntPlayer(true);
                    ai.Alert();
                }
            }

            Jotunn.Logger.LogInfo($"Rival clan raid: {count} raiders assault the settlement at {center}");
        }

        private static float GroundHeight(Vector3 position)
        {
            if (ZoneSystem.instance != null)
            {
                return ZoneSystem.instance.GetGroundHeight(position);
            }
            return position.y;
        }
    }
}
