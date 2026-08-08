using VikingSettlements.World;

namespace VikingSettlements.Settlements
{
    /// <summary>
    /// A structure the player can order their builders to raise: one of the
    /// simple wooden buildings that wild meadows villages are made of, with
    /// a flat material cost drawn from the settlement's supply chests.
    /// </summary>
    internal class Blueprint
    {
        public string Id;
        public string NameToken;
        public int WoodCost;
        public int StoneCost;
        public int MinTier = 1;
        public System.Func<SettlementLayout> Layout;
    }

    internal static class Blueprints
    {
        public static readonly Blueprint[] All =
        {
            new Blueprint
            {
                Id = "cabin",
                NameToken = "$vs_bp_cabin",
                WoodCost = 40,
                StoneCost = 0,
                Layout = Layouts.BlueprintCabin,
            },
            new Blueprint
            {
                Id = "watchtower",
                NameToken = "$vs_bp_watchtower",
                WoodCost = 30,
                StoneCost = 0,
                Layout = Layouts.BlueprintWatchtower,
            },
            new Blueprint
            {
                Id = "longhouse",
                NameToken = "$vs_bp_longhouse",
                WoodCost = 100,
                StoneCost = 10,
                MinTier = 2,
                Layout = Layouts.BlueprintLonghouse,
            },
            new Blueprint
            {
                Id = "greathall",
                NameToken = "$vs_bp_greathall",
                WoodCost = 60,
                StoneCost = 40,
                MinTier = 3,
                Layout = Layouts.BlueprintGreatHall,
            },
        };

        public static Blueprint Find(string id)
        {
            foreach (var blueprint in All)
            {
                if (blueprint.Id == id)
                {
                    return blueprint;
                }
            }
            return null;
        }
    }
}
