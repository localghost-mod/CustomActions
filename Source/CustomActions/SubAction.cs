using System;
using System.Collections.Generic;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    public class SubAction : IExposable
    {
        private Action<SearchResult> _action;
        public Action<SearchResult> action
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

        public Action<SearchResult> ToAction()
        {
            return (Action<SearchResult>)
                Type.GetType(category).GetMethod(name).Invoke(null, parameters.ToArray());
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref category, "category");
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref label, "label");
            Scribe_Collections.Look(ref parameters, "parameters", LookMode.Value);
        }
    }
}
