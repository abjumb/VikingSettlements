using UnityEngine;
using VikingSettlements.Settlements;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Settler families: two housed, happy settlers of a settlement can marry
    /// (rolled nightly by the banner). Marriage is mutual state stored by
    /// partner name; a married settler whose partner is around gains a little
    /// morale each night, and a partner's death - in battle, or lost to an
    /// abduction deadline - triggers real grief. Grief only fires on a
    /// confirmed loss, never on mere absence, so travel, stowing and couriers
    /// can't fake a widowing.
    /// </summary>
    public class SettlerFamily : MonoBehaviour
    {
        public const string PartnerKey = "vs_partner";

        private const int MarriageMorale = 60;
        private const float MarriageChance = 0.25f;
        private const int WeddingJoy = 10;
        private const int TogetherBonus = 2;
        private const int GriefBlow = 30;

        private ZNetView _nview;
        private Character _character;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _character = GetComponent<Character>();
            if (_character != null)
            {
                _character.m_onDeath += OnDeath;
            }
        }

        private void OnDestroy()
        {
            if (_character != null)
            {
                _character.m_onDeath -= OnDeath;
            }
        }

        internal string Partner => _nview != null && _nview.IsValid()
            ? _nview.GetZDO().GetString(PartnerKey)
            : "";

        private void OnDeath()
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner())
            {
                return;
            }
            var partner = Partner;
            if (!string.IsNullOrEmpty(partner) && _character != null)
            {
                GrieveFor(_character.m_name, transform.position);
            }
        }

        /// <summary>
        /// Widows every loaded settler married to <paramref name="lostName"/>
        /// near the position: morale blow, marriage cleared, a mourning
        /// message. Called on death and when an abduction deadline expires.
        /// </summary>
        internal static void GrieveFor(string lostName, Vector3 position)
        {
            if (string.IsNullOrEmpty(lostName))
            {
                return;
            }
            var radius = ModConfig.SettlementRadius.Value * 2f;
            foreach (var settler in SettlerRecruitable.Instances)
            {
                var family = settler.GetComponent<SettlerFamily>();
                if (family == null || family.Partner != lostName
                    || Vector3.Distance(settler.transform.position, position) > radius)
                {
                    continue;
                }
                var view = settler.GetComponent<ZNetView>();
                if (view == null || !view.IsValid())
                {
                    continue;
                }
                view.ClaimOwnership();
                view.GetZDO().Set(PartnerKey, "");
                var morale = settler.GetComponent<SettlerMorale>();
                if (morale != null)
                {
                    morale.AddMorale(-GriefBlow);
                }

                var character = settler.GetComponent<Character>();
                var player = Player.m_localPlayer;
                if (character != null && player != null
                    && Vector3.Distance(player.transform.position, settler.transform.position) < 60f)
                {
                    player.Message(MessageHud.MessageType.TopLeft,
                        Localization.instance.Localize(
                            $"{character.m_name} $vs_grief {lostName}"));
                }
            }
        }

        /// <summary>
        /// The banner's nightly family round: married settlers with their
        /// partner present sleep easier, and two happy, housed singles may
        /// wed. Runs on the banner owner.
        /// </summary>
        internal static void NightlyTick(PlayerSettlement settlement)
        {
            if (!ModConfig.FamiliesEnabled.Value)
            {
                return;
            }
            var settlers = settlement.GetSettlers();

            // Together-bonus first: presence is checked by name against the
            // same assigned roster.
            if (ModConfig.MoraleEnabled.Value)
            {
                foreach (var settler in settlers)
                {
                    var family = settler.GetComponent<SettlerFamily>();
                    if (family == null || string.IsNullOrEmpty(family.Partner))
                    {
                        continue;
                    }
                    if (FindByName(settlers, family.Partner, settler) == null)
                    {
                        continue;
                    }
                    var morale = settler.GetComponent<SettlerMorale>();
                    if (morale != null)
                    {
                        morale.AddMorale(TogetherBonus);
                    }
                }
            }

            if (Random.value >= MarriageChance)
            {
                return;
            }
            // Candidates: single, housed, in good spirits, and - to keep the
            // name-based bookkeeping unambiguous - uniquely named tonight.
            var candidates = new System.Collections.Generic.List<SettlerRecruitable>();
            foreach (var settler in settlers)
            {
                var family = settler.GetComponent<SettlerFamily>();
                var morale = settler.GetComponent<SettlerMorale>();
                var happy = !ModConfig.MoraleEnabled.Value
                    || (morale != null && morale.Morale >= MarriageMorale);
                if (family != null && string.IsNullOrEmpty(family.Partner)
                    && happy && SettlerHousing.HasHome(settler))
                {
                    candidates.Add(settler);
                }
            }
            if (candidates.Count < 2)
            {
                return;
            }
            var first = candidates[Random.Range(0, candidates.Count)];
            candidates.Remove(first);
            var firstName = NameOf(first);
            candidates.RemoveAll(c => NameOf(c) == firstName);
            if (candidates.Count == 0)
            {
                return;
            }
            var second = candidates[Random.Range(0, candidates.Count)];
            Marry(first, second);
        }

        /// <summary>Married pairs currently present in the settlement.</summary>
        internal static int CountCouples(PlayerSettlement settlement)
        {
            if (!ModConfig.FamiliesEnabled.Value)
            {
                return 0;
            }
            var settlers = settlement.GetSettlers();
            var married = 0;
            foreach (var settler in settlers)
            {
                var family = settler.GetComponent<SettlerFamily>();
                if (family != null && !string.IsNullOrEmpty(family.Partner)
                    && FindByName(settlers, family.Partner, settler) != null)
                {
                    married++;
                }
            }
            return married / 2;
        }

        private static void Marry(SettlerRecruitable first, SettlerRecruitable second)
        {
            var firstName = NameOf(first);
            var secondName = NameOf(second);
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(secondName))
            {
                return;
            }
            foreach (var (settler, partnerName) in new[] { (first, secondName), (second, firstName) })
            {
                var view = settler.GetComponent<ZNetView>();
                if (view == null || !view.IsValid())
                {
                    continue;
                }
                view.ClaimOwnership();
                view.GetZDO().Set(PartnerKey, partnerName);
                var morale = settler.GetComponent<SettlerMorale>();
                if (morale != null)
                {
                    morale.AddMorale(WeddingJoy);
                }
            }

            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, first.transform.position) < 60f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize(
                        $"{firstName} $vs_and {secondName} $vs_wedding"));
            }
            Jotunn.Logger.LogInfo($"A settlement wedding: {firstName} and {secondName}");
        }

        private static SettlerRecruitable FindByName(
            System.Collections.Generic.List<SettlerRecruitable> settlers,
            string name, SettlerRecruitable except)
        {
            foreach (var settler in settlers)
            {
                if (settler != except && NameOf(settler) == name)
                {
                    return settler;
                }
            }
            return null;
        }

        private static string NameOf(SettlerRecruitable settler)
        {
            var character = settler.GetComponent<Character>();
            return character != null ? character.m_name : "";
        }
    }
}
