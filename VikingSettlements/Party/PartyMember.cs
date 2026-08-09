using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using VikingSettlements.Npcs;

namespace VikingSettlements.Party
{
    /// <summary>
    /// Per-settler party behavior: stance handling, out-of-combat recovery,
    /// the gravely-wounded auto-retreat, and the owner-proximity flag the
    /// damage contract patch reads. Every settler carries this component but
    /// it only acts while the settler is a flagged party member.
    /// </summary>
    public class PartyMember : MonoBehaviour
    {
        public static readonly List<PartyMember> Instances = new List<PartyMember>();

        private const float RegenDelaySeconds = 10f;
        private const float GravelyWoundedFraction = 0.25f;
        private const float RecoveredFraction = 0.5f;
        private const float TickInterval = 0.5f;

        private ZNetView _nview;
        private Character _character;
        private MonsterAI _ai;
        private SettlerRecruitable _settler;
        private float _lastDamageTime = -1000f;
        private float _nextTick;
        private bool _autoFellBack;

        /// <summary>
        /// Whether the recruiter is close enough for this member's fate to be
        /// in play. Updated owner-side, and the damage patch also runs on the
        /// owning machine, so no synchronization is needed. Defaults to false:
        /// when in doubt, the member is protected.
        /// </summary>
        internal bool OwnerNearby { get; private set; }

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _character = GetComponent<Character>();
            _ai = GetComponent<MonsterAI>();
            _settler = GetComponent<SettlerRecruitable>();
            if (_character != null)
            {
                _character.m_onDamaged += OnDamaged;
            }
        }

        private void OnDestroy()
        {
            if (_character != null)
            {
                _character.m_onDamaged -= OnDamaged;
            }
        }

        private void OnEnable()
        {
            Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        internal bool IsActiveMember => _nview != null && _nview.IsValid()
            && _settler != null && _settler.State == SettlerState.Following
            && _nview.GetZDO().GetBool(PartySystem.PartyKey);

        internal PartyStance Stance => _nview != null && _nview.IsValid()
            ? (PartyStance)_nview.GetZDO().GetInt(PartySystem.StanceKey)
            : PartyStance.Follow;

        internal ZDOID Id => _nview != null && _nview.IsValid()
            ? _nview.GetZDO().m_uid
            : ZDOID.None;

        internal string MemberName => _character != null ? _character.m_name : "";

        internal long RecruiterId => _nview != null && _nview.IsValid()
            ? _nview.GetZDO().GetLong(SettlerRecruitable.OwnerKey)
            : 0L;

        internal bool IsDead => _character == null || _character.IsDead();

        internal float HealthFraction => _character != null ? _character.GetHealthPercentage() : 1f;

        internal static PartyMember FindById(ZDOID id)
        {
            foreach (var member in Instances)
            {
                if (member._nview != null && member._nview.IsValid()
                    && member._nview.GetZDO().m_uid == id)
                {
                    return member;
                }
            }
            return null;
        }

        private void Update()
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner())
            {
                return;
            }
            if (!IsActiveMember)
            {
                return;
            }

            // A falling-back member must not pick fights; clearing the target
            // every frame beats MonsterAI re-acquiring one between ticks.
            if (Stance == PartyStance.Fallback && _ai != null && _ai.m_targetCreature != null)
            {
                _ai.m_targetCreature = null;
            }

            if (Time.time < _nextTick)
            {
                return;
            }
            _nextTick = Time.time + TickInterval;

            UpdateOwnerNearby();
            ApplyStanceAI();
            Regen();
            AutoFallback();
        }

        private void UpdateOwnerNearby()
        {
            var ownerId = _nview.GetZDO().GetLong(SettlerRecruitable.OwnerKey);
            OwnerNearby = false;
            if (ownerId == 0L)
            {
                return;
            }
            foreach (var player in Player.GetAllPlayers())
            {
                // A dead owner does not count: the moment you fall, your
                // party is out of the fight too.
                if (player.GetPlayerID() == ownerId && !player.IsDead()
                    && Vector3.Distance(player.transform.position, transform.position) < PartySystem.GuardDistance)
                {
                    OwnerNearby = true;
                    return;
                }
            }
        }

        private void ApplyStanceAI()
        {
            if (_ai == null)
            {
                return;
            }
            if (Stance == PartyStance.Hold && _ai.GetFollowTarget() != null)
            {
                _ai.SetFollowTarget(null);
                _ai.SetPatrolPoint();
            }
        }

        // Out of combat, members recover on their own: stakes live inside the
        // fight, not as an attrition tax dragged between fights.
        private void Regen()
        {
            var rate = ModConfig.PartyRegenPerSecond.Value;
            if (rate <= 0f || _character == null || _character.IsDead())
            {
                return;
            }
            if (Time.time - _lastDamageTime < RegenDelaySeconds)
            {
                return;
            }
            var health = _character.GetHealth();
            var max = _character.GetMaxHealth();
            if (health >= max)
            {
                return;
            }
            _character.SetHealth(Mathf.Min(max, health + rate * TickInterval));
        }

        // The telegraphed near-death behavior: a gravely wounded member stops
        // fighting and retreats to its owner, once per wounding episode so a
        // deliberate re-engage order sticks.
        private void AutoFallback()
        {
            if (!ModConfig.PartyAutoFallback.Value || _character == null)
            {
                return;
            }
            var fraction = HealthFraction;
            if (fraction > RecoveredFraction)
            {
                _autoFellBack = false;
                return;
            }
            if (fraction < GravelyWoundedFraction && !_autoFellBack && Stance != PartyStance.Fallback)
            {
                _autoFellBack = true;
                SetStance(PartyStance.Fallback, FindOwnerPlayer());
            }
        }

        private Player FindOwnerPlayer()
        {
            var ownerId = _nview.GetZDO().GetLong(SettlerRecruitable.OwnerKey);
            foreach (var player in Player.GetAllPlayers())
            {
                if (player.GetPlayerID() == ownerId)
                {
                    return player;
                }
            }
            return null;
        }

        private void OnDamaged(float damage, Character attacker)
        {
            _lastDamageTime = Time.time;
        }

        internal void SetStance(PartyStance stance, Player owner)
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            _nview.ClaimOwnership();
            _nview.GetZDO().Set(PartySystem.StanceKey, (int)stance);
            if (_ai == null)
            {
                return;
            }
            if (stance == PartyStance.Hold)
            {
                _ai.SetFollowTarget(null);
                _ai.SetPatrolPoint();
            }
            else
            {
                if (owner != null)
                {
                    _ai.SetFollowTarget(owner.gameObject);
                }
                if (stance == PartyStance.Fallback)
                {
                    _ai.m_targetCreature = null;
                    _ai.SetAlerted(false);
                }
            }
        }

        /// <summary>Hold at a rally standard instead of in place: walk there, guard there.</summary>
        internal void RallyTo(Vector3 position, Player owner)
        {
            SetStance(PartyStance.Hold, owner);
            if (_ai != null)
            {
                _ai.SetPatrolPoint(position);
                _ai.Alert();
            }
        }

        /// <summary>The focus-fire command: drop everything, attack this one.</summary>
        internal void OrderAttack(Character target)
        {
            if (_ai == null || target == null || target.IsDead())
            {
                return;
            }
            if (_nview != null && _nview.IsValid())
            {
                _nview.ClaimOwnership();
            }
            _ai.m_targetCreature = target;
            _ai.Alert();
        }

        internal void MarkMember(Player owner)
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            _nview.ClaimOwnership();
            var zdo = _nview.GetZDO();
            zdo.Set(PartySystem.PartyKey, true);
            zdo.Set(PartySystem.StanceKey, (int)PartyStance.Follow);
            if (_ai != null && owner != null)
            {
                _ai.SetFollowTarget(owner.gameObject);
            }
        }

        internal void ClearMember()
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            _nview.ClaimOwnership();
            var zdo = _nview.GetZDO();
            zdo.Set(PartySystem.PartyKey, false);
            zdo.Set(PartySystem.StanceKey, (int)PartyStance.Follow);
        }

        internal void WarpTo(Vector3 position)
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            _nview.ClaimOwnership();
            transform.position = position;
            var body = GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = position;
            }
            _nview.GetZDO().SetPosition(position);
        }

        /// <summary>
        /// Everything needed to rebuild this member from the player save:
        /// prefab, personal name, health, star level, veterancy XP, and the
        /// five equipment slots (specs contain no field separators).
        /// </summary>
        internal string SerializeStow()
        {
            var prefabName = gameObject.name.Replace("(Clone)", "");
            var hp = _character != null ? _character.GetHealth() : 0f;
            var level = _character != null ? _character.GetLevel() : 1;
            var zdo = _nview.GetZDO();
            var xp = zdo.GetInt(SettlerVeterancy.XpKey);
            var fields = new[]
            {
                "S", prefabName, MemberName,
                hp.ToString("F1", CultureInfo.InvariantCulture),
                level.ToString(CultureInfo.InvariantCulture),
                xp.ToString(CultureInfo.InvariantCulture),
                zdo.GetString(SettlerEquipment.SlotKeys[0]),
                zdo.GetString(SettlerEquipment.SlotKeys[1]),
                zdo.GetString(SettlerEquipment.SlotKeys[2]),
                zdo.GetString(SettlerEquipment.SlotKeys[3]),
                zdo.GetString(SettlerEquipment.SlotKeys[4]),
            };
            return string.Join("|", fields);
        }

        internal void DespawnStowed()
        {
            if (_nview == null || !_nview.IsValid() || ZNetScene.instance == null)
            {
                return;
            }
            _nview.ClaimOwnership();
            ZNetScene.instance.Destroy(gameObject);
        }
    }
}
