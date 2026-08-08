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
                    lines.Add(new Line
                    {
                        Token = "$vs_need_damage",
                        Met = SettlerWork.CountDamagedAround(home) > 0,
                    });
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
