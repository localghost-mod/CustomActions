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

        // MedicalRecipesUtility.GetFixedPartsToApplyOn
        public static IEnumerable<BodyPartRecord> GetFixedPartsToApplyOn(RecipeDef recipe)
        {
            int num;
            for (int i = 0; i < recipe.appliedOnFixedBodyParts.Count; i = num)
            {
                BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
                List<BodyPartRecord> bpList = allParts;
                for (int j = 0; j < bpList.Count; j = num + 1)
                {
                    BodyPartRecord bodyPartRecord = bpList[j];
                    if (bodyPartRecord.def == part)
                    {
                        yield return bodyPartRecord;
                    }
                    num = j;
                }
                part = null;
                bpList = null;
                num = i + 1;
            }
            for (int i = 0; i < recipe.appliedOnFixedBodyPartGroups.Count; i = num)
            {
                BodyPartGroupDef group = recipe.appliedOnFixedBodyPartGroups[i];
                List<BodyPartRecord> bpList = allParts;
                for (int j = 0; j < bpList.Count; j = num + 1)
                {
                    BodyPartRecord bodyPartRecord2 = bpList[j];
                    if (bodyPartRecord2.groups != null && bodyPartRecord2.groups.Contains(group))
                    {
                        yield return bodyPartRecord2;
                    }
                    num = j;
                }
                group = null;
                bpList = null;
                num = i + 1;
            }
            yield break;
        }

        public static List<Tuple<RecipeDef, BodyPartRecord>> _recipesWithPart;
        public static List<Tuple<RecipeDef, BodyPartRecord>> recipesWithPart
        {
            get
            {
                if (_recipesWithPart == null)
                {
                    _recipesWithPart = new List<Tuple<RecipeDef, BodyPartRecord>>();
                    foreach (var recipe in recipes)
                    {
                        if (recipe.targetsBodyPart)
                        {
                            if (recipe.workerClass == typeof(Recipe_RemoveBodyPart))
                                foreach (var part in allParts)
                                    _recipesWithPart.Add(
                                        new Tuple<RecipeDef, BodyPartRecord>(recipe, part)
                                    );
                            else if (recipe.workerClass == typeof(Recipe_RemoveImplant))
                            {
                                var installImplant = recipes.First(
                                    install => install.addsHediff == recipe.removesHediff
                                );
                                foreach (var part in GetFixedPartsToApplyOn(installImplant))
                                    _recipesWithPart.Add(
                                        new Tuple<RecipeDef, BodyPartRecord>(recipe, part)
                                    );
                            }
                            else
                                foreach (var part in GetFixedPartsToApplyOn(recipe))
                                    _recipesWithPart.Add(
                                        new Tuple<RecipeDef, BodyPartRecord>(recipe, part)
                                    );
                        }
                        else
                            _recipesWithPart.Add(
                                new Tuple<RecipeDef, BodyPartRecord>(recipe, null)
                            );
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
                            // check if has the same recipe
                            if (
                                pawn.BillStack.Bills.Any(
                                    _bill =>
                                        _bill.recipe == recipe
                                        && (_bill as Bill_Medical)?.Part == part
                                )
                            )
                                return;
                            // HealthCardUtility.DrawMedOperationsTab
                            var report = recipe.Worker.AvailableReport(pawn);
                            if (report.Accepted || !report.Reason.NullOrEmpty())
                            {
                                if (recipe.targetsBodyPart)
                                {
                                    var _part = recipe
                                        .Worker.GetPartsToApplyOn(pawn, recipe)
                                        .FirstOrFallback(p => p.Label == part.Label);
                                    if (_part != null)
                                    {
                                        if (recipe.AvailableOnNow(pawn, _part))
                                        {
                                            var bill = new Bill_Medical(recipe, null);
                                            bill.Part = _part;
                                            pawn.BillStack.AddBill(bill);
                                        }
                                    }
                                }
                                else
                                {
                                    var bill = new Bill_Medical(recipe, null);
                                    pawn.BillStack.AddBill(bill);
                                }
                            }
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
