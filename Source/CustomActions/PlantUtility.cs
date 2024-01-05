using System;
using RimWorld;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    public static class PlantUtility
    {
        public static Action<SearchResult> Harvest()
        {
            return result =>
                result.allThings.ForEach(thing =>
                {
                    var plant = thing as Plant;
                    if (plant != null)
                    {
                        var designationManager = plant.Map.designationManager;
                        if (designationManager.DesignationOn(plant) == null)
                            plant.Map.designationManager.AddDesignation(
                                new Designation(plant, DesignationDefOf.HarvestPlant)
                            );
                    }
                });
        }
    }
}
