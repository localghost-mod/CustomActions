using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TD_Find_Lib;
using UnityEngine;
using Verse;
using static HarmonyLib.AccessTools;

namespace CustomActions
{
    public static class ListExtension
    {
        public static List<Thing> FirstOrAll(this List<Thing> list, int count) => count == 0 ? list : list.Take(count).ToList();
    }

    public static class DesignatorsUtility
    {
        public static Dictionary<string, Designator> _getDesignator = new Dictionary<string, Designator>();

        public static Designator GetDesignator(string name) =>
            _getDesignator.ContainsKey(name)
                ? _getDesignator[name]
                : (_getDesignator[name] = TypeByName(name) == null ? null : (Designator)TypeByName(name).GetConstructor(new Type[0]).Invoke(null));

        public static Action<SearchResult, int> TryDesignate(string name) =>
            (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .Where(thing => GetDesignator(name).CanDesignateThing(thing))
                    .ToList()
                    .ForEach(thing => GetDesignator(name).DesignateThing(thing));

        private static IEnumerable<SubAction> actions;

        public static IEnumerable<SubAction> Actions =>
            actions
            ?? (
                actions = DesignatorNames
                    .Where(name => GetDesignator(name) != null)
                    .Select(name => new SubAction("CustomActions.DesignatorsUtility:TryDesignate", GetDesignator(name).LabelCap, new List<string> { name }))
            );

        public static List<string> DesignatorNames =>
            new List<string>
            {
                "RimWorld.Designator_PlantsHarvest",
                "RimWorld.Designator_PlantsHarvestWood",
                "RimWorld.Designator_PlantsCut",
                "RimWorld.Designator_Mine",
                "RimWorld.Designator_Deconstruct",
                "RimWorld.Designator_Haul",
                "RimWorld.Designator_Tame",
                "RimWorld.Designator_Hunt",
                "RimWorld.Designator_ReleaseAnimalToWild",
                "RimWorld.Designator_Slaughter",
                "RimWorld.Designator_Strip",
                "RimWorld.Designator_Unforbid",
                "RimWorld.Designator_Cancel",
                "AllowTool.Designator_HaulUrgently",
                "AllowTool.Designator_FinishOff",
                "CaptureThem.Designator_CapturePawn",
                "EasyUpgrades.Designator_IncreaseQuality",
                "RimWorld___Improve_This.Designator_ImproveThis",
                "RimWorld___Improve_This.Designator_ImproveThisClear",
            };
        public static Func<List<SubAction>, IEnumerable<FloatMenuOption>> Options = subActions =>
            Actions.Select(action => new FloatMenuOption(action.label, () => subActions.Add(action)));
    }

    [StaticConstructorOnStartup]
    static class Startup
    {
        static Startup()
        {
            DesignatorsUtility.DesignatorNames.ForEach(name => DesignatorsUtility.GetDesignator(name));
        }
    }
}
