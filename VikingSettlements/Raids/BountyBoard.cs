using UnityEngine;
using VikingSettlements.Npcs;

namespace VikingSettlements.Raids
{
    /// <summary>
    /// The village bounty board found in wild meadows villages: one posting
    /// at a time, either a delivery the village needs or a call to break the
    /// nearest clanless camp. Completing a bounty pays coins on the spot and
    /// raises the village's standing toward you - the cheap-recruits economy
    /// already hangs off standing, so bounties are also the honest road to
    /// discounted settlers. One posting per day.
    /// </summary>
    public class BountyBoard : MonoBehaviour, Hoverable, Interactable
    {
        private const string TypeKey = "vs_bounty_type";
        private const string ItemKey = "vs_bounty_item";
        private const string CountKey = "vs_bounty_count";
        private const string CampKey = "vs_bounty_camp";
        private const string DoneDayKey = "vs_bounty_doneday";

        private const int TypeNone = 0;
        private const int TypeCamp = 1;
        private const int TypeDelivery = 2;

        private const int CampRewardCoins = 150;
        private const int CampRewardReputation = 15;
        private const int DeliveryRewardReputation = 5;

        // What a village might ask for: prefab, count, coin reward.
        private static readonly (string Prefab, int Count, int Coins)[] Deliveries =
        {
            ("Wood", 20, 30),
            ("Stone", 15, 30),
            ("CookedMeat", 8, 40),
            ("DeerHide", 6, 40),
            ("Honey", 4, 40),
            ("FishRaw", 5, 40),
        };

        private ZNetView _nview;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
        }

        public string GetHoverName()
        {
            return Localization.instance.Localize("$vs_bounty_board");
        }

        public string GetHoverText()
        {
            if (_nview == null || !_nview.IsValid())
            {
                return "";
            }
            var zdo = _nview.GetZDO();
            string line;
            switch (zdo.GetInt(TypeKey))
            {
                case TypeCamp:
                    var camp = zdo.GetVec3(CampKey, transform.position);
                    var clan = ClanNames.Token(ClanNames.IndexForCamp(camp));
                    var distance = Mathf.RoundToInt(
                        Vector3.Distance(transform.position, camp));
                    line = $"$vs_bounty_camp_txt {clan} ({distance} m)"
                        + $"\n$vs_bounty_reward: {CampRewardCoins} $item_coins";
                    break;
                case TypeDelivery:
                    var sharedName = SettlerWork.SharedName(zdo.GetString(ItemKey));
                    var delivery = FindDelivery(zdo.GetString(ItemKey));
                    line = $"$vs_bounty_deliver {zdo.GetInt(CountKey)} × {sharedName}"
                        + $"\n$vs_bounty_reward: {delivery.Coins} $item_coins";
                    break;
                default:
                    line = "$vs_bounty_none";
                    break;
            }
            return Localization.instance.Localize(
                $"$vs_bounty_board\n{line}\n[<color=yellow><b>$KEY_Use</b></color>] $vs_bounty_use");
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold || _nview == null || !_nview.IsValid())
            {
                return false;
            }
            var player = user as Player;
            if (player == null || EnvMan.instance == null)
            {
                return false;
            }
            _nview.ClaimOwnership();
            var zdo = _nview.GetZDO();
            var day = EnvMan.instance.GetCurrentDay();

            switch (zdo.GetInt(TypeKey))
            {
                case TypeCamp:
                    return TurnInCamp(player, zdo, day);
                case TypeDelivery:
                    return TurnInDelivery(player, zdo, day);
                default:
                    return PostBounty(player, zdo, day);
            }
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        private bool PostBounty(Player player, ZDO zdo, int day)
        {
            if (day <= zdo.GetInt(DoneDayKey, -1))
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_bounty_tomorrow"));
                return true;
            }
            // Half the postings call for a camp to be broken, when there is
            // an unbroken one to point at; the rest are deliveries.
            var clanIndex = ClanNames.IndexNear(transform.position, out var camp);
            var wantCamp = clanIndex >= 0
                && !Abduction.CampClearedNear(camp)
                && Random.value < 0.5f;
            if (wantCamp)
            {
                zdo.Set(TypeKey, TypeCamp);
                zdo.Set(CampKey, camp);
            }
            else
            {
                var delivery = Deliveries[Random.Range(0, Deliveries.Length)];
                zdo.Set(TypeKey, TypeDelivery);
                zdo.Set(ItemKey, delivery.Prefab);
                zdo.Set(CountKey, delivery.Count);
            }
            player.Message(MessageHud.MessageType.Center,
                Localization.instance.Localize("$vs_bounty_new"));
            return true;
        }

        private bool TurnInCamp(Player player, ZDO zdo, int day)
        {
            var camp = zdo.GetVec3(CampKey, transform.position);
            if (!Abduction.CampClearedNear(camp))
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_bounty_notdone"));
                return true;
            }
            Complete(player, zdo, day, CampRewardCoins, CampRewardReputation);
            return true;
        }

        private bool TurnInDelivery(Player player, ZDO zdo, int day)
        {
            var prefabName = zdo.GetString(ItemKey);
            var count = zdo.GetInt(CountKey);
            var sharedName = SettlerWork.SharedName(prefabName);
            if (sharedName == null)
            {
                // The asked-for item no longer exists (game update): repost.
                zdo.Set(TypeKey, TypeNone);
                return true;
            }
            if (player.GetInventory().CountItems(sharedName) < count)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_bounty_needitems"));
                return true;
            }
            player.GetInventory().RemoveItem(sharedName, count);
            Complete(player, zdo, day, FindDelivery(prefabName).Coins, DeliveryRewardReputation);
            return true;
        }

        private void Complete(Player player, ZDO zdo, int day, int coins, int reputation)
        {
            zdo.Set(TypeKey, TypeNone);
            zdo.Set(DoneDayKey, day);

            var payment = SettlerWork.MakeItem("Coins", coins);
            if (payment != null && !player.GetInventory().AddItem(payment))
            {
                ItemDrop.DropItem(payment, coins,
                    player.transform.position + Vector3.up, Quaternion.identity);
            }
            var heart = VillageHeart.FindNearest(transform.position);
            if (heart != null && ModConfig.ReputationEnabled.Value)
            {
                heart.AddReputation(reputation);
            }
            player.Message(MessageHud.MessageType.Center,
                Localization.instance.Localize($"$vs_bounty_done (+{coins} $item_coins)"));
        }

        private static (string Prefab, int Count, int Coins) FindDelivery(string prefabName)
        {
            foreach (var delivery in Deliveries)
            {
                if (delivery.Prefab == prefabName)
                {
                    return delivery;
                }
            }
            return ("", 0, 25);
        }
    }
}
