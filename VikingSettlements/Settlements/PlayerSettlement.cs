using System.Collections.Generic;
using UnityEngine;
using VikingSettlements.Npcs;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// Placed on the buildable settlement banner. Defines a player settlement:
    /// settlers get assigned to it, its area counts as a player base for
    /// Valheim's native raid events, and rival clans roll a nightly raid
    /// against it while its area is loaded.
    /// </summary>
    public class PlayerSettlement : MonoBehaviour, Hoverable, Interactable
    {
        private const string LastRaidDayKey = "vs_lastraid";

        public static readonly List<PlayerSettlement> Instances = new List<PlayerSettlement>();

        private ZNetView _nview;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
        }

        private void OnEnable()
        {
            Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        public static PlayerSettlement FindNearest(Vector3 position, float maxDistance)
        {
            PlayerSettlement best = null;
            var bestDistance = maxDistance;
            foreach (var settlement in Instances)
            {
                var distance = Vector3.Distance(settlement.transform.position, position);
                if (distance <= bestDistance)
                {
                    best = settlement;
                    bestDistance = distance;
                }
            }
            return best;
        }

        public int CountAssignedSettlers()
        {
            var radius = ModConfig.SettlementRadius.Value;
            var count = 0;
            foreach (var settler in SettlerRecruitable.Instances)
            {
                if (settler.State == SettlerState.Assigned
                    && Vector3.Distance(settler.Home, transform.position) <= radius)
                {
                    count++;
                }
            }
            return count;
        }

        private Dictionary<SettlerJob, int> CountJobs()
        {
            var radius = ModConfig.SettlementRadius.Value;
            var jobs = new Dictionary<SettlerJob, int>();
            foreach (var settler in SettlerRecruitable.Instances)
            {
                if (settler.State != SettlerState.Assigned
                    || Vector3.Distance(settler.Home, transform.position) > radius)
                {
                    continue;
                }
                jobs.TryGetValue(settler.Job, out var count);
                jobs[settler.Job] = count + 1;
            }
            return jobs;
        }

        private void Update()
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner())
            {
                return;
            }
            if (EnvMan.instance == null)
            {
                return;
            }

            // One tick per settlement per night while loaded: roll a rival
            // raid and a growth chance.
            if (!EnvMan.IsNight())
            {
                return;
            }
            var day = EnvMan.instance.GetCurrentDay();
            var lastTickDay = _nview.GetZDO().GetInt(LastRaidDayKey, -1);
            if (day <= lastTickDay)
            {
                return;
            }
            _nview.GetZDO().Set(LastRaidDayKey, day);

            if (ModConfig.EnableRaids.Value && Random.value < ModConfig.RivalRaidChancePerDay.Value)
            {
                Raids.RaidSpawner.SpawnRivalRaid(this);
            }
            TryGrow();
        }

        /// <summary>
        /// A settlement below its cap attracts a newcomer if it has a spare
        /// unclaimed bed and enough food in its chests to feed one.
        /// </summary>
        private void TryGrow()
        {
            if (!ModConfig.GrowthEnabled.Value)
            {
                return;
            }
            if (Random.value >= ModConfig.GrowthChancePerDay.Value)
            {
                return;
            }
            var assigned = CountAssignedSettlers();
            if (assigned >= ModConfig.MaxSettlersPerSettlement.Value)
            {
                return;
            }
            if (CountUnclaimedBeds() < assigned + 1)
            {
                return; // every settler notionally needs a bed, plus one spare
            }
            if (!SettlerWork.ConsumeFoodAround(transform.position, ModConfig.GrowthFoodCost.Value))
            {
                return; // not enough food to attract anyone
            }

            SpawnNewcomer();
        }

        private int CountUnclaimedBeds()
        {
            var radius = ModConfig.SettlementRadius.Value;
            var count = 0;
            foreach (var bed in FindObjectsOfType<Bed>())
            {
                if (bed.GetOwner() == 0L
                    && Vector3.Distance(bed.transform.position, transform.position) <= radius)
                {
                    count++;
                }
            }
            return count;
        }

        private void SpawnNewcomer()
        {
            // Seers are rare arrivals.
            var prefabName = Random.value < 0.15f ? SettlerPrefabs.Seer : SettlerPrefabs.Settler;
            var prefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab(prefabName)
                         ?? Jotunn.Managers.PrefabManager.Instance.GetPrefab(SettlerPrefabs.Settler);
            if (prefab == null)
            {
                return;
            }

            var center = transform.position;
            var angle = Random.value * 360f * Mathf.Deg2Rad;
            var distance = ModConfig.SettlementRadius.Value + 6f;
            var position = center + new Vector3(Mathf.Sin(angle) * distance, 0f, Mathf.Cos(angle) * distance);
            if (ZoneSystem.instance != null)
            {
                position.y = ZoneSystem.instance.GetGroundHeight(position);
            }

            var toCenter = center - position;
            toCenter.y = 0f;
            var newcomer = Object.Instantiate(prefab, position, Quaternion.LookRotation(toCenter.normalized));

            var view = newcomer.GetComponent<ZNetView>();
            if (view != null && view.IsValid())
            {
                view.GetZDO().Set(SettlerRecruitable.StateKey, (int)SettlerState.Assigned);
                view.GetZDO().Set(SettlerRecruitable.JobKey, (int)SettlerJob.Villager);
                view.GetZDO().Set(SettlerRecruitable.HomeKey, center);
            }
            var ai = newcomer.GetComponent<MonsterAI>();
            if (ai != null)
            {
                // Patrol home, so the newcomer walks in from the edge.
                ai.SetPatrolPoint(center);
            }

            Jotunn.Logger.LogInfo($"A newcomer joined the settlement at {center}");
        }

        public string GetHoverName()
        {
            return Localization.instance.Localize("$vs_banner");
        }

        public string GetHoverText()
        {
            var jobs = CountJobs();
            var total = 0;
            var parts = new List<string>();
            foreach (var pair in jobs)
            {
                total += pair.Value;
                parts.Add($"{pair.Value} {SettlerRecruitable.JobToken(pair.Key)}");
            }
            var breakdown = parts.Count > 0 ? "\n" + string.Join(", ", parts) : "";

            var radius = ModConfig.SettlementRadius.Value;
            var hungry = 0;
            foreach (var settler in SettlerRecruitable.Instances)
            {
                if (settler.State == SettlerState.Assigned
                    && settler.IsHungry
                    && Vector3.Distance(settler.Home, transform.position) <= radius)
                {
                    hungry++;
                }
            }
            var hungryLine = hungry > 0 ? $"\n$vs_hungry: {hungry}" : "";

            return Localization.instance.Localize(
                $"$vs_banner\n$vs_settlers: {total}/{ModConfig.MaxSettlersPerSettlement.Value}{breakdown}{hungryLine}");
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold)
            {
                return false;
            }
            var player = user as Player;
            if (player != null)
            {
                player.Message(MessageHud.MessageType.Center, GetHoverText());
            }
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}
