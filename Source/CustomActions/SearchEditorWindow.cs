using TD_Find_Lib;
using Verse;

namespace CustomActions
{
    class SearchEditorWindow : SearchEditorRevertableWindow
    {
        QuerySearchAction searchAction;

        public SearchEditorWindow(QuerySearchAction searchAction)
            : base(searchAction.search, SearchActionTransfer.TransferTag)
        {
            this.searchAction = searchAction;

            title = "CustomActions.EditingAction".Translate();
            showNameAfterTitle = true;
        }

        public override void PostClose()
        {
            if (search.changed)
            {
                if (!searchAction.action.enabled)
                    Find.WindowStack.Add(
                        Dialog_MessageBox.CreateConfirmation(
                            "CustomActions.StartAction".Translate(),
                            () => searchAction.action.enabled = true
                        )
                    );
                else
                    base.PostClose();
            }
        }
    }
}
