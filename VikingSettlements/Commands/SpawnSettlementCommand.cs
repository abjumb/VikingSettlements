using System.Collections.Generic;
using Jotunn.Entities;
using UnityEngine;
using VikingSettlements.World;

namespace VikingSettlements.Commands
{
    /// <summary>
    /// Debug/utility command to place a settlement in an already explored
    /// world: builds the chosen layout a few meters in front of the player.
    /// </summary>
    internal class SpawnSettlementCommand : ConsoleCommand
    {
        public override string Name => "vs_spawn";

        public override string Help => "Spawns a viking settlement in front of the player. Usage: vs_spawn [village|outpost|steading|camp]";

        public override bool IsCheat => true;

        public override void Run(string[] args)
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                Console.instance.Print("vs_spawn: no local player, use this command in-game");
                return;
            }

            var variant = args.Length > 0 ? args[0].ToLowerInvariant() : "village";
            SettlementLayout layout;
            switch (variant)
            {
                case "village":
                    layout = Layouts.MeadowsVillage();
                    break;
                case "outpost":
                    layout = Layouts.ForestOutpost();
                    break;
                case "steading":
                    layout = Layouts.PlainsSteading();
                    break;
                case "camp":
                    layout = Layouts.ClanlessCamp();
                    break;
                default:
                    Console.instance.Print($"vs_spawn: unknown settlement '{variant}', options: village, outpost, steading, camp");
                    return;
            }

            var origin = player.transform.position + player.transform.forward * 15f;
            var rotation = Quaternion.Euler(0f, player.transform.eulerAngles.y + 180f, 0f);
            var placed = LayoutBuilder.BuildAt(origin, rotation, layout);
            Console.instance.Print($"vs_spawn: placed {variant} ({placed} objects) in front of you");
        }

        public override List<string> CommandOptionList()
        {
            return new List<string> { "village", "outpost", "steading", "camp" };
        }
    }
}
