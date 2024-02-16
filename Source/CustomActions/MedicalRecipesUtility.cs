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
        public static IEnumerable<RecipeDef> _recipes;
        public static IEnumerable<RecipeDef> Recipes =>
            _recipes ?? (_recipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where(recipe => recipe.workerClass.IsSubclassOf(typeof(Recipe_Surgery))));
        private static List<BodyPartRecord> _allParts;
        public static List<BodyPartRecord> AllPart =>
            _allParts ?? (_allParts = DefDatabase<BodyDef>.AllDefs.Select(def => def.AllParts).SelectMany(x => x).GroupBy(part => part.Label).Select(x => x.First()).ToList());

        private static Dictionary<RecipeDef, IEnumerable<BodyPartRecord>> _bodyparts = new Dictionary<RecipeDef, IEnumerable<BodyPartRecord>>();

        // MedicalRecipesUtility.GetFixedPartsToApplyOn
        public static IEnumerable<BodyPartRecord> GetFixedPartsToApplyOn(RecipeDef recipe)
        {
            if (!_bodyparts.ContainsKey(recipe))
            {
                if (recipe.targetsBodyPart)
                {
                    if (recipe.workerClass == typeof(Recipe_RemoveBodyPart))
                        _bodyparts[recipe] = AllPart;
                    else if (recipe.workerClass == typeof(Recipe_RemoveImplant))
                    {
                        var installImplant = Recipes.First(install => install.addsHediff == recipe.removesHediff);
                        _bodyparts[recipe] = GetFixedPartsToApplyOn(installImplant);
                    }
                    else
                    {
                        if (!recipe.appliedOnFixedBodyParts.NullOrEmpty())
                            _bodyparts[recipe] = recipe.appliedOnFixedBodyParts.Select(part => AllPart.Where(record => record.def == part)).SelectMany(x => x);
                        else if (!recipe.appliedOnFixedBodyPartGroups.NullOrEmpty())
                            _bodyparts[recipe] = recipe
                                .appliedOnFixedBodyPartGroups.Select(group => AllPart.Where(record => record.groups?.Contains(group) ?? false))
                                .SelectMany(x => x);
                        else
                            _bodyparts[recipe] = AllPart;
                    }
                }
                else
                    _bodyparts[recipe] = new List<BodyPartRecord> { null };
            }
            return _bodyparts[recipe];
        }

        public static Action<SearchResult, int> MedicalRecipe(string recipeName, string partName)
        {
            var recipe = DefDatabase<RecipeDef>.GetNamed(recipeName);
            return (result, count) =>
                result
                    .allThings.FirstOrAll(count)
                    .ForEach(thing =>
                    {
                        if (thing is Pawn pawn && pawn.BillStack.Bills.All(bill => bill.recipe != recipe || (bill as Bill_Medical)?.Part?.Label != partName))
                        {
                            var parts = recipe.Worker.GetPartsToApplyOn(pawn, recipe);
                            var part = parts.FirstOrFallback(_part => _part.Label == partName);
                            if (recipe.Worker.AvailableReport(pawn, part))
                            {
                                // HealthCardUtility.DrawMedOperationsTab
                                if (recipe.targetsBodyPart)
                                {
                                    if (part != null)
                                        pawn.BillStack.AddBill(new Bill_Medical(recipe, null) { Part = part });
                                }
                                else
                                    pawn.BillStack.AddBill(new Bill_Medical(recipe, null));
                            }
                        }
                    });
        }

        public static Action<SearchResult, int> ClearBillStack() => (result, count) => result.allThings.FirstOrAll(count).ForEach(thing => (thing as Pawn)?.BillStack.Clear());

        public static string ToString(RecipeDef recipe, BodyPartRecord record) => _bodyparts[recipe].Count() == 1 ? recipe.label : $"{recipe.label} ({record.Label})";

        private static Dictionary<RecipeDef, IEnumerable<SubAction>> actions;

        public static Dictionary<RecipeDef, IEnumerable<SubAction>> Actions =>
            actions
            ?? (
                actions = Recipes.ToDictionary(
                    recipe => recipe,
                    recipe =>
                        GetFixedPartsToApplyOn(recipe)
                            .Select(
                                record =>
                                    new SubAction("CustomActions.MedicalRecipesUtility:MedicalRecipe", ToString(recipe, record), new List<string> { recipe.defName, record?.Label })
                            )
                )
            );
        public static Func<List<SubAction>, IEnumerable<FloatMenuOption>> Options = subActions =>
            new List<FloatMenuOption>
            {
                new FloatMenuOption(
                    "MedicalOperations".Translate(),
                    () =>
                        Find.WindowStack.Add(
                            new FloatMenu(
                                Recipes
                                    .Select(
                                        recipe =>
                                            new FloatMenuOption(
                                                recipe.LabelCap,
                                                () =>
                                                {
                                                    if (Actions[recipe].Count() == 1)
                                                        subActions.Add(Actions[recipe].First());
                                                    else
                                                        Find.WindowStack.Add(
                                                            new FloatMenu(
                                                                Actions[recipe].Select(action => new FloatMenuOption(action.label, () => subActions.Add(action))).ToList()
                                                            )
                                                        );
                                                }
                                            )
                                    )
                                    .ToList()
                            )
                        )
                ),
                new FloatMenuOption(
                    "CustomActions.ClearBillStack".Translate(),
                    () => subActions.Add(new SubAction("CustomActions.MedicalRecipesUtility:ClearBillStack", "CustomActions.ClearBillStack".Translate()))
                )
            };
    }
}
