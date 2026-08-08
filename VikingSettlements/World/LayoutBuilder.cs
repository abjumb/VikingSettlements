using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace VikingSettlements.World
{
    /// <summary>
    /// Instantiates the prefabs of a <see cref="SettlementLayout"/>, either
    /// into an (inactive) location container for world generation, or
    /// directly into the live world for the console command. Missing prefabs
    /// are skipped with a warning so a game update can never break loading.
    ///
    /// Building pieces are spawned as hardened "VS_loc_" clones with the
    /// structural-integrity and rain wear disabled: location pieces appear
    /// on raw terrain (racing the flatten op), where vanilla support math
    /// immediately collapses towers, roofs and anything whose wall bottom
    /// floats over a slope. Hardened pieces stay damageable and repairable,
    /// they just never tear themselves down.
    /// </summary>
    internal static class LayoutBuilder
    {
        private static readonly Dictionary<string, GameObject> _resolved = new Dictionary<string, GameObject>();

        public static int BuildInto(Transform parent, SettlementLayout layout)
        {
            var count = 0;
            var missing = new HashSet<string>();
            foreach (var part in layout.Parts)
            {
                var prefab = Resolve(part.Prefab);
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
                var prefab = Resolve(part.Prefab);
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

        // The clone (not a field tweak on the spawned instance) is what makes
        // the fix stick: respawns after a reload instantiate whatever prefab
        // name the ZDO recorded, so the hardened variant must be a registered
        // prefab of its own.
        private static GameObject Resolve(string name)
        {
            if (_resolved.TryGetValue(name, out var cached) && cached != null)
            {
                return cached;
            }

            var prefab = PrefabManager.Instance.GetPrefab(name);
            if (prefab == null)
            {
                return null;
            }
            // Our own prefabs and anything without structural wear spawn as-is.
            if (name.StartsWith("VS_") || prefab.GetComponent<WearNTear>() == null)
            {
                _resolved[name] = prefab;
                return prefab;
            }

            var hardenedName = "VS_loc_" + name;
            var clone = PrefabManager.Instance.GetPrefab(hardenedName)
                ?? PrefabManager.Instance.CreateClonedPrefab(hardenedName, name);
            if (clone == null)
            {
                _resolved[name] = prefab;
                return prefab;
            }
            var wear = clone.GetComponent<WearNTear>();
            wear.m_noSupportWear = true;
            wear.m_noRoofWear = true;
            PrefabManager.Instance.AddPrefab(new CustomPrefab(clone, false));
            _resolved[name] = clone;
            return clone;
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
