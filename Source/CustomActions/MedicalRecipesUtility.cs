using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    public static class MedicalRecipesUtility
    {
        /*
            public static List<Type> medicalRecipeTypes = new List<Type>()
                {
                    typeof(Recipe_AdministerIngestible),
                    typeof(Recipe_AdministerUsableItem),
                    typeof(Recipe_BloodTransfusion),
                    typeof(Recipe_ChangeImplantLevel),
                    typeof(Recipe_ExtractHemogen),
                    typeof(Recipe_ImplantEmbryo),
                    typeof(Recipe_ImplantXenogerm),
                    typeof(Recipe_InstallArtificialBodyPart),
                    typeof(Recipe_InstallImplant),
                    typeof(Recipe_InstallNaturalBodyPart),
                    typeof(Recipe_RemoveBodyPart),
                    typeof(Recipe_RemoveHediff),
                    typeof(Recipe_RemoveImplant),
                    typeof(Recipe_TerminatePregnancy),
                };
        */
        public static IEnumerable<RecipeDef> recipes =
            DefDatabase<RecipeDef>.AllDefsListForReading.Where(
                recipe => recipe.workerClass.IsSubclassOf(typeof(Recipe_Surgery))
            );
        public static Dictionary<string, RecipeDef> getRecipeByName = recipes.ToDictionary(
            recipe => recipe.defName,
            recipe => recipe
        );
        public static List<BodyPartRecord> allParts = BodyDefOf.Human.AllParts;
        public static Dictionary<string, BodyPartRecord> getPartByName = allParts.ToDictionary(
            part => part.Label,
            part => part
        );
        public static List<Tuple<RecipeDef, BodyPartRecord>> _recipesWithPart;
        public static List<Tuple<RecipeDef, BodyPartRecord>> recipesWithPart
        {
            get
            {
                if (_recipesWithPart == null)
                {
                    _recipesWithPart = new List<Tuple<RecipeDef, BodyPartRecord>>();
                    // RimWorld.MedicalRecipesUtility
                    // handle the recipe with appliedOnFixedBodyParts or appliedOnFixedBodyPartGroups
                    var recordOnPart = new Dictionary<BodyPartDef, List<BodyPartRecord>>();
                    allParts.ForEach(
                        record => recordOnPart.SetOrAdd(record.def, new List<BodyPartRecord>())
                    );
                    allParts.ForEach(record => recordOnPart[record.def].Add(record));

                    var recordInGroup = new Dictionary<BodyPartGroupDef, List<BodyPartRecord>>();
                    allParts.ForEach(
                        record =>
                            record.groups.ForEach(
                                group => recordInGroup.SetOrAdd(group, new List<BodyPartRecord>())
                            )
                    );
                    allParts.ForEach(
                        record => record.groups.ForEach(group => recordInGroup[group].Add(record))
                    );

                    foreach (var recipe in recipes)
                    {
                        recipe.appliedOnFixedBodyParts.ForEach(
                            bodypart =>
                                recordOnPart[bodypart].ForEach(
                                    record =>
                                        _recipesWithPart.Add(
                                            new Tuple<RecipeDef, BodyPartRecord>(recipe, record)
                                        )
                                )
                        );
                        recipe.appliedOnFixedBodyPartGroups.ForEach(
                            group =>
                                recordInGroup[group].ForEach(
                                    record =>
                                        _recipesWithPart.Add(
                                            new Tuple<RecipeDef, BodyPartRecord>(recipe, record)
                                        )
                                )
                        );
                        if (
                            recipe.appliedOnFixedBodyParts.NullOrEmpty()
                            && recipe.appliedOnFixedBodyPartGroups.NullOrEmpty()
                        )
                        {
                            if (recipe.workerClass == typeof(Recipe_RemoveBodyPart))
                                allParts.ForEach(
                                    record =>
                                        _recipesWithPart.Add(
                                            new Tuple<RecipeDef, BodyPartRecord>(recipe, record)
                                        )
                                );
                            else
                                _recipesWithPart.Add(
                                    new Tuple<RecipeDef, BodyPartRecord>(recipe, null)
                                );
                        }
                    }
                }
                return _recipesWithPart;
            }
        }

        public static Action<SearchResult, int> MedicalRecipe(string recipeName, string partName)
        {
            var recipe = getRecipeByName[recipeName];
            var part = partName == null ? null : getPartByName[partName];
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(thing =>
                    {
                        var pawn = thing as Pawn;
                        if (pawn != null)
                        {
                            // check if the pawn has the part
                            if (
                                part != null
                                && !pawn.health.hediffSet.GetNotMissingParts().Contains(part)
                            )
                                return;
                            // check if has the same recipe
                            if (
                                pawn.BillStack.Bills.Any(
                                    _bill =>
                                        _bill.recipe == recipe
                                        && (_bill as Bill_Medical)?.Part == part
                                )
                            )
                                return;
                            // check if the hediff exists
                            if (
                                recipe.addsHediff != null
                                && pawn.health.hediffSet.HasHediff(recipe.addsHediff)
                            )
                                return;
                            var bill = new Bill_Medical(recipe, null);
                            bill.Part = part;
                            pawn.BillStack.AddBill(bill);
                        }
                    });
        }

        public static Action<SearchResult, int> ClearBillStack()
        {
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(thing => (thing as Pawn)?.BillStack.Clear());
        }

        public static string ToString(Tuple<RecipeDef, BodyPartRecord> recipeWithPart)
        {
            if (recipeWithPart.Item2 == null)
                return recipeWithPart.Item1.label;
            else
                return recipeWithPart.Item1.label + "(" + recipeWithPart.Item2?.Label + ")";
        }

        public static void LogAllRecipesWithRecord()
        {
            foreach (var recipeWithPart in recipesWithPart)
                Log.Message(ToString(recipeWithPart));
        }

        public static void LogAll()
        {
            LogAllRecipesWithRecord();
        }

        private static List<SubAction> actions;

        public static List<SubAction> Actions()
        {
            if (actions == null)
            {
                actions = recipesWithPart
                    .Select(
                        recipeWithPart =>
                            new SubAction(
                                "CustomActions.MedicalRecipesUtility",
                                "MedicalRecipe",
                                new List<string>()
                                {
                                    recipeWithPart.Item1.defName,
                                    recipeWithPart.Item2?.Label
                                },
                                ToString(recipeWithPart)
                            )
                    )
                    .ToList();
            }
            return actions;
        }
    }
}
