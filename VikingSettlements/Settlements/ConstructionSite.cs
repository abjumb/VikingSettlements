using System.Collections.Generic;
using UnityEngine;
using VikingSettlements.Npcs;
using VikingSettlements.World;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// A marked-out building project: placed where the player stood when they
    /// gave a builder the order, visualized as a material pile. Builders feed
    /// it from the settlement's supply chests on their work ticks; when the
    /// full cost has been carried over, the blueprint's structure is raised
    /// on the spot. Progress lives in the ZDO so it persists and syncs.
    /// </summary>
    public class ConstructionSite : MonoBehaviour, Hoverable, Interactable
    {
        public const string BlueprintKey = "vs_bp";
        public const string WoodKey = "vs_bpwood";
        public const string StoneKey = "vs_bpstone";

        private const int WoodPerTick = 8;
        private const int StonePerTick = 2;
        private const float SupplyWarnSeconds = 120f;

        public static readonly List<ConstructionSite> Instances = new List<ConstructionSite>();

        private ZNetView _nview;
        private float _supplyTimer;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
        }

        private void OnEnable()
        {
            Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        internal Blueprint Blueprint => _nview != null && _nview.IsValid()
            ? Blueprints.Find(_nview.GetZDO().GetString(BlueprintKey))
            : null;

        internal int Wood
        {
            get => _nview.GetZDO().GetInt(WoodKey);
            set => _nview.GetZDO().Set(WoodKey, value);
        }

        internal int Stone
        {
            get => _nview.GetZDO().GetInt(StoneKey);
            set => _nview.GetZDO().Set(StoneKey, value);
        }

        internal static ConstructionSite FindNear(Vector3 center)
        {
            var radius = ModConfig.SettlementRadius.Value;
            foreach (var site in Instances)
            {
                if (site != null && Vector3.Distance(site.transform.position, center) <= radius)
                {
                    return site;
                }
            }
            return null;
        }

        internal int RemainingOf(string prefabName)
        {
            var blueprint = Blueprint;
            if (blueprint == null)
            {
                return 0;
            }
            switch (prefabName)
            {
                case "Wood": return Mathf.Max(0, blueprint.WoodCost - Wood);
                case "Stone": return Mathf.Max(0, blueprint.StoneCost - Stone);
                default: return 0;
            }
        }

        internal bool StillNeeds(string prefabName)
        {
            return RemainingOf(prefabName) > 0;
        }

        /// <summary>Whether every still-needed material is stocked in a supply chest.</summary>
        internal bool SuppliesAvailable()
        {
            if (StillNeeds("Wood") && BuildChest.CountAround(transform.position, "Wood") == 0)
            {
                return false;
            }
            if (StillNeeds("Stone") && BuildChest.CountAround(transform.position, "Stone") == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// One builder work tick: carry materials from the supply chests into
        /// the site, and raise the building when the cost is fully paid.
        /// Runs on the builder's owner machine.
        /// </summary>
        internal void BuildTick()
        {
            if (_nview == null || !_nview.IsValid())
            {
                return;
            }
            var blueprint = Blueprint;
            if (blueprint == null)
            {
                return;
            }
            _nview.ClaimOwnership();

            var neededWood = blueprint.WoodCost - Wood;
            if (neededWood > 0)
            {
                Wood += BuildChest.ConsumeAround(transform.position, "Wood",
                    Mathf.Min(WoodPerTick, neededWood));
            }
            var neededStone = blueprint.StoneCost - Stone;
            if (neededStone > 0)
            {
                Stone += BuildChest.ConsumeAround(transform.position, "Stone",
                    Mathf.Min(StonePerTick, neededStone));
            }

            if (Wood >= blueprint.WoodCost && Stone >= blueprint.StoneCost)
            {
                Complete(blueprint);
            }
        }

        private void Complete(Blueprint blueprint)
        {
            var position = transform.position;
            var rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            if (ZNetScene.instance != null)
            {
                ZNetScene.instance.Destroy(gameObject);
            }
            LayoutBuilder.BuildAt(position, rotation, blueprint.Layout());

            var player = Player.m_localPlayer;
            if (player != null && Vector3.Distance(player.transform.position, position) < 50f)
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize(
                        $"$vs_bp_complete: {blueprint.NameToken}"));
            }
        }

        // The recurring low-supply warning. Client-side per player, so it
        // reaches whoever is actually there regardless of ZDO ownership.
        private void Update()
        {
            var player = Player.m_localPlayer;
            if (player == null || _nview == null || !_nview.IsValid())
            {
                return;
            }
            if (Vector3.Distance(player.transform.position, transform.position)
                > ModConfig.SettlementRadius.Value + 30f)
            {
                _supplyTimer = 0f;
                return;
            }
            _supplyTimer += Time.deltaTime;
            if (_supplyTimer < SupplyWarnSeconds)
            {
                return;
            }
            _supplyTimer = 0f;
            if (Blueprint != null && !SuppliesAvailable())
            {
                player.Message(MessageHud.MessageType.Center,
                    Localization.instance.Localize("$vs_supply_low"));
            }
        }

        public string GetHoverName()
        {
            return Localization.instance.Localize("$vs_site");
        }

        public string GetHoverText()
        {
            var blueprint = Blueprint;
            if (blueprint == null)
            {
                return Localization.instance.Localize("$vs_site");
            }
            var progress = $"$item_wood {Wood}/{blueprint.WoodCost}";
            if (blueprint.StoneCost > 0)
            {
                progress += $"   $item_stone {Stone}/{blueprint.StoneCost}";
            }
            var status = "";
            if (!HasBuilder())
            {
                status = "\n<color=orange>$vs_bp_needsbuilder</color>";
            }
            else if (!SuppliesAvailable())
            {
                status = "\n<color=orange>$vs_supply_low</color>";
            }
            return Localization.instance.Localize(
                $"$vs_site: {blueprint.NameToken}\n{progress}{status}"
                + "\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] $vs_bp_cancel");
        }

        private bool HasBuilder()
        {
            var radius = ModConfig.SettlementRadius.Value;
            foreach (var settler in SettlerRecruitable.Instances)
            {
                if (settler.State == SettlerState.Assigned
                    && settler.Job == SettlerJob.Builder
                    && Vector3.Distance(settler.transform.position, transform.position) <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold || !alt || _nview == null || !_nview.IsValid())
            {
                return false;
            }
            var player = user as Player;
            _nview.ClaimOwnership();
            if (ZNetScene.instance != null)
            {
                ZNetScene.instance.Destroy(gameObject);
            }
            if (player != null)
            {
                player.Message(MessageHud.MessageType.TopLeft,
                    Localization.instance.Localize("$vs_bp_canceled"));
            }
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }
    }
}
