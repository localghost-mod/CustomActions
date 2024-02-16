using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TD_Find_Lib;
using Verse;
using static HarmonyLib.AccessTools;

namespace CustomActions
{
    public class SubAction : IExposable
    {
        private Action<SearchResult, int> _action;
        public Action<SearchResult, int> Action => _action ?? (_action = ToAction());
        public string typeColonName;
        public string label;
        public List<string> parameters;

        public SubAction() { }

        public SubAction(string typeColonName, string label = "unnamed subaction", List<string> parameters = null)
        {
            this.typeColonName = typeColonName;
            this.label = label;
            this.parameters = parameters;
        }

        public Action<SearchResult, int> ToAction() => (Action<SearchResult, int>)Method(typeColonName).Invoke(null, parameters.ToArray());

        public void ExposeData()
        {
            Scribe_Values.Look(ref typeColonName, "typeColonName");
            Scribe_Values.Look(ref label, "label");
            Scribe_Collections.Look(ref parameters, "parameters");
        }

        public static Func<List<SubAction>, IEnumerable<FloatMenuOption>> Options = subActions =>
            DesignatorsUtility.Options(subActions).Concat(MedicalRecipesUtility.Options(subActions));
    }
}
