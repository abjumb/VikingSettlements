using System.Collections.Generic;
using UnityEngine;

namespace VikingSettlements.World
{
    /// <summary>
    /// A data-driven settlement blueprint: a flat list of prefab placements
    /// relative to the settlement center. Layouts compose - smaller structure
    /// layouts are stamped into a settlement layout via <see cref="Place"/>.
    /// </summary>
    internal class SettlementLayout
    {
        internal struct Part
        {
            public string Prefab;
            public Vector3 Position;
            public float RotationY;
        }

        public string Name { get; }
        public readonly List<Part> Parts = new List<Part>();

        public SettlementLayout(string name)
        {
            Name = name;
        }

        public void Add(string prefab, float x, float y, float z, float rotY = 0f)
        {
            Parts.Add(new Part
            {
                Prefab = prefab,
                Position = new Vector3(x, y, z),
                RotationY = rotY,
            });
        }

        /// <summary>Stamps another layout into this one at an offset and rotation.</summary>
        public void Place(SettlementLayout structure, float x, float z, float rotY = 0f)
        {
            var rotation = Quaternion.Euler(0f, rotY, 0f);
            var offset = new Vector3(x, 0f, z);
            foreach (var part in structure.Parts)
            {
                Parts.Add(new Part
                {
                    Prefab = part.Prefab,
                    Position = rotation * part.Position + offset,
                    RotationY = part.RotationY + rotY,
                });
            }
        }
    }
}
