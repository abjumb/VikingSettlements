using System.Collections.Generic;
using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// The invisible, persistent center of a wild settlement, placed by the
    /// village layouts. It stores the village's standing toward players
    /// (-100..+100, shared by all players): earned by defending villagers and
    /// donating coins, lost by attacking them. Standing scales recruit costs;
    /// a hated village refuses to deal with you at all.
    /// Villages generated before this feature have no heart and simply behave
    /// neutrally - `spawn VS_VillageHeart` can retrofit one.
    /// </summary>
    public class VillageHeart : MonoBehaviour
    {
        public const string RepKey = "vs_rep";
        public const int MinRep = -100;
        public const int MaxRep = 100;

        /// <summary>How far from the heart a settler still belongs to the village.</summary>
        public const float VillageRadius = 48f;

        public static readonly List<VillageHeart> Instances = new List<VillageHeart>();

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

        public static VillageHeart FindNearest(Vector3 position, float maxDistance = VillageRadius)
        {
            VillageHeart best = null;
            var bestDistance = maxDistance;
            foreach (var heart in Instances)
            {
                var distance = Vector3.Distance(heart.transform.position, position);
                if (distance <= bestDistance)
                {
                    best = heart;
                    bestDistance = distance;
                }
            }
            return best;
        }

        public int Reputation => _nview != null && _nview.IsValid()
            ? _nview.GetZDO().GetInt(RepKey)
            : 0;

        public void AddReputation(int delta)
        {
            if (_nview == null || !_nview.IsValid() || delta == 0)
            {
                return;
            }
            _nview.ClaimOwnership();
            var rep = Mathf.Clamp(_nview.GetZDO().GetInt(RepKey) + delta, MinRep, MaxRep);
            _nview.GetZDO().Set(RepKey, rep);
        }

        // ---- Standing tiers ----

        public static string TierToken(int rep)
        {
            if (rep >= 50) return "$vs_rep_honored";
            if (rep >= 20) return "$vs_rep_friendly";
            if (rep <= -50) return "$vs_rep_hated";
            if (rep <= -20) return "$vs_rep_distrusted";
            return "$vs_rep_neutral";
        }

        /// <summary>Recruit cost scaling: honored villages join for half price, distrusted charge extra.</summary>
        public static float CostMultiplier(int rep)
        {
            if (rep >= 50) return 0.5f;
            if (rep >= 20) return 0.75f;
            if (rep <= -20) return 1.5f;
            return 1f;
        }

        /// <summary>A hated village's settlers refuse to be recruited.</summary>
        public static bool RefusesRecruits(int rep)
        {
            return rep <= -50;
        }
    }
}
