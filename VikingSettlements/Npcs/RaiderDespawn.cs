using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Cleans up rival-clan raiders that were never dealt with: after their
    /// welcome runs out they despawn once no player is nearby to notice.
    /// </summary>
    public class RaiderDespawn : MonoBehaviour
    {
        private const float LifetimeSeconds = 600f;
        private const float PlayerCheckRange = 40f;

        private ZNetView _nview;
        private Character _character;
        private float _age;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _character = GetComponent<Character>();
        }

        private void Update()
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner())
            {
                return;
            }

            _age += Time.deltaTime;
            if (_age < LifetimeSeconds)
            {
                return;
            }
            if (_character != null && _character.IsDead())
            {
                return;
            }
            if (Player.IsPlayerInRange(transform.position, PlayerCheckRange))
            {
                return;
            }
            ZNetScene.instance.Destroy(gameObject);
        }
    }
}
