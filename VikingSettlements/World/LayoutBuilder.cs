using System.Collections.Generic;
using Jotunn.Managers;
using UnityEngine;

namespace VikingSettlements.World
{
    /// <summary>
    /// Instantiates the prefabs of a <see cref="SettlementLayout"/>, either
    /// into an (inactive) location container for world generation, or
    /// directly into the live world for the console command. Missing prefabs
    /// are skipped with a warning so a game update can never break loading.
    /// </summary>
    internal static class LayoutBuilder
    {
        public static int BuildInto(Transform parent, SettlementLayout layout)
        {
            var count = 0;
            var missing = new HashSet<string>();
            foreach (var part in layout.Parts)
            {
                var prefab = PrefabManager.Instance.GetPrefab(part.Prefab);
                if (prefab == null)
                {
                    missing.Add(part.Prefab);
                    continue;
                }
                var instance = Object.Instantiate(prefab, parent);
                instance.transform.localPosition = part.Position;
                instance.transform.localRotation = Quaternion.Euler(0f, part.RotationY, 0f);
                count++;
            }
            WarnMissing(layout, missing);
            return count;
        }

        public static int BuildAt(Vector3 origin, Quaternion rotation, SettlementLayout layout)
        {
            var count = 0;
            var missing = new HashSet<string>();
            foreach (var part in layout.Parts)
            {
                var prefab = PrefabManager.Instance.GetPrefab(part.Prefab);
                if (prefab == null)
                {
                    missing.Add(part.Prefab);
                    continue;
                }
                Object.Instantiate(prefab,
                    origin + rotation * part.Position,
                    rotation * Quaternion.Euler(0f, part.RotationY, 0f));
                count++;
            }
            WarnMissing(layout, missing);
            return count;
        }

        private static void WarnMissing(SettlementLayout layout, HashSet<string> missing)
        {
            foreach (var name in missing)
            {
                Jotunn.Logger.LogWarning($"[{layout.Name}] prefab '{name}' not found, skipped");
            }
        }
    }
}
