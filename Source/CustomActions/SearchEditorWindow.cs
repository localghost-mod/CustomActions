using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    class SearchEditorWindow : TD_Find_Lib.SearchEditorWindow
    {
        public SearchEditorWindow(QuerySearch search)
            : base(search, SearchActionTransfer.TransferTag)
        {
            title = "TD.Editing".Translate();
            showNameAfterTitle = true;
        }
        public override void Import(QuerySearch newSearch) => search = newSearch.CloneForUse();
    }
}
