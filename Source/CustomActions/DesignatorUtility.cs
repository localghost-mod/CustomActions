using System;
using RimWorld;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    public static class DesignatorUtility
    {
        public static Action<SearchResult> Harvest()
        {
            return result =>
                result.allThings.ForEach(thing =>
                {
                    if (
                        Find.ReverseDesignatorDatabase.Get<Designator_PlantsHarvest>()
                            .CanDesignateThing(thing)
                    )
                        thing.Map.designationManager.AddDesignation(
                            new Designation(thing, DesignationDefOf.HarvestPlant)
                        );
                });
        }

        public static Action<SearchResult> Mine()
        {
            return result =>
                result.allThings.ForEach(thing =>
                {
                    if (
                        Find.ReverseDesignatorDatabase.Get<Designator_Mine>()
                            .CanDesignateThing(thing)
                    )
                        thing.Map.designationManager.AddDesignation(
                            new Designation(thing, DesignationDefOf.Mine)
                        );
                });
        }

        public static Action<SearchResult> Deconstruct()
        {
            return result =>
                result.allThings.ForEach(thing =>
                {
                    if (
                        Find.ReverseDesignatorDatabase.Get<Designator_Deconstruct>()
                            .CanDesignateThing(thing)
                    )
                        thing.Map.designationManager.AddDesignation(
                            new Designation(thing, DesignationDefOf.Deconstruct)
                        );
                });
        }

        public static Action<SearchResult> HaulThings()
        {
            return result =>
                result.allThings.ForEach(thing =>
                {
                    if (
                        Find.ReverseDesignatorDatabase.Get<Designator_Haul>()
                            .CanDesignateThing(thing)
                    )
                        thing.Map.designationManager.AddDesignation(
                            new Designation(thing, DesignationDefOf.Haul)
                        );
                });
        }

        public static Action<SearchResult> Tame()
        {
            return result =>
                result.allThings.ForEach(thing =>
                {
                    if (
                        Find.ReverseDesignatorDatabase.Get<Designator_Tame>()
                            .CanDesignateThing(thing)
                    )
                        thing.Map.designationManager.AddDesignation(
                            new Designation(thing, DesignationDefOf.Tame)
                        );
                });
        }

        public static Action<SearchResult> Hunt()
        {
            return result =>
                result.allThings.ForEach(thing =>
                {
                    if (
                        Find.ReverseDesignatorDatabase.Get<Designator_Hunt>()
                            .CanDesignateThing(thing)
                    )
                        thing.Map.designationManager.AddDesignation(
                            new Designation(thing, DesignationDefOf.Hunt)
                        );
                });
        }
    }
}
