using System.Collections.Generic;
using Jotunn.Entities;

namespace VikingSettlements.Commands
{
    /// <summary>
    /// Party roster and recovery. Not a cheat: it only reports on your own
    /// companions, and recall exists to undo systems-jank separations (a
    /// crash, a death far from camp) - the same guarantee the party's
    /// traversal handling gives you, on demand.
    /// </summary>
    internal class PartyCommand : ConsoleCommand
    {
        public override string Name => "vs_party";

        public override string Help => "Shows your party. Usage: vs_party [recall] - recall teleports separated members to you (host/singleplayer).";

        public override void Run(string[] args)
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                Console.instance.Print("vs_party: no local player, use this command in-game");
                return;
            }
            if (args.Length > 0 && args[0].ToLowerInvariant() == "recall")
            {
                Console.instance.Print(Party.PartySystem.RecallStragglers(player));
                return;
            }
            foreach (var line in Party.PartySystem.Describe(player))
            {
                Console.instance.Print(line);
            }
        }

        public override List<string> CommandOptionList()
        {
            return new List<string> { "recall" };
        }
    }
}
