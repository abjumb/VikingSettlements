using System.Collections.Generic;
using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Executes the periodic work and upkeep of an assigned settler, owner-side:
    /// - Meals: settlers eat from settlement chests (cheapest food first);
    ///   a settler that finds no food goes hungry and stops producing.
    /// - Lumberjack / Farmer: produce raw resources into settlement chests.
    /// - Blacksmith: smelts ore found in settlement chests (needs a forge).
    /// - Builder: repairs damaged build pieces (needs a workbench).
    /// - Guard: no production (combat bonuses applied by SettlerRecruitable).
    /// </summary>
    public class SettlerWork : MonoBehaviour
    {
        public const string HungryKey = "vs_hungry";
        private const string NextMealKey = "vs_nextmeal";

        private static readonly (string From, int Count, string To)[] SmeltingRecipes =
        {
            ("CopperOre", 1, "Copper"),
            ("TinOre", 1, "Tin"),
            ("IronScrap", 1, "Iron"),
            ("Wood", 1, "Coal"),
        };

        private static readonly (string From, int Count, string To)[] CookingRecipes =
        {
            ("RawMeat", 1, "CookedMeat"),
            ("DeerMeat", 1, "CookedDeerMeat"),
            ("NeckTail", 1, "NeckTailGrilled"),
            ("FishRaw", 1, "FishCooked"),
            ("WolfMeat", 1, "CookedWolfMeat"),
            ("LoxMeat", 1, "CookedLoxMeat"),
        };

        private static readonly (string From, int Count, string To)[] BrewingRecipes =
        {
            ("Honey", 2, "MeadHealthMinor"),
            ("Barley", 2, "BarleyWine"),
        };

        private ZNetView _nview;
        private SettlerRecruitable _settler;
        private Character _character;
        private float _timer;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            _settler = GetComponent<SettlerRecruitable>();
            _character = GetComponent<Character>();
        }

        private void Update()
        {
            if (_nview == null || !_nview.IsValid() || !_nview.IsOwner() || _settler == null)
            {
                return;
            }
            if (_character == null || _character.IsDead() || _settler.State != SettlerState.Assigned)
            {
                return;
            }

            HandleMeals();

            _timer += Time.deltaTime;
            if (_timer < ModConfig.WorkIntervalSeconds.Value)
            {
                return;
            }
            _timer = 0f;

            if (ModConfig.FoodUpkeep.Value && _nview.GetZDO().GetBool(HungryKey))
            {
                return; // hungry settlers down tools until their next meal
            }

            var gated = ModConfig.RequireWorkstations.Value;
            switch (_settler.Job)
            {
                case SettlerJob.Lumberjack:
                    Produce("Wood", Random.Range(2, 5));
                    break;
                case SettlerJob.Farmer:
                    Produce(Random.value < 0.5f ? "Carrot" : "Turnip", Random.Range(1, 3));
                    if (Random.value < 0.2f && (!gated || HasBeehive()))
                    {
                        Produce("Honey", 1);
                    }
                    break;
                case SettlerJob.Blacksmith:
                    if (!gated || HasStation("$piece_forge"))
                    {
                        Convert(SmeltingRecipes);
                    }
                    break;
                case SettlerJob.Builder:
                    if (!gated || HasStation("$piece_workbench"))
                    {
                        Repair();
                    }
                    break;
                case SettlerJob.Cook:
                    if (!gated || HasNearby<CookingStation>())
                    {
                        Convert(CookingRecipes);
                    }
                    break;
                case SettlerJob.Miner:
                    Produce("Stone", Random.Range(2, 5));
                    if (Random.value < 0.15f)
                    {
                        Produce(Random.value < 0.5f ? "CopperOre" : "TinOre", 1);
                    }
                    break;
                case SettlerJob.Hunter:
                    Produce("RawMeat", Random.Range(1, 3));
                    if (Random.value < 0.4f)
                    {
                        Produce("DeerHide", 1);
                    }
                    if (Random.value < 0.2f)
                    {
                        Produce("Feathers", 2);
                    }
                    break;
                case SettlerJob.Brewer:
                    if (!gated || HasNearby<Fermenter>())
                    {
                        Convert(BrewingRecipes);
                    }
                    break;
            }
        }

        // ---- Meals ----

        private void HandleMeals()
        {
            var zdo = _nview.GetZDO();
            if (!ModConfig.FoodUpkeep.Value)
            {
                if (zdo.GetBool(HungryKey))
                {
                    zdo.Set(HungryKey, false);
                }
                return;
            }
            if (ZNet.instance == null)
            {
                return;
            }

            var now = ZNet.instance.GetTimeSeconds();
            var nextMeal = zdo.GetLong(NextMealKey, 0L);
            if (nextMeal == 0L)
            {
                // Newly assigned or first run: schedule the first meal, don't eat yet.
                zdo.Set(NextMealKey, (long)(now + ModConfig.MealIntervalSeconds.Value));
                return;
            }
            if (now < nextMeal)
            {
                return;
            }

            zdo.Set(HungryKey, !TryEatOne());
            zdo.Set(NextMealKey, (long)(now + ModConfig.MealIntervalSeconds.Value));
        }

        /// <summary>Eats the cheapest food item found in settlement chests.</summary>
        private bool TryEatOne()
        {
            return ConsumeFoodAround(_settler.Home, 1);
        }

        /// <summary>
        /// Removes <paramref name="amount"/> food items from settlement chests
        /// around <paramref name="center"/>, cheapest (lowest food value)
        /// first, so settlers never eat the good meals before the humble ones.
        /// Consumes nothing unless the full amount is available.
        /// </summary>
        internal static bool ConsumeFoodAround(Vector3 center, int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            var radius = ModConfig.SettlementRadius.Value;
            var stacks = new List<(Inventory Inventory, ItemDrop.ItemData Item)>();
            foreach (var container in FindObjectsOfType<Container>())
            {
                if (Vector3.Distance(container.transform.position, center) > radius)
                {
                    continue;
                }
                var inventory = container.GetInventory();
                if (inventory == null)
                {
                    continue;
                }
                foreach (var item in inventory.GetAllItems())
                {
                    if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable
                        && item.m_shared.m_food > 0f
                        && item.m_stack > 0)
                    {
                        stacks.Add((inventory, item));
                    }
                }
            }
            stacks.Sort((a, b) => a.Item.m_shared.m_food.CompareTo(b.Item.m_shared.m_food));

            var available = 0;
            foreach (var stack in stacks)
            {
                available += stack.Item.m_stack;
            }
            if (available < amount)
            {
                return false;
            }

            var remaining = amount;
            foreach (var (inventory, item) in stacks)
            {
                var take = Mathf.Min(item.m_stack, remaining);
                inventory.RemoveItem(item, take);
                remaining -= take;
                if (remaining <= 0)
                {
                    break;
                }
            }
            return true;
        }

        // ---- Workstations ----

        private bool HasStation(string nameToken)
        {
            var radius = ModConfig.SettlementRadius.Value;
            var home = _settler.Home;
            foreach (var station in FindObjectsOfType<CraftingStation>())
            {
                if (station.m_name == nameToken
                    && Vector3.Distance(station.transform.position, home) <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasBeehive()
        {
            return HasNearby<Beehive>();
        }

        private bool HasNearby<T>() where T : Component
        {
            var radius = ModConfig.SettlementRadius.Value;
            var home = _settler.Home;
            foreach (var component in FindObjectsOfType<T>())
            {
                if (Vector3.Distance(component.transform.position, home) <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        // ---- Production ----

        private void Produce(string prefabName, int amount)
        {
            var item = MakeItem(prefabName, amount);
            if (item == null)
            {
                return;
            }
            var container = FindStorage(inventory => inventory.CanAddItem(item));
            if (container == null)
            {
                return; // no stockpile with room - the settlement needs chests
            }
            container.GetInventory().AddItem(item);
        }

        /// <summary>
        /// Runs one conversion per tick: the first recipe whose ingredients
        /// and output space are found together in a settlement chest.
        /// </summary>
        private void Convert((string From, int Count, string To)[] recipes)
        {
            foreach (var (from, needed, to) in recipes)
            {
                var fromName = SharedName(from);
                var product = MakeItem(to, 1);
                if (fromName == null || product == null)
                {
                    continue;
                }
                var container = FindStorage(inventory =>
                    inventory.CountItems(fromName) >= needed && inventory.CanAddItem(product));
                if (container == null)
                {
                    continue;
                }
                container.GetInventory().RemoveItem(fromName, needed);
                container.GetInventory().AddItem(product);
                return;
            }
        }

        private void Repair()
        {
            var radius = ModConfig.SettlementRadius.Value;
            var home = _settler.Home;
            var repaired = 0;
            foreach (var wearNTear in FindObjectsOfType<WearNTear>())
            {
                if (repaired >= 3)
                {
                    break;
                }
                if (Vector3.Distance(wearNTear.transform.position, home) > radius)
                {
                    continue;
                }
                if (wearNTear.GetHealthPercentage() < 1f && wearNTear.Repair())
                {
                    repaired++;
                }
            }
        }

        private Container FindStorage(System.Func<Inventory, bool> predicate)
        {
            var radius = ModConfig.SettlementRadius.Value;
            var home = _settler.Home;
            Container best = null;
            var bestDistance = float.MaxValue;
            foreach (var container in FindObjectsOfType<Container>())
            {
                var distance = Vector3.Distance(container.transform.position, home);
                if (distance > radius || distance >= bestDistance)
                {
                    continue;
                }
                var inventory = container.GetInventory();
                if (inventory == null || !predicate(inventory))
                {
                    continue;
                }
                best = container;
                bestDistance = distance;
            }
            return best;
        }

        private static ItemDrop.ItemData MakeItem(string prefabName, int stack)
        {
            var prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(prefabName) : null;
            var drop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
            if (drop == null)
            {
                return null;
            }
            var item = drop.m_itemData.Clone();
            item.m_dropPrefab = prefab;
            item.m_stack = stack;
            return item;
        }

        private static string SharedName(string prefabName)
        {
            var prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(prefabName) : null;
            var drop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
            return drop != null ? drop.m_itemData.m_shared.m_name : null;
        }
    }
}
