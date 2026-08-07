using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Gives each settler instance a persistent personal name, derived
    /// deterministically from its network id so every client sees the same
    /// name without any extra synchronization.
    /// </summary>
    public class SettlerIdentity : MonoBehaviour
    {
        private static readonly string[] Names =
        {
            "Astrid", "Bjorn", "Dagny", "Eirik", "Freydis", "Gunnar",
            "Helga", "Ingolf", "Jorunn", "Kettil", "Leif", "Magnhild",
            "Njal", "Oddny", "Ragnar", "Signy", "Torstein", "Ulfhild",
            "Vigdis", "Yrsa", "Arnbjorg", "Halvar", "Solveig", "Sten",
        };

        private void Start()
        {
            var character = GetComponent<Character>();
            var view = GetComponent<ZNetView>();
            if (character == null || view == null || !view.IsValid())
            {
                return;
            }

            var seed = view.GetZDO().m_uid.GetHashCode();
            var index = (int)((uint)seed % (uint)Names.Length);
            character.m_name = Names[index];
        }
    }
}
