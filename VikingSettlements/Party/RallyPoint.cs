using UnityEngine;

namespace VikingSettlements.Party
{
    /// <summary>
    /// The plantable Rally Standard: press E and your war party walks to the
    /// banner and holds there, alert and fighting - an aggressive hold point
    /// you can place ahead of a fight instead of ordering everyone in place.
    /// Shift+E (or the usual G) releases them back to your side.
    /// </summary>
    public class RallyPoint : MonoBehaviour, Hoverable, Interactable
    {
        public string GetHoverName()
        {
            return Localization.instance.Localize("$vs_rally");
        }

        public string GetHoverText()
        {
            return Localization.instance.Localize(
                "$vs_rally"
                + "\n[<color=yellow><b>$KEY_Use</b></color>] $vs_rally_order_hint"
                + "\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $vs_rally_release_hint");
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold)
            {
                return false;
            }
            var player = user as Player;
            if (player == null || player != Player.m_localPlayer)
            {
                return false;
            }
            if (alt)
            {
                PartySystem.ReleaseParty(player);
                return true;
            }
            var rallied = PartySystem.RallyParty(player, transform.position);
            player.Message(MessageHud.MessageType.Center,
                Localization.instance.Localize(
                    rallied > 0 ? "$vs_rally_order" : "$vs_party_none"));
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}
