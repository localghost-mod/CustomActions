using System;
using System.Collections.Generic;
using System.Linq;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    public class SubAction : IExposable
    {
        private Action<SearchResult, int> _action;
        public Action<SearchResult, int> action
        {
            get { return _action ?? (_action = ToAction()); }
        }
        public string category;
        public string name;
        public string label;
        public List<string> parameters;

        public SubAction() { }

        public SubAction(string category, string name, List<string> parameters, string label)
        {
            this.category = category;
            this.name = name;
            this.parameters = parameters;
            this.label = label;
        }

        public Action<SearchResult, int> ToAction()
        {
            return (Action<SearchResult, int>)
                Type.GetType(category).GetMethod(name).Invoke(null, parameters.ToArray());
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref category, "category");
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref label, "label");
            Scribe_Collections.Look(ref parameters, "parameters", LookMode.Value);
        }

        public static Func<List<SubAction>, List<FloatMenuOption>> Options = subActions =>
        {
            var designatorOptions = DesignatorsUtility
                .Actions()
                .Select(action => new FloatMenuOption(action.label, () => subActions.Add(action)))
                .ToList();
            var medicalRecipeOptions = MedicalRecipesUtility
                .Actions()
                .Select(action => new FloatMenuOption(action.label, () => subActions.Add(action)))
                .ToList();
            var _result = designatorOptions;
            _result.Add(
                new FloatMenuOption(
                    "MedicalOperations".Translate(),
                    () => Find.WindowStack.Add(new FloatMenu(medicalRecipeOptions))
                )
            );
            _result.Add(
                new FloatMenuOption(
                    "CustomActions.ClearBillStack".Translate(),
                    () =>
                        subActions.Add(
                            new SubAction(
                                "CustomActions.MedicalRecipesUtility",
                                "ClearBillStack",
                                new List<string>(),
                                "CustomActions.ClearBillStack".Translate()
                            )
                        )
                )
            );
            return _result;
        };
    }
}
