using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Feeds the village reputation from what happens to a wild settler:
    /// - A player hurting them costs standing; killing them costs a lot.
    /// - A monster hurting them while a player is nearby earns standing -
    ///   the village saw you stand with them.
    /// Only wild settlers report to their village; recruited settlers left it.
    /// </summary>
    public class SettlerReputation : MonoBehaviour
    {
        private const int PlayerHitPenalty = -5;
        private const int PlayerKillPenalty = -25;
        private const int DefenseReward = 1;
        private const float PlayerHitCooldown = 5f;
        private const float DefenseCooldown = 60f;
        private const float DefenderRange = 40f;
        private const float KillAttributionWindow = 10f;

        private ZNetView _nview;
        private Character _character;
        private SettlerRecruitable _settler;
        private float _playerHitCooldown;
        private float _defenseCooldown;
        private float _lastPlayerHitTime = -1000f;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _character = GetComponent<Character>();
            _settler = GetComponent<SettlerRecruitable>();
            if (_character != null)
            {
                _character.m_onDamaged += OnDamaged;
                _character.m_onDeath += OnDeath;
            }
        }

        private void OnDestroy()
        {
            if (_character != null)
            {
                _character.m_onDamaged -= OnDamaged;
                _character.m_onDeath -= OnDeath;
            }
        }

        private void Update()
        {
            if (_playerHitCooldown > 0f)
            {
                _playerHitCooldown -= Time.deltaTime;
            }
            if (_defenseCooldown > 0f)
            {
                _defenseCooldown -= Time.deltaTime;
            }
        }

        private bool Tracks()
        {
            return ModConfig.ReputationEnabled.Value
                   && _nview != null && _nview.IsValid() && _nview.IsOwner()
                   && _settler != null && _settler.State == SettlerState.Wild;
        }

        private void OnDamaged(float damage, Character attacker)
        {
            if (!Tracks() || attacker == null || damage <= 0f)
            {
                return;
            }

            if (attacker.IsPlayer())
            {
                _lastPlayerHitTime = Time.time;
                if (_playerHitCooldown <= 0f)
                {
                    _playerHitCooldown = PlayerHitCooldown;
                    VillageHeart.FindNearest(transform.position)?.AddReputation(PlayerHitPenalty);
                }
                return;
            }

            // Attacked by a monster: if a player is close, they stood with us.
            if (_defenseCooldown <= 0f && Player.IsPlayerInRange(transform.position, DefenderRange))
            {
                _defenseCooldown = DefenseCooldown;
                VillageHeart.FindNearest(transform.position)?.AddReputation(DefenseReward);
            }
        }

        private void OnDeath()
        {
            if (!Tracks())
            {
                return;
            }
            if (Time.time - _lastPlayerHitTime <= KillAttributionWindow)
            {
                VillageHeart.FindNearest(transform.position)?.AddReputation(PlayerKillPenalty);
            }
        }
    }
}
