using System.Collections.Generic;
using Jotunn.Entities;
using UnityEngine;
using VikingSettlements.World;

namespace VikingSettlements.Commands
{
    /// <summary>
    /// Marks the closest settlement (or clanless camp) on the player's map,
    /// the same way Hugin points at boss locations. Not a cheat: it only
    /// reveals a direction to walk, which the mod otherwise leaves to blind
    /// exploration.
    /// </summary>
    internal class FindSettlementCommand : ConsoleCommand
    {
        public override string Name => "vs_find";

        public override string Help => "Marks the closest settlement on your map. Usage: vs_find [village|outpost|steading|camp]";

        public override void Run(string[] args)
        {
            var player = Player.m_localPlayer;
            if (player == null || ZoneSystem.instance == null)
            {
                Console.instance.Print("vs_find: no local player, use this command in-game");
                return;
            }

            var variant = args.Length > 0 ? args[0].ToLowerInvariant() : "village";
            string locationName;
            string label;
            switch (variant)
            {
                case "village":
                    locationName = SettlementLocations.MeadowsVillageLocation;
                    label = "Village";
                    break;
                case "outpost":
                    locationName = SettlementLocations.ForestOutpostLocation;
                    label = "Outpost";
                    break;
                case "steading":
                    locationName = SettlementLocations.PlainsSteadingLocation;
                    label = "Steading";
                    break;
                case "camp":
                    locationName = SettlementLocations.ClanlessCampLocation;
                    label = "Clanless camp";
                    break;
                default:
                    Console.instance.Print($"vs_find: unknown type '{variant}', options: village, outpost, steading, camp");
                    return;
            }

            if (!ZoneSystem.instance.FindClosestLocation(locationName, player.transform.position, out var closest))
            {
                Console.instance.Print($"vs_find: no {variant} exists in this world's generated terrain yet");
                return;
            }

            if (Minimap.instance != null)
            {
                Minimap.instance.AddPin(closest.m_position, Minimap.PinType.Icon1, label, true, false);
            }

            var delta = closest.m_position - player.transform.position;
            var distance = new Vector3(delta.x, 0f, delta.z).magnitude;
            Console.instance.Print($"vs_find: {label.ToLowerInvariant()} marked on your map, {distance:0} m to the {Compass(delta)}");
        }

        public override List<string> CommandOptionList()
        {
            return new List<string> { "village", "outpost", "steading", "camp" };
        }

        private static string Compass(Vector3 delta)
        {
            var directions = new[] { "north", "north-east", "east", "south-east", "south", "south-west", "west", "north-west" };
            var angle = Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg;
            var index = Mathf.RoundToInt(angle / 45f);
            return directions[((index % 8) + 8) % 8];
        }
    }
}
