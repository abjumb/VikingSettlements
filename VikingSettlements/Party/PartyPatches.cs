using HarmonyLib;
using VikingSettlements.Npcs;

namespace VikingSettlements.Party
{
    /// <summary>
    /// The permadeath contract, enforced at the single point where all damage
    /// is applied (Character.RPC_Damage runs on whichever machine owns the
    /// character, and NetworkCompatibility guarantees that machine has this
    /// patch):
    ///
    ///  - players can never damage a recruited villager, so a stray swing
    ///    cannot kill (or even aggro) your own people;
    ///  - party members take no environmental damage (falls, drowning,
    ///    smoke, fire spread) - traversal and jank cannot kill them;
    ///  - party members are untouchable while their owner is away - fate
    ///    only finds them in a fight you are standing in;
    ///  - a falling-back member takes a fraction of incoming damage, so the
    ///    rescue command genuinely rescues when you disengage with them.
    ///
    /// Everything left is a monster hitting a member at your side: the one
    /// death the design wants to keep possible.
    /// </summary>
    internal static class PartyDamageContract
    {
        private const float FallbackDamageMultiplier = 0.25f;

        internal static bool AllowDamage(Character target, HitData hit)
        {
            if (target == null || hit == null)
            {
                return true;
            }
            var settler = target.GetComponent<SettlerRecruitable>();
            if (settler == null || settler.State == SettlerState.Wild)
            {
                return true;
            }

            var attacker = hit.GetAttacker();
            if (attacker is Player)
            {
                return false;
            }

            var member = target.GetComponent<PartyMember>();
            if (member != null && member.IsActiveMember)
            {
                if (attacker == null)
                {
                    return false;
                }
                if (!member.OwnerNearby)
                {
                    return false;
                }
                if (member.Stance == PartyStance.Fallback)
                {
                    hit.ApplyModifier(FallbackDamageMultiplier);
                }
            }

            // Player-given armor counts: NPCs never apply equipment armor on
            // their own, so worn pieces reduce the hit here on the owner.
            var equipment = target.GetComponent<SettlerEquipment>();
            if (equipment != null && equipment.EquippedArmor > 0f)
            {
                hit.ApplyArmor(equipment.EquippedArmor);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.RPC_Damage))]
    internal static class Character_RPC_Damage_Patch
    {
        private static bool Prefix(Character __instance, HitData hit)
        {
            return PartyDamageContract.AllowDamage(__instance, hit);
        }
    }

    // Pocket the party into the character save before the profile is written,
    // so logging out anywhere - mid-ocean, mid-dungeon - can never strand
    // them. Shutdown covers quit-to-desktop; both are no-ops on servers.
    [HarmonyPatch(typeof(Game), nameof(Game.Logout))]
    internal static class Game_Logout_Patch
    {
        private static void Prefix()
        {
            PartySystem.StowForExit();
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Shutdown))]
    internal static class Game_Shutdown_Patch
    {
        private static void Prefix()
        {
            PartySystem.StowForExit();
        }
    }
}
