using System.Collections.Generic;
using UnityEngine;
using VikingSettlements.Npcs;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// Marks the buildable Builders' Supply Chest: the stockpile construction
    /// draws from. Builders take project materials only from these chests
    /// (never from food or production storage), and lumberjacks and miners
    /// refill them when an active project runs low.
    /// </summary>
    public class BuildChest : MonoBehaviour
    {
        public static readonly List<BuildChest> Instances = new List<BuildChest>();

        private Container _container;

        private void Awake()
        {
            _container = GetComponent<Container>();
        }

        private void OnEnable()
        {
            Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        /// <summary>Total of the item stocked in supply chests around the point.</summary>
        internal static int CountAround(Vector3 center, string prefabName)
        {
            var sharedName = SettlerWork.SharedName(prefabName);
            if (sharedName == null)
            {
                return 0;
            }
            var radius = ModConfig.SettlementRadius.Value;
            var count = 0;
            foreach (var chest in Instances)
            {
                var inventory = chest.InventoryIfNear(center, radius);
                if (inventory != null)
                {
                    count += inventory.CountItems(sharedName);
                }
            }
            return count;
        }

        /// <summary>Takes up to the amount from supply chests; returns what was taken.</summary>
        internal static int ConsumeAround(Vector3 center, string prefabName, int amount)
        {
            var sharedName = SettlerWork.SharedName(prefabName);
            if (sharedName == null || amount <= 0)
            {
                return 0;
            }
            var radius = ModConfig.SettlementRadius.Value;
            var taken = 0;
            foreach (var chest in Instances)
            {
                if (taken >= amount)
                {
                    break;
                }
                var inventory = chest.InventoryIfNear(center, radius);
                if (inventory == null)
                {
                    continue;
                }
                var here = Mathf.Min(inventory.CountItems(sharedName), amount - taken);
                if (here > 0)
                {
                    inventory.RemoveItem(sharedName, here);
                    taken += here;
                }
            }
            return taken;
        }

        /// <summary>Deposits the item into a supply chest with room, if any.</summary>
        internal static bool DepositAround(Vector3 center, ItemDrop.ItemData item)
        {
            var radius = ModConfig.SettlementRadius.Value;
            foreach (var chest in Instances)
            {
                var inventory = chest.InventoryIfNear(center, radius);
                if (inventory != null && inventory.CanAddItem(item))
                {
                    inventory.AddItem(item);
                    return true;
                }
            }
            return false;
        }

        internal static bool AnyAround(Vector3 center)
        {
            var radius = ModConfig.SettlementRadius.Value;
            foreach (var chest in Instances)
            {
                if (chest != null && Vector3.Distance(chest.transform.position, center) <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        private Inventory InventoryIfNear(Vector3 center, float radius)
        {
            if (_container == null || Vector3.Distance(transform.position, center) > radius)
            {
                return null;
            }
            return _container.GetInventory();
        }
    }
}
