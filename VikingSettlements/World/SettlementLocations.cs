using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;

namespace VikingSettlements.World
{
    /// <summary>
    /// Registers the settlements as world generation locations. Locations are
    /// only placed in unexplored zones, so existing explored areas keep their
    /// terrain; use the vs_spawn console command to add a settlement there.
    /// </summary>
    internal static class SettlementLocations
    {
        public const string MeadowsVillageLocation = "VS_MeadowsVillage";
        public const string ForestOutpostLocation = "VS_ForestOutpost";
        public const string PlainsSteadingLocation = "VS_PlainsSteading";

        private static bool _registered;

        public static void RegisterAll()
        {
            if (_registered)
            {
                return;
            }
            _registered = true;

            Register(Layouts.MeadowsVillage(), new LocationConfig
            {
                Biome = Heightmap.Biome.Meadows,
                Quantity = ModConfig.MeadowsVillages.Value,
                Priotized = true,
                ExteriorRadius = 26f,
                ClearArea = true,
                RandomRotation = true,
                MinAltitude = 3f,
                MinDistance = 700f,
                MinDistanceFromSimilar = 900f,
                MaxTerrainDelta = 3f,
                Group = "vs_settlements",
            });

            Register(Layouts.ForestOutpost(), new LocationConfig
            {
                Biome = Heightmap.Biome.BlackForest,
                Quantity = ModConfig.ForestOutposts.Value,
                Priotized = false,
                ExteriorRadius = 14f,
                ClearArea = true,
                RandomRotation = true,
                MinAltitude = 2f,
                MinDistance = 600f,
                MinDistanceFromSimilar = 600f,
                MaxTerrainDelta = 3f,
                Group = "vs_settlements",
            });

            Register(Layouts.PlainsSteading(), new LocationConfig
            {
                Biome = Heightmap.Biome.Plains,
                Quantity = ModConfig.PlainsSteadings.Value,
                Priotized = false,
                ExteriorRadius = 20f,
                ClearArea = true,
                RandomRotation = true,
                MinAltitude = 3f,
                MinDistance = 1500f,
                MinDistanceFromSimilar = 800f,
                MaxTerrainDelta = 3f,
                Group = "vs_settlements",
            });
        }

        private static void Register(SettlementLayout layout, LocationConfig config)
        {
            if (config.Quantity <= 0)
            {
                Jotunn.Logger.LogInfo($"Location {layout.Name} disabled via config");
                return;
            }

            var container = ZoneManager.Instance.CreateLocationContainer(layout.Name);
            var placed = LayoutBuilder.BuildInto(container.transform, layout);
            if (placed == 0)
            {
                Jotunn.Logger.LogWarning($"Location {layout.Name} has no valid parts, not registered");
                return;
            }

            ZoneManager.Instance.AddCustomLocation(new CustomLocation(container, false, config));
            Jotunn.Logger.LogInfo($"Registered location {layout.Name} ({placed} parts, quantity {config.Quantity})");
        }
    }
}
