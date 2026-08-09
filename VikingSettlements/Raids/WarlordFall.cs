using UnityEngine;
using VikingSettlements.Settlements;

namespace VikingSettlements.Raids
{
    /// <summary>
    /// The payoff for killing a clanless warlord: the settlement he marched
    /// on is granted a stretch of days with no rival raids. Runs on the
    /// warlord's owner when he dies.
    /// </summary>
    public class WarlordFall : MonoBehaviour
    {
        private ZNetView _nview;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
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

        private void OnDeath()
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner() || EnvMan.instance == null)
            {
                return;
            }
            // A warlord is his clan's spine: his death breaks the clan for
            // good, permanently ending its raids (the nightly roll checks).
            var clanIndex = _nview.GetZDO().GetInt(ClanNames.ClanKey, -1);

            var settlement = PlayerSettlement.FindNearest(transform.position, 80f);
            if (settlement != null)
            {
                settlement.GrantPeace(
                    EnvMan.instance.GetCurrentDay() + ModConfig.WarlordPeaceDays.Value);
                settlement.RecordSaga($"{ClanNames.Token(clanIndex)} $vs_saga_warlord");
            }
            var shattered = clanIndex >= 0 && !ClanNames.IsBroken(clanIndex);
            if (shattered)
            {
                ClanNames.MarkBroken(clanIndex);
            }

            var player = Player.m_localPlayer;
            if (player != null
                && Vector3.Distance(player.transform.position, transform.position) < 80f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_warlord_slain"));
                if (shattered)
                {
                    player.Message(MessageHud.MessageType.TopLeft,
                        Localization.instance.Localize(
                            $"{ClanNames.Token(clanIndex)} $vs_clan_shattered"));
                }
            }
        }
    }
}
