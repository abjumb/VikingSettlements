using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Pins a settler to its spawn position by setting the AI patrol point
    /// once, so villagers stay in their settlement instead of wandering off.
    /// </summary>
    public class SettlerHome : MonoBehaviour
    {
        private bool _applied;

        private void Update()
        {
            if (_applied)
            {
                return;
            }

            var view = GetComponent<ZNetView>();
            if (view == null || !view.IsValid())
            {
                return;
            }

            // Patrol data lives in the ZDO, so it only ever needs to be set
            // once by whoever owns the creature first.
            if (view.GetZDO().GetBool("patrol"))
            {
                _applied = true;
                return;
            }
            if (!view.IsOwner())
            {
                return;
            }

            var ai = GetComponent<MonsterAI>();
            if (ai != null)
            {
                ai.SetPatrolPoint();
            }
            _applied = true;
        }
    }
}
