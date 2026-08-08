using System.Collections.Generic;
using UnityEngine;
using VikingSettlements.Settlements;

namespace VikingSettlements.Npcs
{
    internal enum SettlerState
    {
        Wild = 0,
        Following = 1,
        Assigned = 2,
    }

    internal enum SettlerJob
    {
        Villager = 0,
        Lumberjack = 1,
        Farmer = 2,
        Builder = 3,
        Blacksmith = 4,
        Guard = 5,
        Cook = 6,
        Miner = 7,
        Hunter = 8,
        Brewer = 9,
    }

    /// <summary>
    /// Makes a settler recruitable and manages its state machine:
    /// wild villager -> following a player -> assigned to a player settlement
    /// with a job. All state lives in the ZDO so it persists and syncs.
    /// </summary>
    public class SettlerRecruitable : MonoBehaviour, Interactable, Hoverable
    {
        public const string StateKey = "vs_state";
        public const string OwnerKey = "vs_recruiter";
        public const string JobKey = "vs_job";
        public const string HomeKey = "vs_home";

        public static readonly List<SettlerRecruitable> Instances = new List<SettlerRecruitable>();

        private ZNetView _nview;
        private Humanoid _character;
        private MonsterAI _ai;
        private float _baseAlertRange = -1f;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _character = GetComponent<Humanoid>();
            _ai = GetComponent<MonsterAI>();
        }

        private void OnEnable()
        {
            Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        internal SettlerState State
        {
            get => _nview != null && _nview.IsValid()
                ? (SettlerState)_nview.GetZDO().GetInt(StateKey)
                : SettlerState.Wild;
            set => _nview.GetZDO().Set(StateKey, (int)value);
        }

        internal SettlerJob Job
        {
            get => _nview != null && _nview.IsValid()
                ? (SettlerJob)_nview.GetZDO().GetInt(JobKey)
                : SettlerJob.Villager;
            set => _nview.GetZDO().Set(JobKey, (int)value);
        }

        internal Vector3 Home => _nview.GetZDO().GetVec3(HomeKey, transform.position);

        internal bool IsHungry => _nview != null && _nview.IsValid()
            && _nview.GetZDO().GetBool(SettlerWork.HungryKey);

        private void Update()
        {
            if (_nview == null || !_nview.IsValid() || _character == null)
            {
                return;
            }

            var state = State;
            SyncFaction(state);
            SyncGuardSenses(state);

            if (!_nview.IsOwner() || _ai == null)
            {
                return;
            }

            if (state == SettlerState.Following && _ai.GetFollowTarget() == null)
            {
                var recruiter = FindRecruiter();
                if (recruiter != null && Vector3.Distance(recruiter.transform.position, transform.position) < 60f)
                {
                    _ai.SetFollowTarget(recruiter.gameObject);
                }
            }
            else if (state != SettlerState.Following && _ai.GetFollowTarget() != null)
            {
                _ai.SetFollowTarget(null);
            }
        }

        // Recruited settlers always side with players; wild ones follow the
        // configured default. Faction is component state (not ZDO), so every
        // client re-applies it locally.
        private void SyncFaction(SettlerState state)
        {
            var desired = state == SettlerState.Wild && !ModConfig.SettlersDefendPlayers.Value
                ? Character.Faction.Dverger
                : Character.Faction.Players;
            if (_character.m_faction != desired)
            {
                _character.m_faction = desired;
            }
        }

        private void SyncGuardSenses(SettlerState state)
        {
            if (_ai == null)
            {
                return;
            }
            if (_baseAlertRange < 0f)
            {
                _baseAlertRange = _ai.m_alertRange;
            }
            var desired = state == SettlerState.Assigned && Job == SettlerJob.Guard
                ? _baseAlertRange * 1.6f
                : _baseAlertRange;
            if (!Mathf.Approximately(_ai.m_alertRange, desired))
            {
                _ai.m_alertRange = desired;
            }
        }

        private Player FindRecruiter()
        {
            var ownerId = _nview.GetZDO().GetLong(OwnerKey);
            if (ownerId == 0L)
            {
                return null;
            }
            foreach (var player in Player.GetAllPlayers())
            {
                if (player.GetPlayerID() == ownerId)
                {
                    return player;
                }
            }
            return null;
        }

        public string GetHoverName()
        {
            if (_character == null)
            {
                return "";
            }
            return Localization.instance.Localize(
                _character.m_name + SettlerVeterancy.RankToken(_character));
        }

        public string GetHoverText()
        {
            if (_nview == null || !_nview.IsValid())
            {
                return "";
            }

            var name = GetHoverName();
            string text;
            switch (State)
            {
                case SettlerState.Wild:
                    var heart = VillageHeart.FindNearest(transform.position);
                    if (heart == null || !ModConfig.ReputationEnabled.Value)
                    {
                        text = $"{name}\n[<color=yellow><b>$KEY_Use</b></color>] $vs_recruit ({ModConfig.RecruitCostCoins.Value} $item_coins)";
                        break;
                    }
                    var rep = heart.Reputation;
                    var standing = $"$vs_rep: {VillageHeart.TierToken(rep)}";
                    var donate = $"\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $vs_donate ({ModConfig.DonationCostCoins.Value} $item_coins)";
                    if (VillageHeart.RefusesRecruits(rep))
                    {
                        text = $"{name}\n{standing}\n<color=orange>$vs_rep_refuse</color>{donate}";
                    }
                    else
                    {
                        text = $"{name}\n{standing}\n[<color=yellow><b>$KEY_Use</b></color>] $vs_recruit ({ScaledRecruitCost(rep)} $item_coins){donate}";
                    }
                    break;
                case SettlerState.Following:
                    text = $"{name} ($vs_following)\n[<color=yellow><b>$KEY_Use</b></color>] $vs_assign\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $vs_dismiss";
                    break;
                default:
                    var hungry = IsHungry ? " — $vs_hungry" : "";
                    text = $"{name} ({JobToken(Job)}{hungry})\n[<color=yellow><b>$KEY_Use</b></color>] $vs_changejob\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $vs_unassign";
                    break;
            }
            return Localization.instance.Localize(text);
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold || _nview == null || !_nview.IsValid())
            {
                return false;
            }
            var player = user as Player;
            if (player == null || _character == null || _character.IsDead())
            {
                return false;
            }

            _nview.ClaimOwnership();

            switch (State)
            {
                case SettlerState.Wild:
                    return alt ? Donate(player) : Recruit(player);
                case SettlerState.Following:
                    return alt ? Dismiss(player) : Assign(player);
                default:
                    return alt ? Unassign(player) : CycleJob(player);
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        private int ScaledRecruitCost(int rep)
        {
            return Mathf.Max(0,
                Mathf.RoundToInt(ModConfig.RecruitCostCoins.Value * VillageHeart.CostMultiplier(rep)));
        }

        private bool Recruit(Player player)
        {
            var heart = ModConfig.ReputationEnabled.Value
                ? VillageHeart.FindNearest(transform.position)
                : null;
            var cost = ModConfig.RecruitCostCoins.Value;
            if (heart != null)
            {
                if (VillageHeart.RefusesRecruits(heart.Reputation))
                {
                    player.Message(MessageHud.MessageType.Center,
                        Localization.instance.Localize("$vs_rep_refuse"));
                    return true;
                }
                cost = ScaledRecruitCost(heart.Reputation);
            }

            var coinsName = CoinsSharedName();
            if (cost > 0)
            {
                if (coinsName == null || player.GetInventory().CountItems(coinsName) < cost)
                {
                    player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$vs_needcoins"));
                    return true;
                }
                player.GetInventory().RemoveItem(coinsName, cost);
            }

            // The village notices its people leaving.
            heart?.AddReputation(-2);

            _nview.GetZDO().Set(OwnerKey, player.GetPlayerID());
            State = SettlerState.Following;
            if (_ai != null)
            {
                _ai.SetFollowTarget(player.gameObject);
            }
            player.Message(MessageHud.MessageType.Center,
                Localization.instance.Localize($"{GetHoverName()} $vs_joined"));
            return true;
        }

        private bool Donate(Player player)
        {
            if (!ModConfig.ReputationEnabled.Value)
            {
                return false;
            }
            var heart = VillageHeart.FindNearest(transform.position);
            if (heart == null)
            {
                return false;
            }

            var cost = ModConfig.DonationCostCoins.Value;
            var coinsName = CoinsSharedName();
            if (cost > 0)
            {
                if (coinsName == null || player.GetInventory().CountItems(coinsName) < cost)
                {
                    player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$vs_needcoins"));
                    return true;
                }
                player.GetInventory().RemoveItem(coinsName, cost);
            }
            heart.AddReputation(ModConfig.DonationReputation.Value);
            player.Message(MessageHud.MessageType.Center,
                Localization.instance.Localize(
                    $"$vs_donated ($vs_rep: {VillageHeart.TierToken(heart.Reputation)})"));
            return true;
        }

        private bool Dismiss(Player player)
        {
            State = SettlerState.Wild;
            _nview.GetZDO().Set(OwnerKey, 0L);
            if (_ai != null)
            {
                _ai.SetFollowTarget(null);
                _ai.SetPatrolPoint();
            }
            player.Message(MessageHud.MessageType.TopLeft,
                Localization.instance.Localize($"{GetHoverName()} $vs_dismissed"));
            return true;
        }

        private bool Assign(Player player)
        {
            var settlement = PlayerSettlement.FindNearest(transform.position, ModConfig.SettlementRadius.Value);
            if (settlement == null)
            {
                player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$vs_nosettlement"));
                return true;
            }
            if (settlement.CountAssignedSettlers() >= ModConfig.MaxSettlersPerSettlement.Value)
            {
                player.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$vs_settlementfull"));
                return true;
            }

            State = SettlerState.Assigned;
            Job = SettlerJob.Villager;
            _nview.GetZDO().Set(HomeKey, settlement.transform.position);
            if (_ai != null)
            {
                _ai.SetFollowTarget(null);
                _ai.SetPatrolPoint(settlement.transform.position);
            }
            player.Message(MessageHud.MessageType.Center,
                Localization.instance.Localize($"{GetHoverName()} $vs_assigned"));
            return true;
        }

        private bool Unassign(Player player)
        {
            State = SettlerState.Following;
            Job = SettlerJob.Villager;
            _nview.GetZDO().Set(OwnerKey, player.GetPlayerID());
            if (_ai != null)
            {
                _ai.SetFollowTarget(player.gameObject);
            }
            player.Message(MessageHud.MessageType.TopLeft,
                Localization.instance.Localize($"{GetHoverName()} $vs_following"));
            return true;
        }

        internal const int JobCount = 10;

        /// <summary>Assigns a job directly (used by interact cycling and the management panel).</summary>
        internal void SetJob(SettlerJob job)
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            _nview.ClaimOwnership();
            Job = job;
            if (_ai != null)
            {
                // Re-pin to the settlement so job changes never leave stale follow state.
                _ai.SetFollowTarget(null);
                _ai.SetPatrolPoint(Home);
            }
        }

        private bool CycleJob(Player player)
        {
            var next = (SettlerJob)(((int)Job + 1) % JobCount);
            SetJob(next);
            player.Message(MessageHud.MessageType.TopLeft,
                Localization.instance.Localize($"{GetHoverName()}: {JobToken(next)}"));
            return true;
        }

        internal static string JobToken(SettlerJob job)
        {
            switch (job)
            {
                case SettlerJob.Lumberjack: return "$vs_job_lumberjack";
                case SettlerJob.Farmer: return "$vs_job_farmer";
                case SettlerJob.Builder: return "$vs_job_builder";
                case SettlerJob.Blacksmith: return "$vs_job_blacksmith";
                case SettlerJob.Guard: return "$vs_job_guard";
                case SettlerJob.Cook: return "$vs_job_cook";
                case SettlerJob.Miner: return "$vs_job_miner";
                case SettlerJob.Hunter: return "$vs_job_hunter";
                case SettlerJob.Brewer: return "$vs_job_brewer";
                default: return "$vs_job_villager";
            }
        }

        private static string CoinsSharedName()
        {
            var prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab("Coins") : null;
            var drop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
            return drop != null ? drop.m_itemData.m_shared.m_name : null;
        }
    }
}
