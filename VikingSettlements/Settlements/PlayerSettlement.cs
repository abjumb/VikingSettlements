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
    public class PlayerSettlement : MonoBehaviour, Hoverable, Interactable, TextReceiver
    {
        private const string LastRaidDayKey = "vs_lastraid";
        private const string PendingRaidKey = "vs_nextraid";
        private const string NameKey = "vs_name";
        public const string TierKey = "vs_tier";
        public const string PeaceDayKey = "vs_peaceday";
        private const int NameCharLimit = 30;

        public static readonly List<PlayerSettlement> Instances = new List<PlayerSettlement>();

        private ZNetView _nview;
        private float _captiveTimer;

        /// <summary>The banner's network view, for systems that keep state on it (abductions).</summary>
        internal ZNetView View => _nview;

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

        /// <summary>The loaded settlers assigned to this settlement, sorted by name.</summary>
        internal List<SettlerRecruitable> GetSettlers()
        {
            var radius = ModConfig.SettlementRadius.Value;
            var settlers = new List<SettlerRecruitable>();
            foreach (var settler in SettlerRecruitable.Instances)
            {
                if (settler.State == SettlerState.Assigned
                    && Vector3.Distance(settler.Home, transform.position) <= radius)
                {
                    settlers.Add(settler);
                }
            }
            settlers.Sort((a, b) => string.CompareOrdinal(a.GetHoverName(), b.GetHoverName()));
            return settlers;
        }

        public int CountAssignedSettlers()
        {
            return GetSettlers().Count;
        }

        /// <summary>Hamlet (1) → Village (2) → Town (3). Permanent once earned.</summary>
        internal int Tier => !ModConfig.TiersEnabled.Value ? 2
            : _nview != null && _nview.IsValid()
                ? Mathf.Clamp(_nview.GetZDO().GetInt(TierKey, 1), 1, 3)
                : 1;

        /// <summary>The settler cap for the current tier (config value = Village).</summary>
        internal int SettlerCap
        {
            get
            {
                var baseline = ModConfig.MaxSettlersPerSettlement.Value;
                switch (Tier)
                {
                    case 1: return Mathf.Max(3, baseline * 3 / 5);
                    case 3: return baseline + baseline / 2;
                    default: return baseline;
                }
            }
        }

        internal static string TierToken(int tier)
        {
            switch (tier)
            {
                case 3: return "$vs_tier3";
                case 2: return "$vs_tier2";
                default: return "$vs_tier1";
            }
        }

        /// <summary>Whether rival raids are suspended (warlord recently slain).</summary>
        internal bool InPeace(int day)
        {
            return _nview != null && _nview.IsValid()
                && _nview.GetZDO().GetInt(PeaceDayKey) > day;
        }

        internal void GrantPeace(int untilDay)
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            _nview.ClaimOwnership();
            _nview.GetZDO().Set(PeaceDayKey, untilDay);
        }

        private Dictionary<SettlerJob, int> CountJobs()
        {
            var jobs = new Dictionary<SettlerJob, int>();
            foreach (var settler in GetSettlers())
            {
                jobs.TryGetValue(settler.Job, out var count);
                jobs[settler.Job] = count + 1;
            }
            return jobs;
        }

        /// <summary>The player-given settlement name, or the localized default.</summary>
        public string DisplayName
        {
            get
            {
                var name = _nview != null && _nview.IsValid()
                    ? _nview.GetZDO().GetString(NameKey)
                    : "";
                return string.IsNullOrEmpty(name)
                    ? Localization.instance.Localize("$vs_banner")
                    : name;
            }
        }

        public string GetText()
        {
            return _nview != null && _nview.IsValid() ? _nview.GetZDO().GetString(NameKey) : "";
        }

        public void SetText(string text)
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            text = text == null ? "" : text.Trim();
            if (text.Length > NameCharLimit)
            {
                text = text.Substring(0, NameCharLimit);
            }
            _nview.ClaimOwnership();
            _nview.GetZDO().Set(NameKey, text);
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

            // Captives don't wait for nightfall: rescue (or loss) resolves
            // within moments of the totem falling or the deadline passing.
            _captiveTimer += Time.deltaTime;
            if (_captiveTimer >= 5f)
            {
                _captiveTimer = 0f;
                Raids.Abduction.CheckCaptive(this, EnvMan.instance.GetCurrentDay());
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

            RollRaid(day);
            TryGrow();
            TryPromote();
            Npcs.SettlerFamily.NightlyTick(this);
        }

        // With a seer in the settlement, a successful raid roll is foreseen a
        // night ahead: tonight the warning, tomorrow the war party. Without
        // one, the raid lands the night it is rolled, as ever.
        private void RollRaid(int day)
        {
            if (!ModConfig.EnableRaids.Value)
            {
                return;
            }
            var zdo = _nview.GetZDO();
            var pending = zdo.GetInt(PendingRaidKey, -1);
            if (InPeace(day))
            {
                if (pending >= 0)
                {
                    zdo.Set(PendingRaidKey, -1); // a warlord's peace unmakes omens
                }
                return;
            }
            if (pending >= 0 && pending <= day)
            {
                zdo.Set(PendingRaidKey, -1);
                Raids.RaidSpawner.SpawnRivalRaid(this);
                return;
            }
            if (pending >= 0 || Random.value >= Raids.RaidSpawner.EffectiveRaidChance())
            {
                return;
            }
            if (!HasSeer())
            {
                Raids.RaidSpawner.SpawnRivalRaid(this);
                return;
            }
            zdo.Set(PendingRaidKey, day + 1);
            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, transform.position) < 60f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_seer_warning"));
            }
        }

        private bool HasSeer()
        {
            foreach (var settler in GetSettlers())
            {
                if (settler.gameObject.name.StartsWith(SettlerPrefabs.Seer))
                {
                    return true;
                }
            }
            return false;
        }

        // Promotion is a head-count and infrastructure check, once per night:
        // Hamlet -> Village needs people and a workbench; Village -> Town
        // needs a real population and a forge. Tiers never regress.
        private void TryPromote()
        {
            if (!ModConfig.TiersEnabled.Value || _nview == null || !_nview.IsValid())
            {
                return;
            }
            var tier = Mathf.Clamp(_nview.GetZDO().GetInt(TierKey, 1), 1, 3);
            var assigned = CountAssignedSettlers();
            var baseline = ModConfig.MaxSettlersPerSettlement.Value;
            var promoted = false;
            if (tier == 1 && assigned >= Mathf.Max(4, baseline / 2)
                && SettlerWork.HasStationAround(transform.position, "$piece_workbench"))
            {
                tier = 2;
                promoted = true;
            }
            else if (tier == 2 && assigned >= Mathf.Max(6, baseline * 4 / 5)
                && SettlerWork.HasStationAround(transform.position, "$piece_forge"))
            {
                tier = 3;
                promoted = true;
            }
            if (!promoted)
            {
                return;
            }
            _nview.GetZDO().Set(TierKey, tier);
            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, transform.position) < 50f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize(
                        $"{DisplayName} $vs_promoted {TierToken(tier)}!"));
            }
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
            // Families put down roots: settlements with couples grow faster,
            // and their newcomers are sometimes children come of age.
            var couples = SettlerFamily.CountCouples(this);
            var chance = ModConfig.GrowthChancePerDay.Value * (couples > 0 ? 1.5f : 1f);
            if (Random.value >= chance)
            {
                return;
            }
            var assigned = CountAssignedSettlers();
            if (assigned >= SettlerCap)
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

            SpawnNewcomer(couples > 0 && Random.value < 0.5f);
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

        private void SpawnNewcomer(bool bornHere = false)
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

            if (bornHere)
            {
                var player = Player.m_localPlayer;
                if (player != null
                    && Vector3.Distance(player.transform.position, center) < 50f)
                {
                    player.Message(MessageHud.MessageType.Center,
                        Localization.instance.Localize("$vs_child"));
                }
            }
            Jotunn.Logger.LogInfo(bornHere
                ? $"A settlement child came of age at {center}"
                : $"A newcomer joined the settlement at {center}");
        }

        public string GetHoverName()
        {
            return DisplayName;
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
                $"{DisplayName} ({TierToken(Tier)})"
                + $"\n$vs_settlers: {total}/{SettlerCap}{breakdown}{hungryLine}"
                + Raids.Abduction.HoverLine(this)
                + "\n[<color=yellow><b>$KEY_Use</b></color>] $vs_manage"
                + "\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $vs_rename");
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold)
            {
                return false;
            }
            var player = user as Player;
            if (player == null)
            {
                return false;
            }
            if (alt)
            {
                if (TextInput.instance != null)
                {
                    TextInput.instance.RequestText(this, "$vs_rename_topic", NameCharLimit);
                }
                return true;
            }
            SettlementPanel.Toggle(this);
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}
