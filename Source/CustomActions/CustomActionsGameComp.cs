using System.Linq;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    // GameComponent to hold the SearchActions
    // SearchAction holds a Action_Find
    // Action_Find have to be inserted into the game's AllActions
    class CustomActionsGameComp : GameComponent
    {
        public SearchActionGroup actions = new SearchActionGroup();

        public CustomActionsGameComp(Game g)
            : base() { }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref actions, "actions");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (actions == null)
                    actions = new SearchActionGroup();
            }
        }

        public bool HasSavedAction(string name) => actions.Any(sa => name == sa.search.name);

        public void AddAction(QuerySearch search) => AddAction(new QuerySearchAction(search));

        public void AddAction(QuerySearchAction newSearchAction) => actions.TryAdd(newSearchAction);

        public void AddActions(SearchGroup searches)
        {
            foreach (QuerySearch search in searches)
            {
                QuerySearchAction newSearchAction = new QuerySearchAction(search);

                if (HasSavedAction(newSearchAction.search.name))
                    newSearchAction.search.name += "TD.CopyNameSuffix".Translate();

                actions.Add(newSearchAction);
            }
        }

        public void RenameAction(QuerySearchAction searchAction)
        {
            Find.WindowStack.Add(
                new Dialog_Name(
                    searchAction.search.name,
                    name => searchAction.search.name = name,
                    rejector: name => actions.Any(sa => sa.search.name == name)
                )
            );
        }

        public void RemoveAction(QuerySearchAction searchAction)
        {
            actions.Remove(searchAction);
        }

        public override void GameComponentTick()
        {
            actions.ForEach(searchAction => searchAction.action.DoAction());
        }
    }

    public class SearchActionGroup : SearchGroupBase<QuerySearchAction>
    {
        public override void Replace(QuerySearchAction newSearchAction, int i)
        {
            base.Replace(newSearchAction, i);
        }

        public override void Copy(QuerySearchAction newSearchAction, int i)
        {
            base.Copy(newSearchAction, i);
        }

        public override void DoAdd(QuerySearchAction newSearchAction)
        {
            base.DoAdd(newSearchAction);
        }

        public SearchGroup AsSearchGroup()
        {
            SearchGroup clone = new SearchGroup("CustomActions.CustomActions".Translate(), null);
            clone.AddRange(this.Select(sa => sa.Search));
            return clone;
        }
    }
}
