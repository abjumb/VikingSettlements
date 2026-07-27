using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Executes the periodic work of an assigned settler, owner-side only:
    /// - Lumberjack / Farmer: produce raw resources into settlement chests.
    /// - Blacksmith: smelts ore found in settlement chests into metal.
    /// - Builder: repairs damaged build pieces inside the settlement.
    /// - Guard: no production (combat bonuses applied by SettlerRecruitable).
    /// </summary>
    public class SettlerWork : MonoBehaviour
    {
        private static readonly (string From, string To)[] SmeltingRecipes =
        {
            ("CopperOre", "Copper"),
            ("TinOre", "Tin"),
            ("IronScrap", "Iron"),
            ("Wood", "Coal"),
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

            _timer += Time.deltaTime;
            if (_timer < ModConfig.WorkIntervalSeconds.Value)
            {
                return;
            }
            _timer = 0f;

            switch (_settler.Job)
            {
                case SettlerJob.Lumberjack:
                    Produce("Wood", Random.Range(2, 5));
                    break;
                case SettlerJob.Farmer:
                    Produce(Random.value < 0.5f ? "Carrot" : "Turnip", Random.Range(1, 3));
                    if (Random.value < 0.2f)
                    {
                        Produce("Honey", 1);
                    }
                    break;
                case SettlerJob.Blacksmith:
                    Smelt();
                    break;
                case SettlerJob.Builder:
                    Repair();
                    break;
            }
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

        private void Smelt()
        {
            foreach (var (from, to) in SmeltingRecipes)
            {
                var fromName = SharedName(from);
                var product = MakeItem(to, 1);
                if (fromName == null || product == null)
                {
                    continue;
                }
                var container = FindStorage(inventory =>
                    inventory.CountItems(fromName) > 0 && inventory.CanAddItem(product));
                if (container == null)
                {
                    continue;
                }
                container.GetInventory().RemoveItem(fromName, 1);
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
