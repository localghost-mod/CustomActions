using System;
using System.Collections.Generic;
using RimWorld;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    public class QueryAction : IExposable
    {
        public enum CompareType
        {
            Greater,
            Equal,
            Less
        }

        public static string ToString(CompareType compareType)
        {
            if (compareType == CompareType.Greater)
                return ">";
            if (compareType == CompareType.Equal)
                return "=";
            if (compareType == CompareType.Less)
                return "<";
            return null;
        }

        public bool folded = false;
        public bool enabled = false;
        public string label = "CustomActions.NewAction".Translate();
        public QuerySearch filter = new QuerySearch { name = "CustomActions.NewFilter".Translate(), active = true };
        public QuerySearch trigger = new QuerySearch { name = "CustomActions.NewTrigger".Translate(), active = true };
        public CompareType compareType;
        public int countToTrigger = 0;
        public int count = 0;
        public int searchInterval = 1000;
        private int lastSearchTick = -1;
        public List<SubAction> subactions = new List<SubAction>();
        private Action<SearchResult, int> _action;
        public Action<SearchResult, int> Action => _action ?? (_action = (result, count) => subactions.ForEach(subAction => subAction.Action(result, count)));

        public QueryAction()
        {
            filter.SetSearchCurrentMap();
            trigger.SetSearchCurrentMap();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref enabled, "enabled");
            Scribe_Values.Look(ref label, "label");
            Scribe_Deep.Look(ref filter, "filter");
            Scribe_Deep.Look(ref trigger, "trigger");
            Scribe_Values.Look(ref compareType, "type");
            Scribe_Values.Look(ref countToTrigger, "countToTrigger");
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref searchInterval, "searchInterval");
            Scribe_Values.Look(ref lastSearchTick, "lastSearchTick");
            Scribe_Collections.Look(ref subactions, "subactions");
        }

        public void DoAction()
        {
            if (!enabled || subactions.NullOrEmpty() || Find.TickManager.TicksGame - lastSearchTick < searchInterval * GenTicks.TicksPerRealSecond)
                return;
            trigger.RemakeList();
            lastSearchTick = Find.TickManager.TicksGame;
            int triggerCount = trigger.result.allThingsCount;
            bool active =
                compareType == CompareType.Greater
                    ? triggerCount > countToTrigger
                    : compareType == CompareType.Equal
                        ? triggerCount == countToTrigger
                        : compareType == CompareType.Less
                            ? triggerCount < countToTrigger
                            : false;
            if (!active)
                return;
            filter.RemakeList();
            Action(filter.result, count);
        }
    }
}
