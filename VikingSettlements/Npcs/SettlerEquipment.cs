using System.Globalization;
using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Player-given gear for recruited settlers: one weapon, shield, helmet,
    /// chest and legs slot, stored in the ZDO as "prefab:quality:durability"
    /// so it survives reloads and the party stow cycle. The owner re-equips
    /// from the ZDO, so what a settler wears is what the save says they wear.
    /// Equipped gear drops at their corpse when they die - lost with them.
    /// </summary>
    public class SettlerEquipment : MonoBehaviour
    {
        public const int SlotCount = 5;
        public static readonly string[] SlotKeys = { "vs_eq_w", "vs_eq_s", "vs_eq_h", "vs_eq_c", "vs_eq_l" };
        public static readonly string[] SlotTokens = { "$vs_slot_weapon", "$vs_slot_shield", "$vs_slot_helmet", "$vs_slot_chest", "$vs_slot_legs" };

        private const float TickInterval = 1f;

        private ZNetView _nview;
        private Humanoid _humanoid;
        private Character _character;
        private float _nextTick;
        private readonly ItemDrop.ItemData[] _applied = new ItemDrop.ItemData[SlotCount];
        private readonly string[] _appliedSpec = new string[SlotCount];

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _humanoid = GetComponent<Humanoid>();
            _character = GetComponent<Character>();
            if (_character != null)
            {
                _character.m_onDeath += OnDeath;
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

        /// <summary>Which slot the item belongs in, or -1 if not equippable.</summary>
        internal static int SlotFor(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                return -1;
            }
            switch (item.m_shared.m_itemType)
            {
                case ItemDrop.ItemData.ItemType.OneHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
                case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
                case ItemDrop.ItemData.ItemType.Bow:
                    return 0;
                case ItemDrop.ItemData.ItemType.Shield:
                    return 1;
                case ItemDrop.ItemData.ItemType.Helmet:
                    return 2;
                case ItemDrop.ItemData.ItemType.Chest:
                    return 3;
                case ItemDrop.ItemData.ItemType.Legs:
                    return 4;
                default:
                    return -1;
            }
        }

        internal string SlotSpec(int slot)
        {
            return _nview != null && _nview.IsValid()
                ? _nview.GetZDO().GetString(SlotKeys[slot])
                : "";
        }

        /// <summary>Localized display name of the slot's item, or null when empty.</summary>
        internal string SlotDisplayName(int slot)
        {
            var item = MakeItem(SlotSpec(slot));
            if (item == null)
            {
                return null;
            }
            var name = Localization.instance.Localize(item.m_shared.m_name);
            return item.m_quality > 1 ? $"{name} ({item.m_quality}★)" : name;
        }

        /// <summary>
        /// Hands the player's item to the settler. Whatever occupied the slot
        /// goes back to the player first; refuses if their bags can't take it.
        /// </summary>
        internal bool Give(Player player, ItemDrop.ItemData item)
        {
            var slot = SlotFor(item);
            if (slot < 0 || player == null || _nview == null || !_nview.IsValid())
            {
                return false;
            }
            if (item.m_dropPrefab == null)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(SlotSpec(slot)) && !TakeBack(player, slot))
            {
                return false; // no room to swap the old piece back
            }

            if (player.IsItemEquiped(item))
            {
                player.UnequipItem(item, false);
            }
            player.GetInventory().RemoveItem(item, 1);

            _nview.ClaimOwnership();
            _nview.GetZDO().Set(SlotKeys[slot], Spec(item));
            _nextTick = 0f;
            return true;
        }

        /// <summary>Returns the slot's item to the player, if they have room.</summary>
        internal bool TakeBack(Player player, int slot)
        {
            if (player == null || _nview == null || !_nview.IsValid())
            {
                return false;
            }
            var item = MakeItem(SlotSpec(slot));
            if (item == null)
            {
                return true; // nothing there
            }
            if (!player.GetInventory().CanAddItem(item))
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_inv_full"));
                return false;
            }
            player.GetInventory().AddItem(item);
            _nview.ClaimOwnership();
            _nview.GetZDO().Set(SlotKeys[slot], "");
            _nextTick = 0f;
            return true;
        }

        private void Update()
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner() || _humanoid == null)
            {
                return;
            }
            if (Time.time < _nextTick)
            {
                return;
            }
            _nextTick = Time.time + TickInterval;

            for (var slot = 0; slot < SlotCount; slot++)
            {
                var spec = _nview.GetZDO().GetString(SlotKeys[slot]);
                if (spec != (_appliedSpec[slot] ?? ""))
                {
                    Apply(slot, spec);
                    RecalcArmor();
                }
            }
        }

        /// <summary>
        /// Armor from worn pieces. Vanilla NPCs don't derive armor from
        /// equipment (only Player overrides GetBodyArmor), so the damage
        /// patch applies this with HitData.ApplyArmor on the owner - the
        /// same machine that computes it here.
        /// </summary>
        internal float EquippedArmor { get; private set; }

        private void RecalcArmor()
        {
            var armor = 0f;
            for (var slot = 2; slot < SlotCount; slot++)
            {
                var item = _applied[slot];
                if (item != null)
                {
                    armor += item.m_shared.m_armor
                        + item.m_shared.m_armorPerLevel * Mathf.Max(0, item.m_quality - 1);
                }
            }
            EquippedArmor = armor;
        }

        private void Apply(int slot, string spec)
        {
            if (_applied[slot] != null)
            {
                _humanoid.UnequipItem(_applied[slot], false);
                _humanoid.GetInventory().RemoveItem(_applied[slot]);
                _applied[slot] = null;
            }

            if (!string.IsNullOrEmpty(spec))
            {
                var item = MakeItem(spec);
                if (item != null)
                {
                    if (slot == 0)
                    {
                        // The gift replaces their default armament outright,
                        // so the AI never switches back to the old crossbow.
                        RemoveOtherWeapons();
                    }
                    _humanoid.GetInventory().AddItem(item);
                    _humanoid.EquipItem(item, false);
                    _applied[slot] = item;
                }
            }
            else if (slot == 0 && _applied[0] == null)
            {
                // Weapon taken away: rearm with the default kit.
                _humanoid.GiveDefaultItems();
            }
            _appliedSpec[slot] = spec;
        }

        private void RemoveOtherWeapons()
        {
            var items = _humanoid.GetInventory().GetAllItems();
            for (var i = items.Count - 1; i >= 0; i--)
            {
                if (SlotFor(items[i]) == 0)
                {
                    _humanoid.UnequipItem(items[i], false);
                    _humanoid.GetInventory().RemoveItem(items[i]);
                }
            }
        }

        // Gear is part of the bet: when a settler falls, what they carried
        // lands beside them.
        private void OnDeath()
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner())
            {
                return;
            }
            var zdo = _nview.GetZDO();
            for (var slot = 0; slot < SlotCount; slot++)
            {
                var item = MakeItem(zdo.GetString(SlotKeys[slot]));
                if (item != null)
                {
                    ItemDrop.DropItem(item, 1,
                        transform.position + Vector3.up * 0.75f, Quaternion.identity);
                }
                zdo.Set(SlotKeys[slot], "");
            }
        }

        private static string Spec(ItemDrop.ItemData item)
        {
            return string.Join(":", item.m_dropPrefab.name,
                item.m_quality.ToString(CultureInfo.InvariantCulture),
                item.m_durability.ToString("F0", CultureInfo.InvariantCulture));
        }

        private static ItemDrop.ItemData MakeItem(string spec)
        {
            if (string.IsNullOrEmpty(spec))
            {
                return null;
            }
            var parts = spec.Split(':');
            var prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(parts[0]) : null;
            var drop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
            if (drop == null)
            {
                return null;
            }
            var item = drop.m_itemData.Clone();
            item.m_dropPrefab = prefab;
            item.m_stack = 1;
            if (parts.Length > 1
                && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var quality))
            {
                item.m_quality = Mathf.Max(1, quality);
            }
            if (parts.Length > 2
                && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var durability))
            {
                item.m_durability = durability;
            }
            return item;
        }
    }
}
