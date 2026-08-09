using System.Collections.Generic;
using UnityEngine;

namespace VikingSettlements.Npcs
{
    /// <summary>
    /// Evaluates what a settler needs to do their job, using the same live
    /// checks the work loop gates on, so the talk panel never disagrees with
    /// what the settler will actually do on their next work tick.
    /// </summary>
    internal static class SettlerNeeds
    {
        internal struct Line
        {
            public string Token;
            public bool Met;
        }

        internal static List<Line> Evaluate(SettlerRecruitable settler)
        {
            var lines = new List<Line>();
            if (settler == null || settler.State != SettlerState.Assigned)
            {
                return lines;
            }
            var home = settler.Home;
            var gated = ModConfig.RequireWorkstations.Value;

            if (ModConfig.FoodUpkeep.Value)
            {
                lines.Add(new Line
                {
                    Token = "$vs_need_food",
                    Met = SettlerWork.CountFoodAround(home) > 0,
                });
            }
            if (ModConfig.HomesMatter.Value)
            {
                var housed = SettlerHousing.HasHome(settler);
                lines.Add(new Line
                {
                    Token = housed ? "$vs_talk_home" : "$vs_talk_homeless",
                    Met = housed,
                });
            }

            switch (settler.Job)
            {
                case SettlerJob.Lumberjack:
                    lines.Add(Storage(home, "Wood"));
                    break;
                case SettlerJob.Farmer:
                    lines.Add(Storage(home, "Carrot"));
                    if (gated)
                    {
                        lines.Add(new Line
                        {
                            Token = "$vs_need_beehive",
                            Met = SettlerWork.HasAround<Beehive>(home),
                        });
                    }
                    break;
                case SettlerJob.Builder:
                    if (gated)
                    {
                        lines.Add(new Line
                        {
                            Token = "$vs_need_workbench",
                            Met = SettlerWork.HasStationAround(home, "$piece_workbench"),
                        });
                    }
                    var site = Settlements.ConstructionSite.FindNear(home);
                    if (site != null)
                    {
                        lines.Add(new Line
                        {
                            Token = "$vs_need_supplies",
                            Met = site.SuppliesAvailable(),
                        });
                    }
                    else
                    {
                        lines.Add(new Line
                        {
                            Token = "$vs_need_damage",
                            Met = SettlerWork.CountDamagedAround(home) > 0,
                        });
                    }
                    break;
                case SettlerJob.Blacksmith:
                    if (gated)
                    {
                        lines.Add(new Line
                        {
                            Token = "$vs_need_forge",
                            Met = SettlerWork.HasStationAround(home, "$piece_forge"),
                        });
                    }
                    lines.Add(new Line
                    {
                        Token = "$vs_need_ore",
                        Met = SettlerWork.CanConvertAround(home, SettlerWork.SmeltingRecipes),
                    });
                    break;
                case SettlerJob.Cook:
                    if (gated)
                    {
                        lines.Add(new Line
                        {
                            Token = "$vs_need_cookstation",
                            Met = SettlerWork.HasAround<CookingStation>(home),
                        });
                    }
                    lines.Add(new Line
                    {
                        Token = "$vs_need_rawfood",
                        Met = SettlerWork.CanConvertAround(home, SettlerWork.CookingRecipes),
                    });
                    break;
                case SettlerJob.Miner:
                    lines.Add(Storage(home, "Stone"));
                    break;
                case SettlerJob.Hunter:
                    lines.Add(Storage(home, "RawMeat"));
                    break;
                case SettlerJob.Courier:
                    lines.Add(new Line
                    {
                        Token = "$vs_need_dest",
                        Met = SettlerCourier.FindPartner(home) != null,
                    });
                    lines.Add(new Line
                    {
                        Token = "$vs_need_surplus",
                        Met = HasSurplus(home),
                    });
                    break;
                case SettlerJob.Herder:
                    lines.Add(new Line
                    {
                        Token = "$vs_need_animals",
                        Met = HasTamedAnimal(home),
                    });
                    lines.Add(new Line
                    {
                        Token = "$vs_need_feed",
                        Met = SettlerWork.CountItemAround(home, "Carrot") > 0
                            || SettlerWork.CountItemAround(home, "Turnip") > 0,
                    });
                    break;
                case SettlerJob.Brewer:
                    if (gated)
                    {
                        lines.Add(new Line
                        {
                            Token = "$vs_need_fermenter",
                            Met = SettlerWork.HasAround<Fermenter>(home),
                        });
                    }
                    lines.Add(new Line
                    {
                        Token = "$vs_need_brewing",
                        Met = SettlerWork.CanConvertAround(home, SettlerWork.BrewingRecipes),
                    });
                    break;
                case SettlerJob.Engineer:
                    if (gated)
                    {
                        lines.Add(new Line
                        {
                            Token = "$vs_need_workbench",
                            Met = SettlerWork.HasStationAround(home, "$piece_workbench"),
                        });
                    }
                    lines.Add(new Line
                    {
                        Token = "$vs_need_ballista",
                        Met = SettlerWork.HasAround<Turret>(home),
                    });
                    lines.Add(new Line
                    {
                        Token = "$vs_need_boltwood",
                        Met = SettlerWork.CountItemAround(home, "Wood") >= 2
                            || SettlerWork.CountItemAround(home, SettlerWork.BoltPrefab) > 0,
                    });
                    break;
                case SettlerJob.Innkeeper:
                    if (gated)
                    {
                        lines.Add(new Line
                        {
                            Token = "$vs_need_meadhall",
                            Met = SettlerWork.HasAround<Settlements.MeadHallMarker>(home),
                        });
                    }
                    lines.Add(new Line
                    {
                        Token = "$vs_need_mead",
                        Met = SettlerWork.CountItemAround(home, "MeadHealthMinor") > 0
                            || SettlerWork.CountItemAround(home, "BarleyWine") > 0,
                    });
                    break;
                case SettlerJob.Fisher:
                    lines.Add(new Line
                    {
                        Token = "$vs_need_water",
                        Met = SettlerWork.HasWaterAround(home),
                    });
                    lines.Add(Storage(home, "FishRaw"));
                    break;
            }
            return lines;
        }

        /// <summary>Minutes until this settler's next meal, or -1 when not applicable.</summary>
        internal static int MinutesToNextMeal(SettlerRecruitable settler)
        {
            var view = settler != null ? settler.GetComponent<ZNetView>() : null;
            if (view == null || !view.IsValid() || ZNet.instance == null
                || settler.State != SettlerState.Assigned || !ModConfig.FoodUpkeep.Value)
            {
                return -1;
            }
            var nextMeal = view.GetZDO().GetLong(SettlerWork.NextMealKey, 0L);
            if (nextMeal == 0L)
            {
                return -1;
            }
            var seconds = nextMeal - ZNet.instance.GetTimeSeconds();
            return Mathf.Max(0, Mathf.CeilToInt((float)seconds / 60f));
        }

        private static bool HasSurplus(Vector3 home)
        {
            foreach (var prefabName in new[] { "Wood", "Stone", "Coal", "Carrot", "Turnip", "RawMeat" })
            {
                if (SettlerWork.CountItemAround(home, prefabName) > 10)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasTamedAnimal(Vector3 home)
        {
            var radius = ModConfig.SettlementRadius.Value;
            foreach (var tameable in Object.FindObjectsOfType<Tameable>())
            {
                var animal = tameable.GetComponent<Character>();
                if (animal != null && animal.IsTamed() && !animal.IsDead()
                    && Vector3.Distance(animal.transform.position, home) <= radius)
                {
                    return true;
                }
            }
            return false;
        }

        private static Line Storage(Vector3 home, string product)
        {
            return new Line
            {
                Token = "$vs_need_chest",
                Met = SettlerWork.HasStorageForAround(home, product),
            };
        }
    }
}
