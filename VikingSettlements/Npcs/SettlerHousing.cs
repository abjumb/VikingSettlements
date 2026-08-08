using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Homes for assigned settlers: a home is a door (any door - built by
    /// blueprint or by the player's own hammer) the settler has been given
    /// via the talk-key panel. Stored as the door's position, so it survives
    /// the door being repaired or replaced in place; a destroyed door leaves
    /// the settler homeless again. Homeless settlers work at half speed.
    /// </summary>
    internal static class SettlerHousing
    {
        public const string HomePosKey = "vs_homepos";
        private const float DoorMatchRange = 2f;

        internal static bool HasHome(SettlerRecruitable settler)
        {
            var home = HomePosition(settler);
            return home != Vector3.zero && DoorNear(home) != null;
        }

        internal static Vector3 HomePosition(SettlerRecruitable settler)
        {
            var view = settler != null ? settler.GetComponent<ZNetView>() : null;
            if (view == null || !view.IsValid())
            {
                return Vector3.zero;
            }
            return view.GetZDO().GetVec3(HomePosKey, Vector3.zero);
        }

        internal static Door DoorNear(Vector3 position)
        {
            foreach (var door in Object.FindObjectsOfType<Door>())
            {
                if (Vector3.Distance(door.transform.position, position) <= DoorMatchRange)
                {
                    return door;
                }
            }
            return null;
        }

        internal static bool LivesAt(SettlerRecruitable settler, Vector3 doorPosition)
        {
            var home = HomePosition(settler);
            return home != Vector3.zero
                && Vector3.Distance(home, doorPosition) <= DoorMatchRange;
        }

        internal static void AssignHome(SettlerRecruitable settler, Vector3 doorPosition)
        {
            SetHome(settler, doorPosition);
        }

        internal static void ClearHome(SettlerRecruitable settler)
        {
            SetHome(settler, Vector3.zero);
        }

        private static void SetHome(SettlerRecruitable settler, Vector3 position)
        {
            var view = settler != null ? settler.GetComponent<ZNetView>() : null;
            if (view == null || !view.IsValid())
            {
                return;
            }
            view.ClaimOwnership();
            view.GetZDO().Set(HomePosKey, position);
        }
    }
}
