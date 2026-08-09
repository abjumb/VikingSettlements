using UnityEngine;

namespace VikingSettlements.Raids
{
    /// <summary>
    /// The war totem at the heart of a clanless camp. Destroying it counts as
    /// clearing the camp: a persistent global key is set, which reduces the
    /// rival raid chance world-wide. Clearing ten camps silences the native
    /// bandit raid event entirely.
    /// </summary>
    public class CampTotem : MonoBehaviour, Hoverable
    {
        public const int MaxCountedCamps = 10;
        private const string KeyPrefix = "vs_camp_cleared_";

        // Resolved through the location registry on first hover (the totem
        // sits a few meters off the camp center, so its own position must
        // not be hashed directly). -2 = not yet resolved.
        private int _clanIndex = -2;

        private void Awake()
        {
            var wearNTear = GetComponent<WearNTear>();
            if (wearNTear != null)
            {
                wearNTear.m_onDestroyed += OnTotemDestroyed;
                return;
            }
            var destructible = GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.m_onDestroyed += OnTotemDestroyed;
            }
        }

        private void OnTotemDestroyed()
        {
            if (ZoneSystem.instance == null)
            {
                return;
            }
            for (var i = 1; i <= MaxCountedCamps; i++)
            {
                var key = KeyPrefix + i;
                if (!ZoneSystem.instance.GetGlobalKey(key))
                {
                    ZoneSystem.instance.SetGlobalKey(key);
                    break;
                }
            }
            // Position stamp for the abduction system: a settlement holding
            // a captive from this camp can see it fell, from anywhere.
            ZoneSystem.instance.SetGlobalKey(Abduction.CampClearedKeyAt(transform.position));

            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, transform.position) < 40f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_camp_cleared"));
            }
        }

        public static int ClearedCampCount()
        {
            if (ZoneSystem.instance == null)
            {
                return 0;
            }
            var count = 0;
            for (var i = 1; i <= MaxCountedCamps; i++)
            {
                if (ZoneSystem.instance.GetGlobalKey(KeyPrefix + i))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>Global key that, once set, disables the native raid event.</summary>
        public static string AllCampsClearedKey => KeyPrefix + MaxCountedCamps;

        public string GetHoverName()
        {
            return Localization.instance.Localize("$vs_camp_totem");
        }

        public string GetHoverText()
        {
            if (_clanIndex == -2)
            {
                _clanIndex = ClanNames.IndexNear(transform.position, out _);
            }
            var clan = ClanNames.Token(_clanIndex);
            var broken = ClanNames.IsBroken(_clanIndex) ? " — $vs_clan_broken_note" : "";
            return Localization.instance.Localize(
                $"$vs_camp_totem ({clan}{broken})\n<color=orange>$vs_camp_totem_hint</color>");
        }
    }
}
