using HarmonyLib;
using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    class SearchEditorWindow : TD_Find_Lib.SearchEditorWindow
    {
        public SearchEditorWindow(QuerySearch search)
            : base(search, null)
        {
            title = "TD.Editing".Translate();
            showNameAfterTitle = true;
        }

        public override void Import(QuerySearch newSearch)
        {
            Search.name = newSearch.name;
            Search.parameters = newSearch.parameters.Clone();
            Traverse.Create(filter).Field("children").SetValue(newSearch.Children);
        }
    }
}
