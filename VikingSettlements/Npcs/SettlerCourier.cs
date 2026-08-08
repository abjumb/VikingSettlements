using System.Globalization;
using UnityEngine;
using VikingSettlements.Settlements;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// The travelling half of the Courier job: once SettlerWork loads them
    /// with cargo (see the Courier case there), this component walks them to
    /// the partner settlement, delivers into its chests, and walks them home.
    /// On the open road they can be ambushed by clanless raiders, and a
    /// courier that dies drops everything they carried. Journeys progress
    /// while the area is loaded - caravans travel when somebody is around
    /// to see them.
    /// </summary>
    public class SettlerCourier : MonoBehaviour
    {
        public const string StateKey = "vs_cstate"; // 0 home, 1 outbound, 2 returning
        public const string CargoKey = "vs_cargo";  // "prefab:count"
        public const string DestKey = "vs_cdest";

        private const float ArriveRange = 8f;
        private const float RoadRange = 40f;
        private const float TickInterval = 5f;

        private ZNetView _nview;
        private SettlerRecruitable _settler;
        private MonsterAI _ai;
        private float _nextTick;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _settler = GetComponent<SettlerRecruitable>();
            _ai = GetComponent<MonsterAI>();
            var character = GetComponent<Character>();
            if (character != null)
            {
                character.m_onDeath += OnDeath;
            }
        }

        private void OnDestroy()
        {
            var character = GetComponent<Character>();
            if (character != null)
            {
                character.m_onDeath -= OnDeath;
            }
        }

        internal int TravelState => _nview != null && _nview.IsValid()
            ? _nview.GetZDO().GetInt(StateKey)
            : 0;

        internal bool HasCargo => _nview != null && _nview.IsValid()
            && !string.IsNullOrEmpty(_nview.GetZDO().GetString(CargoKey));

        /// <summary>The nearest other settlement in courier range, if any.</summary>
        internal static PlayerSettlement FindPartner(Vector3 home)
        {
            PlayerSettlement best = null;
            var bestDistance = ModConfig.CourierRange.Value;
            foreach (var settlement in PlayerSettlement.Instances)
            {
                var distance = Vector3.Distance(settlement.transform.position, home);
                if (distance > ModConfig.SettlementRadius.Value && distance <= bestDistance)
                {
                    best = settlement;
                    bestDistance = distance;
                }
            }
            return best;
        }

        /// <summary>Loads cargo and starts the trip (called from the work tick).</summary>
        internal void Depart(Vector3 destination, string cargoPrefab, int count)
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            _nview.ClaimOwnership();
            var zdo = _nview.GetZDO();
            zdo.Set(CargoKey, cargoPrefab + ":" + count.ToString(CultureInfo.InvariantCulture));
            zdo.Set(DestKey, destination);
            zdo.Set(StateKey, 1);
            if (_ai != null)
            {
                _ai.SetPatrolPoint(destination);
            }
        }

        internal void DropCargo()
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            var zdo = _nview.GetZDO();
            var item = MakeCargoItem(zdo.GetString(CargoKey), out var count);
            if (item != null)
            {
                ItemDrop.DropItem(item, count,
                    transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
            zdo.Set(CargoKey, "");
            zdo.Set(StateKey, 0);
        }

        private void Update()
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner()
                || _settler == null)
            {
                return;
            }
            if (Time.time < _nextTick)
            {
                return;
            }
            _nextTick = Time.time + TickInterval;

            if (_settler.State != SettlerState.Assigned || _settler.Job != SettlerJob.Courier)
            {
                return;
            }
            var zdo = _nview.GetZDO();
            var state = zdo.GetInt(StateKey);
            if (state == 0)
            {
                return; // departures happen on the work tick
            }

            var home = _settler.Home;
            var destination = zdo.GetVec3(DestKey, home);

            if (state == 1)
            {
                if (_ai != null)
                {
                    _ai.SetPatrolPoint(destination);
                }
                if (Vector3.Distance(transform.position, destination) <= ArriveRange)
                {
                    Deliver(destination);
                    zdo.Set(StateKey, 2);
                    if (_ai != null)
                    {
                        _ai.SetPatrolPoint(home);
                    }
                }
            }
            else
            {
                if (_ai != null)
                {
                    _ai.SetPatrolPoint(home);
                }
                if (Vector3.Distance(transform.position, home) <= ArriveRange)
                {
                    zdo.Set(StateKey, 0);
                }
            }

            TryAmbush(home, destination);
        }

        private void Deliver(Vector3 destination)
        {
            var zdo = _nview.GetZDO();
            var item = MakeCargoItem(zdo.GetString(CargoKey), out var count);
            zdo.Set(CargoKey, "");
            if (item == null)
            {
                return;
            }
            item.m_stack = count;
            var container = SettlerWork.FindStorageAround(destination,
                inventory => inventory.CanAddItem(item));
            if (container != null)
            {
                container.GetInventory().AddItem(item);
            }
            else
            {
                // No room at the destination: leave the goods at the gate.
                ItemDrop.DropItem(item, count,
                    transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }

        // The road is dangerous: away from both settlements, a travelling
        // courier can draw clanless attention.
        private void TryAmbush(Vector3 home, Vector3 destination)
        {
            if (ModConfig.CourierAmbushChance.Value <= 0f
                || Vector3.Distance(transform.position, home) < RoadRange
                || Vector3.Distance(transform.position, destination) < RoadRange)
            {
                return;
            }
            if (Random.value >= ModConfig.CourierAmbushChance.Value)
            {
                return;
            }
            foreach (var raider in Object.FindObjectsOfType<RaiderDespawn>())
            {
                if (Vector3.Distance(raider.transform.position, transform.position) < 30f)
                {
                    return; // one ambush at a time
                }
            }
            var prefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab(SettlerPrefabs.Raider);
            if (prefab == null)
            {
                return;
            }
            for (var i = 0; i < 2; i++)
            {
                var offset = Quaternion.Euler(0f, 120f * i - 60f, 0f) * transform.forward * 8f;
                var position = transform.position + offset;
                if (ZoneSystem.instance != null)
                {
                    position.y = ZoneSystem.instance.GetGroundHeight(position);
                }
                var raider = Object.Instantiate(prefab, position,
                    Quaternion.LookRotation(-offset.normalized));
                var view = raider.GetComponent<ZNetView>();
                if (view != null && view.IsValid())
                {
                    view.GetZDO().Set(RaiderDespawn.WarPartyKey, true);
                }
                var ai = raider.GetComponent<MonsterAI>();
                if (ai != null)
                {
                    ai.Alert();
                }
            }
            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, transform.position) < 60f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_courier_ambush"));
            }
        }

        private void OnDeath()
        {
            if (_nview != null && _nview.IsValid() && _nview.IsOwner() && HasCargo)
            {
                DropCargo();
            }
        }

        private static ItemDrop.ItemData MakeCargoItem(string cargo, out int count)
        {
            count = 0;
            if (string.IsNullOrEmpty(cargo))
            {
                return null;
            }
            var parts = cargo.Split(':');
            if (parts.Length < 2
                || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out count)
                || count <= 0)
            {
                return null;
            }
            var item = SettlerWork.MakeItem(parts[0], count);
            return item;
        }
    }
}
