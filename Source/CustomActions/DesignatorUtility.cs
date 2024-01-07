using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    public static class ListExtension
    {
        public static List<Thing> FirstOrAll(this List<Thing> list, int count) =>
            count == 0 ? list : list.Take(count).ToList();
    }

    public static class DesignatorUtility
    {
        public static Action<SearchResult, int> Harvest()
        {
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(thing =>
                    {
                        if (
                            Find.ReverseDesignatorDatabase.Get<Designator_PlantsHarvest>()
                                .CanDesignateThing(thing)
                            || Find.ReverseDesignatorDatabase.Get<Designator_PlantsHarvestWood>()
                                .CanDesignateThing(thing)
                        )
                            thing.Map.designationManager.AddDesignation(
                                new Designation(thing, DesignationDefOf.HarvestPlant)
                            );
                    });
        }

        public static Action<SearchResult, int> Mine()
        {
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(thing =>
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

        public static Action<SearchResult, int> Deconstruct()
        {
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(thing =>
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

        public static Action<SearchResult, int> HaulThings()
        {
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(thing =>
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

        public static Action<SearchResult, int> Tame()
        {
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(thing =>
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

        public static Action<SearchResult, int> Hunt()
        {
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(thing =>
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
