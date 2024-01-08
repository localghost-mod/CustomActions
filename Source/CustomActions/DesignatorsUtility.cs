using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TD_Find_Lib;
using Verse;
using static HarmonyLib.AccessTools;

namespace CustomActions
{
    public static class ListExtension
    {
        public static List<Thing> FirstOrAll(this List<Thing> list, int count) =>
            count == 0 ? list : list.Take(count).ToList();
    }

    public static class DesignatorsUtility
    {
        public static Action<SearchResult, int> TryDesignate(List<Designator> designators)
        {
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(
                        thing =>
                            designators.ForEach(designator =>
                            {
                                if (designator.CanDesignateThing(thing))
                                    designator.DesignateThing(thing);
                            })
                    );
        }

        public static Action<SearchResult, int> Harvest() =>
            TryDesignate(
                new List<Designator>
                {
                    Find.ReverseDesignatorDatabase.Get<Designator_PlantsHarvest>(),
                    Find.ReverseDesignatorDatabase.Get<Designator_PlantsHarvestWood>()
                }
            );

        public static Action<SearchResult, int> Mine() =>
            TryDesignate(
                new List<Designator> { Find.ReverseDesignatorDatabase.Get<Designator_Mine>() }
            );

        public static Action<SearchResult, int> Deconstruct() =>
            TryDesignate(
                new List<Designator>
                {
                    Find.ReverseDesignatorDatabase.Get<Designator_Deconstruct>()
                }
            );

        public static Action<SearchResult, int> HaulThings() =>
            TryDesignate(
                new List<Designator> { Find.ReverseDesignatorDatabase.Get<Designator_Haul>() }
            );

        public static Action<SearchResult, int> Tame() =>
            TryDesignate(
                new List<Designator> { Find.ReverseDesignatorDatabase.Get<Designator_Tame>() }
            );

        public static Action<SearchResult, int> Hunt() =>
            TryDesignate(
                new List<Designator> { Find.ReverseDesignatorDatabase.Get<Designator_Hunt>() }
            );

        public static Action<SearchResult, int> Strip() =>
            TryDesignate(
                new List<Designator> { Find.ReverseDesignatorDatabase.Get<Designator_Strip>() }
            );

        public static Action<SearchResult, int> Cancel() =>
            TryDesignate(
                new List<Designator> { Find.ReverseDesignatorDatabase.Get<Designator_Cancel>() }
            );

        public static Action<SearchResult, int> HaulUrgently()
        {
            var Designator_HaulUrgently = (Designator)
                TypeByName("AllowTool.Designator_HaulUrgently")
                    .GetConstructor(new Type[] { })
                    .Invoke(new object[] { });
            return TryDesignate(new List<Designator> { Designator_HaulUrgently });
        }

        public static Action<SearchResult, int> FinishOff()
        {
            var Designator_FinishOff = (Designator)
                TypeByName("AllowTool.Designator_FinishOff")
                    .GetConstructor(new Type[] { })
                    .Invoke(new object[] { });
            return TryDesignate(new List<Designator> { Designator_FinishOff });
        }

        public static Action<SearchResult, int> CapturePawn()
        {
            var Designator_CapturePawn = (Designator)
                TypeByName("CaptureThem.Designator_CapturePawn")
                    ?.GetConstructor(new Type[] { })
                    .Invoke(new object[] { });
            return TryDesignate(new List<Designator> { Designator_CapturePawn });
        }

        private static List<SubAction> actions;

        public static List<SubAction> Actions()
        {
            if (actions == null)
                actions = new List<string>
                {
                    "Harvest",
                    "Mine",
                    "Deconstruct",
                    "HaulThings",
                    "Tame",
                    "Hunt",
                    "Strip",
                    "Cancel"
                }
                    .Select(
                        designatorName =>
                            new SubAction(
                                "CustomActions.DesignatorsUtility",
                                designatorName,
                                new List<string>(),
                                ("Designator" + designatorName).Translate()
                            )
                    )
                    .Concat(ModActions())
                    .ToList();
            return actions;
        }

        public static List<SubAction> ModActions()
        {
            var actions = new List<SubAction>();

            var Designator_HaulUrgently = (Designator)
                TypeByName("AllowTool.Designator_HaulUrgently")
                    ?.GetConstructor(new Type[] { })
                    .Invoke(new object[] { });
            if (Designator_HaulUrgently != null)
                actions.Add(
                    new SubAction(
                        "CustomActions.DesignatorsUtility",
                        "HaulUrgently",
                        new List<string>(),
                        Designator_HaulUrgently.Label
                    )
                );

            var Designator_FinishOff = (Designator)
                TypeByName("AllowTool.Designator_FinishOff")
                    ?.GetConstructor(new Type[] { })
                    .Invoke(new object[] { });
            if (Designator_FinishOff != null)
                actions.Add(
                    new SubAction(
                        "CustomActions.DesignatorsUtility",
                        "FinishOff",
                        new List<string>(),
                        Designator_FinishOff.Label
                    )
                );
            var Designator_CapturePawn = (Designator)
                TypeByName("CaptureThem.Designator_CapturePawn")
                    ?.GetConstructor(new Type[] { })
                    .Invoke(new object[] { });
            if (Designator_CapturePawn != null)
                actions.Add(
                    new SubAction(
                        "CustomActions.DesignatorsUtility",
                        "CapturePawn",
                        new List<string>(),
                        Designator_CapturePawn.Label
                    )
                );
            return actions;
        }
    }
}
