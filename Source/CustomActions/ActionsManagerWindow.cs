using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TD_Find_Lib;
using UnityEngine;
using Verse;

namespace CustomActions
{
    public class ActionsManagerWindow : Window
    {
        SearchActionListDrawer actionsDrawer;
        CustomActionsGameComp comp;
        public override Vector2 InitialSize => new Vector2(850, 500f);

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            windowRect.x = StandardMargin;
            windowRect.y =
                UI.screenHeight - MainButtonDef.ButtonHeight - windowRect.height - StandardMargin;
        }

        public ActionsManagerWindow()
        {
            preventCameraMotion = false;
            draggable = true;
            resizeable = true;
            closeOnAccept = false;
            doCloseX = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            comp = Current.Game.GetComponent<CustomActionsGameComp>();

            actionsDrawer = new SearchActionListDrawer(comp.actions, this);
        }

        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight;
        private string title = "CustomActions.CustomActions".Translate() + ":";

        public override void DoWindowContents(Rect inRect)
        {
            //Title
            Text.Font = GameFont.Medium;
            Rect titleRect = inRect.TopPartPixels(Text.LineHeight).AtZero();
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;

            // Add action row
            Rect addRect = inRect.LeftHalf().BottomPartPixels(Text.LineHeight);
            WidgetRow addRow = new WidgetRow(addRect.x, addRect.y);

            if (addRow.ButtonIcon(FindTex.GreyPlus))
                PopUpCreateAction();

            addRow.ButtonOpenLibrary();

            //Check off
            Rect enableRect = inRect.RightHalf().BottomPartPixels(Text.LineHeight);
            Widgets.CheckboxLabeled(
                enableRect,
                "CustomActions.EnableActions".Translate(),
                ref QuerySearchAction.enableAll
            );

            //Scrolling!
            inRect.yMin = titleRect.yMax + Listing.DefaultGap;
            inRect.yMax = enableRect.yMin - Listing.DefaultGap;

            Listing_StandardIndent listing = new Listing_StandardIndent();
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, scrollViewHeight);
            listing.BeginScrollView(inRect, ref scrollPosition, viewRect);

            actionsDrawer.DrawQuerySearchList(listing);

            listing.EndScrollView(ref scrollViewHeight);
        }

        public void PopUpCreateAction()
        {
            Find.WindowStack.Add(
                new Dialog_Name(
                    "CustomActions.NewAction".Translate(),
                    name =>
                    {
                        QuerySearchAction searchAction = new QuerySearchAction() { name = name };
                        comp.AddAction(searchAction);
                    },
                    "CustomActions.NameForNewAction".Translate()
                )
            );
        }

        public void PopUpEditor(QuerySearch search)
        {
            var editor = new SearchEditorWindow(search);

            Find.WindowStack.Add(editor);
            editor.windowRect.x = StandardMargin;
            editor.windowRect.y = windowRect.yMin / 3;
            editor.windowRect.yMax = windowRect.yMin;
        }
    }

    public class SearchActionListDrawer
        : SearchGroupDrawerBase<SearchActionGroup, QuerySearchAction>
    {
        CustomActionsGameComp comp = Current.Game.GetComponent<CustomActionsGameComp>();
        ActionsManagerWindow parent;

        public SearchActionListDrawer(SearchActionGroup list, ActionsManagerWindow window)
            : base(list)
        {
            parent = window;
        }

        public override string Name => "TD.ActiveSearches".Translate();

        public new void DrawQuerySearchList(Listing_StandardIndent listing)
        {
            float startHeight = listing.CurHeight;
            float reorderRectHeight = listing.CurHeight;
            const float RowHeight = WidgetRow.IconSize + 6;
            // Reorder Search rect
            if (Event.current.type == EventType.Repaint)
            {
                Rect reorderRect = new Rect(
                    0f,
                    startHeight - Text.LineHeight,
                    listing.ColumnWidth,
                    reorderRectHeight + Text.LineHeight
                );
                reorderID = ReorderableWidget.NewGroup(
                    DoReorderSearch,
                    ReorderableDirection.Vertical,
                    reorderRect,
                    1f,
                    extraDraggedItemOnGUI: (int index, Vector2 dragStartPos) =>
                        DrawMouseAttachedQuerySearch(list[index].Search, listing.ColumnWidth)
                );
            }

            // List of QuerySearches
            for (int i = 0; i < Count; i++)
            {
                DrawPreRow(listing, i);
                var item = list[i];
                QuerySearch search = item.action.filter;
                Rect rowRect = listing.GetRect(RowHeight);

                var row = new WidgetRow(
                    rowRect.x,
                    rowRect.y,
                    UIDirection.RightThenDown,
                    rowRect.width
                );

                // Buttons
                DrawRowButtons(row, item, i);

                DrawExtraRowRect(rowRect, item, i);

                ReorderableWidget.Reorderable(reorderID, rowRect);
            }
            reorderRectHeight = listing.CurHeight - startHeight;

            DrawPostList(listing);
        }

        public override void DrawRowButtons(WidgetRow row, QuerySearchAction searchAction, int i)
        {
            if (row.ButtonIcon(FindTex.Edit, "CustomActions.EditAction".Translate()))
            {
                var subActions = searchAction.action.subActions;
                var Options = subActions
                    .Select(
                        subAction =>
                            new FloatMenuOption(subAction.label, () => subActions.Remove(subAction))
                    )
                    .ToList();

                Options.Add(
                    new FloatMenuOption(
                        "CustomActions.AddSubAction".Translate(),
                        () => Find.WindowStack.Add(new FloatMenu(SubAction.Options(subActions)))
                    )
                );
                Find.WindowStack.Add(new FloatMenu(Options));
            }
            if (row.ButtonIcon(TexButton.Rename))
                comp.RenameAction(searchAction);
            row.Label(searchAction.name + ": ");
            // set count
            Rect textRect = row.GetRect(36);
            textRect.height -= 4;
            textRect.width -= 4;
            string countStr = null;
            Widgets.TextFieldNumeric(textRect, ref searchAction.action.count, ref countStr, 0, 999);
            TooltipHandler.TipRegion(textRect, "CustomActions.Tip0MeansAll".Translate());
            
            row.Label("CustomActions.OF".Translate());

            row.ButtonChooseImportSearch(
                search => searchAction.action.filter = search.CloneForUse()
            );

            if (row.ButtonIcon(FindTex.Edit, "CustomActions.EditFilter".Translate()))
                parent.PopUpEditor(searchAction.action.filter);

            row.Label(searchAction.action.filter.name);
        }

        public override void DrawExtraRowRect(Rect rowRect, QuerySearchAction searchAction, int i)
        {
            WidgetRow row = new WidgetRow(rowRect.xMax, rowRect.y, UIDirection.LeftThenDown);

            if (row.ButtonIcon(FindTex.Trash))
            {
                if (Event.current.shift)
                    comp.RemoveAction(searchAction);
                else
                    Find.WindowStack.Add(
                        Dialog_MessageBox.CreateConfirmation(
                            "TD.Delete0".Translate(searchAction.Search.name),
                            () => comp.RemoveAction(searchAction),
                            true
                        )
                    );
            }

            //Check off
            row.Checkbox(ref searchAction.enabled);

            // actionInterval
            Rect textRect = row.GetRect(60);
            textRect.height -= 4;
            textRect.width -= 4;
            string searchIntervalStr = null;
            Widgets.TextFieldNumeric(
                textRect,
                ref searchAction.searchInterval,
                ref searchIntervalStr,
                1,
                999999
            );
            TooltipHandler.TipRegion(
                textRect,
                "CustomActions.Tip1000SecondsInARimworldDay".Translate()
            );
            row.Label("CustomActions.SearchInterval".Translate() + " ");

            // show when
            textRect = row.GetRect(60);
            textRect.height -= 4;
            textRect.width -= 4;
            string countToActionStr = null;
            Widgets.TextFieldNumeric(
                textRect,
                ref searchAction.countToAction,
                ref countToActionStr,
                0,
                999999
            );

            if (row.ButtonText(SymbolFor(searchAction.compareType)))
                searchAction.compareType = (CompareType)((int)(searchAction.compareType + 1) % 3);

            row.Label(searchAction.Search.name);

            if (row.ButtonIcon(FindTex.Edit, "CustomActions.EditSearch".Translate()))
                parent.PopUpEditor(searchAction.Search);

            row.ButtonChooseImportSearch(
                search => searchAction.search = search.CloneForUse()
            );

            row.Label("CustomActions.DoWhen".Translate());
        }

        public static string SymbolFor(CompareType compareType) =>
            compareType == CompareType.Equal
                ? "="
                : compareType == CompareType.Greater
                    ? ">"
                    : "<";
    }

    public class MainButtonWorker_ToggleActionsWindow : MainButtonWorker
    {
        public static void OpenWith(SearchGroup searches)
        {
            Open();

            Current.Game.GetComponent<CustomActionsGameComp>().AddActions(searches);
        }

        public static void OpenWith(QuerySearch search)
        {
            Open();

            Current.Game.GetComponent<CustomActionsGameComp>().AddAction(search);
        }

        public static ActionsManagerWindow Open()
        {
            if (Find.WindowStack.WindowOfType<ActionsManagerWindow>() is ActionsManagerWindow w)
            {
                Find.WindowStack.Notify_ClickedInsideWindow(w);
                return w;
            }
            else
            {
                ActionsManagerWindow window = new ActionsManagerWindow();
                Find.WindowStack.Add(window);
                return window;
            }
        }

        public static void Toggle()
        {
            if (Find.WindowStack.WindowOfType<ActionsManagerWindow>() is ActionsManagerWindow w)
                w.Close();
            else
                Open();
        }

        public override void Activate()
        {
            Toggle();
        }
    }

    [StaticConstructorOnStartup]
    public class SearchActionTransfer : ISearchReceiver, ISearchGroupReceiver
    {
        static SearchActionTransfer()
        {
            SearchTransfer.Register(new SearchActionTransfer());
        }

        public static string TransferTag = "Custom Action";
        public string Source => TransferTag;
        public string ReceiveName => "CustomActions.MakeCustomAction".Translate();
        public QuerySearch.CloneArgs CloneArgs => QuerySearch.CloneArgs.use;

        public bool CanReceive() => Current.Game?.GetComponent<CustomActionsGameComp>() != null;

        public void Receive(QuerySearch search) =>
            MainButtonWorker_ToggleActionsWindow.OpenWith(search);

        public void Receive(SearchGroup searches) =>
            MainButtonWorker_ToggleActionsWindow.OpenWith(searches);
    }
}
