using System;
using System.Collections.Generic;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    public class QuerySearchAction : IExposable, IQuerySearch
    {
        public QuerySearch search;
        public QuerySearch Search => search;
        public Action_QuerySearch action;

        public QuerySearchAction()
        {
            this.action = new Action_QuerySearch(this);
        }

        public QuerySearchAction(QuerySearch search, bool enabled = false)
        {
            this.search = search;
            this.search.active = true;

            this.action = new Action_QuerySearch(this);
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref search, "search");
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
        public QuerySearchAction searchAction;
        public List<SubAction> subActions;

        public Action<SearchResult> _action;
        public Action<SearchResult> action =>
            _action
            ?? (_action = result => subActions.ForEach(subAction => subAction.action(result)));

        public bool enabled = false;
        // TODO
        public int maxActionCount = -1; // the maximal count to do action, -1 means loop
        public int curActionCount = 0; // should be able to reset to 0
        public int actionInterval = 1000; // the least time (in seconds) to do the next action, should not be too small, default: 1000 (a day).
        public int countToAction = 0;
        private int lastActionTick = -1;
        public CompareType compareType;

        public static bool enableAll = true;

        public static readonly string transferTags =
            SearchActionTransfer.TransferTag + "," + Settings.StorageTransferTag;

        public Action_QuerySearch(QuerySearchAction searchAction)
        {
            this.searchAction = searchAction;
            this.subActions = new List<SubAction>();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref maxActionCount, "maxActionCount");
            Scribe_Values.Look(ref curActionCount, "curActionCount");
            Scribe_Values.Look(ref actionInterval, "actionInterval");
            Scribe_Values.Look(ref countToAction, "countToAction");
            Scribe_Values.Look(ref compareType, "countComp");
            Scribe_Values.Look(ref enabled, "enabled");
            Scribe_Collections.Look(ref subActions, "subActions");
        }

        public void DoAction()
        {
            if (
                !enableAll
                || !enabled
                || subActions.NullOrEmpty()
                || (maxActionCount != -1 && curActionCount == maxActionCount)
                || (
                    curActionCount > 0
                    && Find.TickManager.TicksGame - lastActionTick
                        < actionInterval * GenTicks.TicksPerRealSecond
                )
            )
                return;
            var result = SearchResult();
            // fix lagging if not active,
            // TODO: rename to lastSearchTick when release
            lastActionTick = Find.TickManager.TicksGame;
            int count = result.allThingsCount;
            bool active =
                compareType == CompareType.Greater
                    ? count > countToAction
                    : compareType == CompareType.Equal
                        ? count == countToAction
                        : compareType == CompareType.Less
                            ? count < countToAction
                            : false;
            if (!active)
                return;
            action(result);
            curActionCount += 1;
        }

        private SearchResult SearchResult()
        {
            searchAction.search.RemakeList();
            return searchAction.search.result;
        }
    }
}
