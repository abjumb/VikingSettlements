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
            if (!ModConfig.EnableRaids.Value || EnvMan.instance == null)
            {
                return;
            }

            // Roll a rival raid once per settlement per night while loaded.
            if (!EnvMan.IsNight())
            {
                return;
            }
            var day = EnvMan.instance.GetCurrentDay();
            var lastRaidDay = _nview.GetZDO().GetInt(LastRaidDayKey, -1);
            if (day <= lastRaidDay)
            {
                return;
            }
            _nview.GetZDO().Set(LastRaidDayKey, day);

            if (Random.value < ModConfig.RivalRaidChancePerDay.Value)
            {
                Raids.RaidSpawner.SpawnRivalRaid(this);
            }
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
            return Localization.instance.Localize(
                $"$vs_banner\n$vs_settlers: {total}/{ModConfig.MaxSettlersPerSettlement.Value}{breakdown}");
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
