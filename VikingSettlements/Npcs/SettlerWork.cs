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
        internal const string NextMealKey = "vs_nextmeal";

        internal static readonly (string From, int Count, string To)[] SmeltingRecipes =
        {
            ("CopperOre", 1, "Copper"),
            ("TinOre", 1, "Tin"),
            ("IronScrap", 1, "Iron"),
            ("Wood", 1, "Coal"),
        };

        internal static readonly (string From, int Count, string To)[] CookingRecipes =
        {
            ("RawMeat", 1, "CookedMeat"),
            ("DeerMeat", 1, "CookedDeerMeat"),
            ("NeckTail", 1, "NeckTailGrilled"),
            ("FishRaw", 1, "FishCooked"),
            ("WolfMeat", 1, "CookedWolfMeat"),
            ("LoxMeat", 1, "CookedLoxMeat"),
        };

        internal static readonly (string From, int Count, string To)[] BrewingRecipes =
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
            if (ModConfig.HomesMatter.Value && !SettlerHousing.HasHome(_settler)
                && Random.value < 0.5f)
            {
                return; // no roof over their head: half the ticks are lost
            }
            var morale = GetComponent<SettlerMorale>();
            var moraleValue = ModConfig.MoraleEnabled.Value && morale != null ? morale.Morale : 50;
            if (moraleValue < SettlerMorale.MiserableBelow && Random.value < 0.5f)
            {
                return; // the miserable drag their feet
            }
            // Cheerful settlers put in a little extra.
            var bonus = moraleValue >= SettlerMorale.CheerfulAt ? 1 : 0;

            var gated = ModConfig.RequireWorkstations.Value;
            switch (_settler.Job)
            {
                case SettlerJob.Lumberjack:
                    var wood = Random.Range(2, 5) + bonus;
                    if (!TrySupplyConstruction("Wood", wood))
                    {
                        Produce("Wood", wood);
                    }
                    break;
                case SettlerJob.Farmer:
                    Produce(Random.value < 0.5f ? "Carrot" : "Turnip", Random.Range(1, 3) + bonus);
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
                        // Construction first; repairs while nothing is ordered.
                        var site = Settlements.ConstructionSite.FindNear(_settler.Home);
                        if (site != null)
                        {
                            site.BuildTick();
                        }
                        else
                        {
                            Repair();
                        }
                    }
                    break;
                case SettlerJob.Cook:
                    if (!gated || HasNearby<CookingStation>())
                    {
                        Convert(CookingRecipes);
                    }
                    break;
                case SettlerJob.Miner:
                    var stone = Random.Range(2, 5) + bonus;
                    if (!TrySupplyConstruction("Stone", stone))
                    {
                        Produce("Stone", stone);
                    }
                    if (Random.value < 0.15f)
                    {
                        Produce(Random.value < 0.5f ? "CopperOre" : "TinOre", 1);
                    }
                    break;
                case SettlerJob.Hunter:
                    Produce("RawMeat", Random.Range(1, 3) + bonus);
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
                case SettlerJob.Courier:
                    TryDepartWithCargo();
                    break;
                case SettlerJob.Herder:
                    HerdPen();
                    break;
            }
        }

        // ---- Courier departures (the journey itself runs in SettlerCourier) ----

        // Goods a courier considers surplus worth hauling.
        private static readonly string[] TradeGoods =
        {
            "Wood", "Stone", "Coal", "Copper", "Tin", "Bronze", "Iron",
            "DeerHide", "LeatherScraps", "Carrot", "Turnip", "Honey", "RawMeat",
        };

        private void TryDepartWithCargo()
        {
            var courier = GetComponent<SettlerCourier>();
            if (courier == null || courier.TravelState != 0 || courier.HasCargo)
            {
                return;
            }
            var partner = SettlerCourier.FindPartner(_settler.Home);
            if (partner == null)
            {
                return;
            }

            // Haul the most-stocked trade good, leaving a reserve of 10 behind.
            string bestName = null;
            var bestCount = 0;
            foreach (var prefabName in TradeGoods)
            {
                var count = CountItemAround(_settler.Home, prefabName);
                if (count > bestCount)
                {
                    bestCount = count;
                    bestName = prefabName;
                }
            }
            if (bestName == null || bestCount <= 10)
            {
                return;
            }
            var haul = Mathf.Min(8, bestCount - 10);
            if (TakeItemsAround(_settler.Home, bestName, haul) < haul)
            {
                return;
            }
            courier.Depart(partner.transform.position, bestName, haul);
        }

        // ---- Herding ----

        private void HerdPen()
        {
            var radius = ModConfig.SettlementRadius.Value;
            var home = _settler.Home;
            var animals = new List<Character>();
            foreach (var tameable in FindObjectsOfType<Tameable>())
            {
                var animal = tameable.GetComponent<Character>();
                if (animal != null && animal.IsTamed() && !animal.IsDead()
                    && Vector3.Distance(animal.transform.position, home) <= radius)
                {
                    animals.Add(animal);
                }
            }
            if (animals.Count == 0)
            {
                return;
            }

            // Feed: drop one vegetable by an animal, unless feed already lies out.
            if (!LooseFeedAround(home, radius))
            {
                foreach (var feed in new[] { "Carrot", "Turnip" })
                {
                    if (TakeItemsAround(home, feed, 1) == 1)
                    {
                        var item = MakeItem(feed, 1);
                        if (item != null)
                        {
                            ItemDrop.DropItem(item, 1,
                                animals[0].transform.position + Vector3.up * 0.5f,
                                Quaternion.identity);
                        }
                        break;
                    }
                }
            }

            // Cull: past four head, one goes to the larder (vanilla drops).
            if (animals.Count > 4)
            {
                animals[animals.Count - 1].SetHealth(0f);
            }

            // Collect: carry loose non-food drops in the pen area to storage.
            var collected = 0;
            foreach (var drop in FindObjectsOfType<ItemDrop>())
            {
                if (collected >= 3
                    || drop.m_itemData == null
                    || drop.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable
                    || drop.m_itemData.m_shared.m_food > 0f
                    || Vector3.Distance(drop.transform.position, home) > radius)
                {
                    continue;
                }
                var container = FindStorageAround(home,
                    inventory => inventory.CanAddItem(drop.m_itemData));
                if (container == null)
                {
                    break;
                }
                container.GetInventory().AddItem(drop.m_itemData.Clone());
                var view = drop.GetComponent<ZNetView>();
                if (view != null && view.IsValid() && ZNetScene.instance != null)
                {
                    view.ClaimOwnership();
                    ZNetScene.instance.Destroy(drop.gameObject);
                }
                collected++;
            }
        }

        private static bool LooseFeedAround(Vector3 center, float radius)
        {
            foreach (var drop in FindObjectsOfType<ItemDrop>())
            {
                if (drop.m_itemData != null
                    && drop.m_itemData.m_shared.m_food > 0f
                    && drop.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable
                    && Vector3.Distance(drop.transform.position, center) <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Total of the item across all settlement chests.</summary>
        internal static int CountItemAround(Vector3 center, string prefabName)
        {
            var sharedName = SharedName(prefabName);
            if (sharedName == null)
            {
                return 0;
            }
            var radius = ModConfig.SettlementRadius.Value;
            var count = 0;
            foreach (var container in FindObjectsOfType<Container>())
            {
                if (Vector3.Distance(container.transform.position, center) > radius)
                {
                    continue;
                }
                var inventory = container.GetInventory();
                if (inventory != null)
                {
                    count += inventory.CountItems(sharedName);
                }
            }
            return count;
        }

        /// <summary>Removes up to the amount from settlement chests; returns what was taken.</summary>
        internal static int TakeItemsAround(Vector3 center, string prefabName, int amount)
        {
            var sharedName = SharedName(prefabName);
            if (sharedName == null || amount <= 0)
            {
                return 0;
            }
            var radius = ModConfig.SettlementRadius.Value;
            var taken = 0;
            foreach (var container in FindObjectsOfType<Container>())
            {
                if (taken >= amount
                    || Vector3.Distance(container.transform.position, center) > radius)
                {
                    continue;
                }
                var inventory = container.GetInventory();
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
        // Static "around a point" variants exist so the talk panel can run
        // exactly the same checks the work loop gates on.

        private bool HasStation(string nameToken)
        {
            return HasStationAround(_settler.Home, nameToken);
        }

        private bool HasBeehive()
        {
            return HasNearby<Beehive>();
        }

        private bool HasNearby<T>() where T : Component
        {
            return HasAround<T>(_settler.Home);
        }

        internal static bool HasStationAround(Vector3 center, string nameToken)
        {
            var radius = ModConfig.SettlementRadius.Value;
            foreach (var station in FindObjectsOfType<CraftingStation>())
            {
                if (station.m_name == nameToken
                    && Vector3.Distance(station.transform.position, center) <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool HasAround<T>(Vector3 center) where T : Component
        {
            var radius = ModConfig.SettlementRadius.Value;
            foreach (var component in FindObjectsOfType<T>())
            {
                if (Vector3.Distance(component.transform.position, center) <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Food items sitting in settlement chests, without eating any.</summary>
        internal static int CountFoodAround(Vector3 center)
        {
            var radius = ModConfig.SettlementRadius.Value;
            var count = 0;
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
                        && item.m_shared.m_food > 0f)
                    {
                        count += item.m_stack;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Whether any conversion of <paramref name="recipes"/> could run right
        /// now: ingredients and room for the output found in one chest - the
        /// same single-chest rule <see cref="Convert"/> applies.
        /// </summary>
        internal static bool CanConvertAround(Vector3 center, (string From, int Count, string To)[] recipes)
        {
            foreach (var (from, needed, to) in recipes)
            {
                var fromName = SharedName(from);
                var product = MakeItem(to, 1);
                if (fromName == null || product == null)
                {
                    continue;
                }
                if (FindStorageAround(center, inventory =>
                        inventory.CountItems(fromName) >= needed && inventory.CanAddItem(product)) != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Whether a chest around the point has room for one more of the item.</summary>
        internal static bool HasStorageForAround(Vector3 center, string prefabName)
        {
            var item = MakeItem(prefabName, 1);
            return item != null && FindStorageAround(center, inventory => inventory.CanAddItem(item)) != null;
        }

        // ---- Production ----

        /// <summary>
        /// The auto-gathering half of construction: when an active project
        /// still needs this resource and the supply chests don't yet hold
        /// enough to finish it, the settler's haul goes there instead of the
        /// regular stockpile.
        /// </summary>
        private bool TrySupplyConstruction(string prefabName, int amount)
        {
            var site = Settlements.ConstructionSite.FindNear(_settler.Home);
            if (site == null || !site.StillNeeds(prefabName))
            {
                return false;
            }
            if (Settlements.BuildChest.CountAround(site.transform.position, prefabName)
                >= site.RemainingOf(prefabName))
            {
                return false; // the chests already hold enough to finish
            }
            var item = MakeItem(prefabName, amount);
            if (item == null)
            {
                return false;
            }
            return Settlements.BuildChest.DepositAround(site.transform.position, item);
        }

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

        internal static int CountDamagedAround(Vector3 center)
        {
            var radius = ModConfig.SettlementRadius.Value;
            var damaged = 0;
            foreach (var wearNTear in FindObjectsOfType<WearNTear>())
            {
                if (Vector3.Distance(wearNTear.transform.position, center) <= radius
                    && wearNTear.GetHealthPercentage() < 1f)
                {
                    damaged++;
                }
            }
            return damaged;
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
            return FindStorageAround(_settler.Home, predicate);
        }

        internal static Container FindStorageAround(Vector3 center, System.Func<Inventory, bool> predicate)
        {
            var radius = ModConfig.SettlementRadius.Value;
            Container best = null;
            var bestDistance = float.MaxValue;
            foreach (var container in FindObjectsOfType<Container>())
            {
                var distance = Vector3.Distance(container.transform.position, center);
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

        internal static ItemDrop.ItemData MakeItem(string prefabName, int stack)
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

        internal static string SharedName(string prefabName)
        {
            var prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(prefabName) : null;
            var drop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
            return drop != null ? drop.m_itemData.m_shared.m_name : null;
        }
    }
}
