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
        public static void SpawnRivalRaid(PlayerSettlement settlement)
        {
            var raiderPrefab = PrefabManager.Instance.GetPrefab(SettlerPrefabs.Raider);
            if (raiderPrefab == null)
            {
                return;
            }

            var center = settlement.transform.position;
            var count = Random.Range(3, 6);
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
