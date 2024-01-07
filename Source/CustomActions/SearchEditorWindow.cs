using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    class SearchEditorWindow : SearchEditorRevertableWindow
    {
        public SearchEditorWindow(QuerySearch search)
            : base(search, SearchActionTransfer.TransferTag)
        {
            title = "TD.Editing".Translate();
            showNameAfterTitle = true;
        }
    }
}
