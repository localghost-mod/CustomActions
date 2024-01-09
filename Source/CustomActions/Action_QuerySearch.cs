using System;
using System.Collections.Generic;
using RimWorld;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    public class QuerySearchAction : IExposable, IQuerySearch
    {
        public static bool enableAll = true;

        public string name;

        public QuerySearch search;
        public QuerySearch Search => search;
        public Action_QuerySearch action;

        public int searchInterval = 1000;
        public int countToAction = 0;
        public bool enabled = false;

        public CompareType compareType;

        private int lastSearchTick = -1;

        public QuerySearchAction()
        {
            action = new Action_QuerySearch();
            search = new QuerySearch() { name = "TD.NewSearch".Translate(), active = true };
            search.SetSearchCurrentMap();
        }

        public QuerySearchAction(QuerySearch search)
        {
            action = new Action_QuerySearch();
            this.search = search;
            this.search.active = true;
        }

        public void TrySearch()
        {
            if (
                !enableAll
                || !enabled
                || action.Empty
                || Find.TickManager.TicksGame - lastSearchTick
                    < searchInterval * GenTicks.TicksPerRealSecond
            )
                return;
            search.RemakeList();
            lastSearchTick = Find.TickManager.TicksGame;
            int count = search.result.allThingsCount;
            bool active =
                compareType == CompareType.Greater
                    ? count > countToAction
                    : compareType == CompareType.Equal
                        ? count == countToAction
                        : compareType == CompareType.Less
                            ? count < countToAction
                            : false;
            if (active)
                action.DoAction();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref name, "name");
            Scribe_Deep.Look(ref search, "search");
            Scribe_Deep.Look(ref action, "action");
            Scribe_Values.Look(ref searchInterval, "searchInterval");
            Scribe_Values.Look(ref countToAction, "countToAction");
            Scribe_Values.Look(ref enabled, "enabled");
            Scribe_Values.Look(ref compareType, "compareType");
            Scribe_Values.Look(ref lastSearchTick, "lastSearchTick");
            action.ExposeData();
        }
    }

    public enum CompareType
    {
        Greater,
        Equal,
        Less
    }

    public class Action_QuerySearch : IExposable
    {
        public QuerySearch filter;
        public List<SubAction> subActions;
        public int count;

        private Action<SearchResult, int> _action;
        public Action<SearchResult, int> action =>
            _action
            ?? (
                _action = (result, count) =>
                    subActions.ForEach(subAction => subAction.action(result, count))
            );

        public static readonly string transferTags =
            SearchActionTransfer.TransferTag + "," + Settings.StorageTransferTag;

        public bool Empty => subActions.NullOrEmpty();

        public Action_QuerySearch()
        {
            subActions = new List<SubAction>();
            filter = new QuerySearch()
            {
                name = "CustomActions.NewFilter".Translate(),
                active = true
            };
            filter.SetSearchCurrentMap();
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref filter, "filter");
            Scribe_Collections.Look(ref subActions, "subActions");
            Scribe_Values.Look(ref count, "count");
        }

        public void DoAction()
        {
            filter.RemakeList();
            action(filter.result, count);
        }
    }
}
