using VikingSettlements.Npcs;

namespace VikingSettlements.World
{
    /// <summary>
    /// The concrete settlement blueprints, assembled from vanilla building
    /// pieces. Positions use Valheim's 2m build grid; walls sit on y=0 with
    /// their bottom edge, floors are centered on their tile.
    /// </summary>
    internal static class Layouts
    {
        /// <summary>A 4x4m flat-roofed cabin. Door faces +Z.</summary>
        private static SettlementLayout Cabin(string chest = "piece_chest_wood")
        {
            var l = new SettlementLayout("cabin");

            // Floor and flat sod roof, four 2x2m tiles each.
            foreach (var x in new[] { -1f, 1f })
            {
                foreach (var z in new[] { -1f, 1f })
                {
                    l.Add("wood_floor", x, 0f, z);
                    l.Add("wood_floor", x, 2f, z);
                }
            }

            // Back wall.
            l.Add("wood_wall_full", -1f, 0f, -2f);
            l.Add("wood_wall_full", 1f, 0f, -2f);
            // Side walls.
            l.Add("wood_wall_full", -2f, 0f, -1f, 90f);
            l.Add("wood_wall_full", -2f, 0f, 1f, 90f);
            l.Add("wood_wall_full", 2f, 0f, -1f, 90f);
            l.Add("wood_wall_full", 2f, 0f, 1f, 90f);
            // Front wall with door opening.
            l.Add("wood_wall_full", -1f, 0f, 2f);
            l.Add("wood_door", 1f, 0f, 2f);

            l.Add("bed", -1f, 0f, -1f);
            if (chest != null)
            {
                l.Add(chest, 1.2f, 0f, -1.5f);
            }
            return l;
        }

        /// <summary>
        /// A 6x10m hall with a 45 degree roof. Long axis along Z, door at +Z.
        /// Wall material is parameterized so the plains variant can use stone.
        /// </summary>
        private static SettlementLayout Hall(string wall, bool oneMeterCourses, string chest)
        {
            var l = new SettlementLayout("hall");
            var xTiles = new[] { -2f, 0f, 2f };
            var zTiles = new[] { -4f, -2f, 0f, 2f, 4f };

            foreach (var x in xTiles)
            {
                foreach (var z in zTiles)
                {
                    l.Add("wood_floor", x, 0f, z);
                }
            }

            // Stone wall pieces are 2x1m, so stone walls need two courses to
            // reach the 2m eaves height of a wooden wall.
            var courses = oneMeterCourses ? new[] { 0f, 1f } : new[] { 0f };

            foreach (var y in courses)
            {
                // Long side walls.
                foreach (var z in zTiles)
                {
                    l.Add(wall, -3f, y, z, 90f);
                    l.Add(wall, 3f, y, z, 90f);
                }
                // Back gable wall.
                foreach (var x in xTiles)
                {
                    l.Add(wall, x, y, -5f);
                }
                // Front wall leaves a 2m door opening in the middle.
                l.Add(wall, -2f, y, 5f);
                l.Add(wall, 2f, y, 5f);
            }
            l.Add("wood_door", 0f, 0f, 5f);

            // 45 degree roof: eaves on the wall tops, ridge along the Z axis.
            foreach (var z in zTiles)
            {
                l.Add("wood_roof_45", -3f, 2f, z, 90f);
                l.Add("wood_roof_45", 3f, 2f, z, 270f);
                l.Add("wood_roof_top_45", 0f, 4f, z, 90f);
            }
            // Gable in-fills above the short walls.
            foreach (var zEnd in new[] { -5f, 5f })
            {
                var facing = zEnd < 0f ? 0f : 180f;
                l.Add("wood_wall_roof_45", -2f, 2f, zEnd, facing);
                l.Add("wood_wall_roof_45", 2f, 2f, zEnd, facing);
                l.Add("wood_wall_roof_top_45", 0f, 4f, zEnd, facing);
            }

            // Furnishings.
            l.Add("piece_table", 0f, 0f, -1f);
            l.Add("piece_chair", 1.2f, 0f, -1f, 270f);
            l.Add("piece_chair", -1.2f, 0f, -1f, 90f);
            l.Add("bed", -2f, 0f, -4f);
            l.Add("bed", 2f, 0f, -4f);
            if (chest != null)
            {
                l.Add(chest, 2f, 0f, -2f, 90f);
            }
            return l;
        }

        /// <summary>An open 2x2m watch platform on 6m poles with a torch.</summary>
        private static SettlementLayout Watchtower()
        {
            var l = new SettlementLayout("watchtower");
            foreach (var x in new[] { -1f, 1f })
            {
                foreach (var z in new[] { -1f, 1f })
                {
                    l.Add("wood_pole2", x, 0f, z);
                    l.Add("wood_pole2", x, 2f, z);
                    l.Add("wood_pole2", x, 4f, z);
                }
            }
            l.Add("wood_floor", 0f, 6f, 0f);
            l.Add("wood_fence", 0f, 6f, 1f);
            l.Add("wood_fence", 0f, 6f, -1f);
            l.Add("wood_fence", 1f, 6f, 0f, 90f);
            l.Add("wood_fence", -1f, 6f, 0f, 90f);
            l.Add("piece_groundtorch_wood", 0f, 6f, 0f);
            return l;
        }

        /// <summary>A fenced 6x4m field with crops. Gate at +Z.</summary>
        private static SettlementLayout Farm(string cropA, string cropB)
        {
            var l = new SettlementLayout("farm");
            foreach (var x in new[] { -2f, 0f, 2f })
            {
                l.Add("wood_fence", x, 0f, -2f);
            }
            l.Add("wood_fence", -2f, 0f, 2f);
            l.Add("wood_fence", 2f, 0f, 2f);
            l.Add("wood_gate", 0f, 0f, 2f);
            foreach (var z in new[] { -1f, 1f })
            {
                l.Add("wood_fence", -3f, 0f, z, 90f);
                l.Add("wood_fence", 3f, 0f, z, 90f);
            }
            foreach (var x in new[] { -2f, 0f, 2f })
            {
                l.Add(cropA, x, 0f, -1f);
                l.Add(cropB, x, 0f, 1f);
            }
            l.Add("piece_beehive", 3.8f, 0f, 2.8f);
            return l;
        }

        /// <summary>A 4x4m open market stall with the village trader.</summary>
        private static SettlementLayout TraderStall()
        {
            var l = new SettlementLayout("stall");
            foreach (var x in new[] { -2f, 2f })
            {
                foreach (var z in new[] { -2f, 2f })
                {
                    l.Add("wood_pole2", x, 0f, z);
                }
            }
            foreach (var x in new[] { -1f, 1f })
            {
                foreach (var z in new[] { -1f, 1f })
                {
                    l.Add("wood_floor", x, 2f, z);
                }
            }
            l.Add(SettlerPrefabs.Trader, 0f, 0f, 0f, 180f);
            l.Add("piece_chest_wood", 1.2f, 0f, -1.2f);
            l.Add("piece_banner01", -2f, 0f, 2f);
            return l;
        }

        /// <summary>The central fire pit with seating.</summary>
        private static SettlementLayout FirePlaza()
        {
            var l = new SettlementLayout("plaza");
            l.Add("fire_pit", 0f, 0f, 0f);
            l.Add("piece_chair", 1.5f, 0f, 0f, 270f);
            l.Add("piece_chair", -1.5f, 0f, 0f, 90f);
            l.Add("piece_chair", 0f, 0f, 1.5f, 180f);
            l.Add("wood_stack", 2.5f, 0f, 1.8f);
            return l;
        }

        /// <summary>Ring of defensive stakes with entrance gaps.</summary>
        private static void AddStakeRing(SettlementLayout l, float radius, int segments, float gapDegrees)
        {
            for (var i = 0; i < segments; i++)
            {
                var angle = 360f * i / segments;
                // Leave an opening around +Z for the entrance.
                if (angle < gapDegrees / 2f || angle > 360f - gapDegrees / 2f)
                {
                    continue;
                }
                var rad = angle * UnityEngine.Mathf.Deg2Rad;
                var x = radius * UnityEngine.Mathf.Sin(rad);
                var z = radius * UnityEngine.Mathf.Cos(rad);
                l.Add("sharp_stakes", x, 0f, z, angle + 90f);
            }
        }

        /// <summary>
        /// The large meadows village: fire plaza, three cabins, a longhouse,
        /// a farm, a trader stall, a watchtower and eight villagers.
        /// </summary>
        public static SettlementLayout MeadowsVillage()
        {
            var v = new SettlementLayout(SettlementLocations.MeadowsVillageLocation);
            // One flatten op per built-up area; they all level to the same
            // height since they share the location's origin altitude.
            v.Add(SettlerPrefabs.Flatten, 0f, 0f, 0f);
            v.Add(SettlerPrefabs.Flatten, 0f, 0f, 13f);
            v.Add(SettlerPrefabs.Flatten, -11f, 0f, 10f);
            v.Add(SettlerPrefabs.Flatten, 10f, 0f, 9f);
            v.Add(SettlerPrefabs.Flatten, 10f, 0f, -9f);
            v.Add(SettlerPrefabs.Flatten, 0f, 0f, -10f);

            v.Place(FirePlaza(), 0f, 0f);
            v.Add("piece_maypole", 4f, 0f, -3f);

            // Cabins around the plaza, doors facing the center.
            v.Place(Cabin(), 0f, -10f, 0f);
            v.Place(Cabin(), -10f, 0f, 90f);
            v.Place(Cabin(null), 10f, 0f, 270f);

            // Longhouse north of the plaza, door facing back to the center.
            v.Place(Hall("wood_wall_full", false, "TreasureChest_meadows"), 0f, 13f, 180f);

            v.Place(Farm("Pickable_Carrot", "Pickable_Turnip"), -11f, 10f, 0f);
            v.Place(TraderStall(), 10f, 9f, 225f);
            v.Place(Watchtower(), 10f, -9f);

            foreach (var pos in new[]
            {
                (x: 2.5f, z: 2.5f), (x: -2.5f, z: 3f), (x: -3f, z: -2.5f), (x: 3f, z: -3.5f),
            })
            {
                v.Add("piece_groundtorch_wood", pos.x, 0f, pos.z);
            }

            // The villagers.
            v.Add(SettlerPrefabs.Settler, 1.2f, 0f, -1.2f, 45f);
            v.Add(SettlerPrefabs.Settler, -2f, 0f, 1f, 135f);
            v.Add(SettlerPrefabs.Settler, 0f, 0f, -7f);
            v.Add(SettlerPrefabs.Settler, -7f, 0f, 1f, 90f);
            v.Add(SettlerPrefabs.Settler, 7f, 0f, -7f, 315f);
            v.Add(SettlerPrefabs.Settler, -9f, 0f, 8f);
            v.Add(SettlerPrefabs.Seer, 0f, 0f, 7f, 180f);
            return v;
        }

        /// <summary>A small fortified black forest outpost with three settlers.</summary>
        public static SettlementLayout ForestOutpost()
        {
            var o = new SettlementLayout(SettlementLocations.ForestOutpostLocation);
            o.Add(SettlerPrefabs.Flatten, 0f, 0f, 0f);

            o.Place(Watchtower(), 0f, 0f);
            o.Place(Cabin("TreasureChest_blackforest"), -6f, 2f, 90f);
            o.Add("fire_pit", 3f, 0f, 2f);
            o.Add("wood_stack", 4.5f, 0f, 0.5f);
            o.Add("piece_workbench", 3.5f, 0f, -2f, 180f);
            o.Add("piece_groundtorch_wood", -1.8f, 0f, 1.8f);
            o.Add("piece_groundtorch_wood", 1.8f, 0f, -1.8f);
            AddStakeRing(o, 10f, 14, 40f);

            o.Add(SettlerPrefabs.Settler, 2f, 0f, 2.5f, 225f);
            o.Add(SettlerPrefabs.Settler, -2.5f, 0f, -1f, 90f);
            o.Add(SettlerPrefabs.Settler, 0f, 0f, 5f, 180f);
            return o;
        }

        /// <summary>
        /// A clanless bandit camp: crude shelters, loot, raiders, and the war
        /// totem whose destruction clears the camp.
        /// </summary>
        public static SettlementLayout ClanlessCamp()
        {
            var c = new SettlementLayout(SettlementLocations.ClanlessCampLocation);
            c.Add(SettlerPrefabs.Flatten, 0f, 0f, 0f);

            c.Add("fire_pit", 0f, 0f, 0f);
            c.Add(SettlerPrefabs.CampTotem, 2.5f, 0f, 0.5f);
            c.Add("wood_stack", 3f, 0f, -2f);

            // Two crude open shelters: poles carrying a flat roof.
            foreach (var (sx, sz, rot) in new[] { (-4.5f, 3.5f, 30f), (4f, 4.5f, 300f) })
            {
                var shelter = new SettlementLayout("shelter");
                shelter.Add("wood_pole2", -1.8f, 0f, -0.8f);
                shelter.Add("wood_pole2", 1.8f, 0f, -0.8f);
                shelter.Add("wood_pole2", -1.8f, 0f, 0.8f);
                shelter.Add("wood_pole2", 1.8f, 0f, 0.8f);
                shelter.Add("wood_floor", -1f, 2f, 0f);
                shelter.Add("wood_floor", 1f, 2f, 0f);
                c.Place(shelter, sx, sz, rot);
            }

            c.Add("TreasureChest_blackforest", -4.5f, 0f, 2.5f, 30f);
            AddStakeRing(c, 9f, 10, 70f);

            c.Add(SettlerPrefabs.Raider, 1.5f, 0f, 1.8f, 200f);
            c.Add(SettlerPrefabs.Raider, -2f, 0f, -1.2f, 80f);
            c.Add(SettlerPrefabs.Raider, -1f, 0f, 4f, 160f);
            c.Add(SettlerPrefabs.Raider, 3.2f, 0f, -3.2f, 340f);
            return c;
        }

        /// <summary>A stone-walled plains steading with a barley farm.</summary>
        public static SettlementLayout PlainsSteading()
        {
            var s = new SettlementLayout(SettlementLocations.PlainsSteadingLocation);
            s.Add(SettlerPrefabs.Flatten, 0f, 0f, 0f);
            s.Add(SettlerPrefabs.Flatten, -10f, 0f, -3f);
            s.Add(SettlerPrefabs.Flatten, 11f, 0f, 2f);

            s.Place(Hall("stone_wall_2x1", true, "TreasureChest_heath"), 0f, 4f, 180f);
            s.Place(Farm("Pickable_Barley", "Pickable_Flax"), -10f, -3f, 0f);
            s.Place(FirePlaza(), 6f, -5f);
            s.Place(Watchtower(), 11f, 2f);
            s.Add("piece_groundtorch_wood", 2.5f, 0f, -3f);
            s.Add("piece_groundtorch_wood", -2.5f, 0f, -3f);
            AddStakeRing(s, 15f, 20, 36f);

            s.Add(SettlerPrefabs.Settler, 5f, 0f, -4f, 270f);
            s.Add(SettlerPrefabs.Settler, -8f, 0f, -4f, 90f);
            s.Add(SettlerPrefabs.Settler, 0f, 0f, -2f, 180f);
            s.Add(SettlerPrefabs.Seer, 1.5f, 0f, -6f);
            return s;
        }
    }
}
