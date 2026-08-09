using UnityEngine;

namespace VikingSettlements.Raids
{
    /// <summary>
    /// Gives the clanless their names: every bandit camp belongs to one of
    /// eight rival clans, chosen deterministically from the camp location's
    /// position so every client and every session agrees without any sync.
    /// A settlement is raided by the clan of its nearest camp; felling that
    /// clan's warlord breaks the clan for good (a persistent global key),
    /// silencing its raids on every settlement it threatened.
    /// </summary>
    internal static class ClanNames
    {
        public const int ClanCount = 8;

        /// <summary>ZDO key a spawned warlord carries his clan index under.</summary>
        public const string ClanKey = "vs_clan";

        private const string BrokenKeyPrefix = "vs_clan_broken_";

        /// <summary>
        /// The clan threatening this position: the clan of the nearest camp,
        /// or -1 when the world has no camps (raids stay anonymous). The camp
        /// is resolved through the location registry, so it works for camps
        /// in terrain that has never been loaded.
        /// </summary>
        public static int IndexNear(Vector3 position, out Vector3 campPosition)
        {
            campPosition = Vector3.zero;
            if (ZoneSystem.instance == null
                || !ZoneSystem.instance.FindClosestLocation(
                    World.SettlementLocations.ClanlessCampLocation, position, out var camp))
            {
                return -1;
            }
            campPosition = camp.m_position;
            return IndexForCamp(camp.m_position);
        }

        /// <summary>
        /// Clan of the camp centered at this location position. Only ever
        /// called with location-registry positions - hashing an arbitrary
        /// point (like a totem a few meters off center) would disagree.
        /// </summary>
        public static int IndexForCamp(Vector3 campPosition)
        {
            var hash = Mathf.RoundToInt(campPosition.x) * 73856093
                ^ Mathf.RoundToInt(campPosition.z) * 19349663;
            return (int)((uint)hash % ClanCount);
        }

        public static string Token(int index)
        {
            return index >= 0 && index < ClanCount ? $"$vs_clan{index + 1}" : "$vs_clan0";
        }

        public static bool IsBroken(int index)
        {
            return index >= 0 && ZoneSystem.instance != null
                && ZoneSystem.instance.GetGlobalKey(BrokenKeyPrefix + (index + 1));
        }

        public static void MarkBroken(int index)
        {
            if (index >= 0 && ZoneSystem.instance != null)
            {
                ZoneSystem.instance.SetGlobalKey(BrokenKeyPrefix + (index + 1));
            }
        }
    }
}
