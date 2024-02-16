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
        CustomActionsGameComp comp;
        public override Vector2 InitialSize => new Vector2(850, 500f);

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            windowRect.x = StandardMargin;
            windowRect.y = UI.screenHeight - MainButtonDef.ButtonHeight - windowRect.height - StandardMargin;
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
        }

        private Vector2 scrollPosition = Vector2.zero;
        private float scrollViewHeight;
        private string title = "CustomActions.CustomActions".Translate();

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = inRect.TopPartPixels(Text.LineHeight).AtZero();
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;

            var addRect = inRect.BottomPartPixels(Text.LineHeight);
            var addRow = new WidgetRow(addRect.x, addRect.y);
            if (addRow.ButtonIcon(FindTex.GreyPlus))
                PopUpCreateAction();

            inRect.yMin = titleRect.yMax + Listing.DefaultGap;
            inRect.yMax -= Listing.DefaultGap;
            var ls = new Listing_StandardIndent();
            var width = inRect.width - 16f;
            ls.BeginScrollView(inRect, ref scrollPosition, new Rect(0f, 0f, width, scrollViewHeight));
            var actionToRemove = new List<QueryAction>();
            for (var i = 0; i < comp.actions.Count(); ++i)
            {
                var action = comp.actions[i];
                var layer = action.folded ? 1 : 4;
                var rowRect = ls.GetRect(28f * layer - 4f);
                if (Mouse.IsOver(rowRect))
                    GUI.DrawTexture(rowRect, TexUI.HighlightTex);
                var row = new WidgetRow(rowRect.x, rowRect.y, UIDirection.RightThenDown, ls.ColumnWidth);
                if (row.ButtonIcon(FindTex.Trash))
                    actionToRemove.Add(action);
                if (row.ButtonIcon(TexButton.Rename))
                    Find.WindowStack.Add(new Dialog_Name(action.label, name => action.label = name));
                row.Label(action.label);
                var rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                if (rowRight.ButtonIcon(action.enabled ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex))
                    action.enabled = !action.enabled;
                if (action.folded)
                {
                    if (action.searchInterval > 1000)
                        row.Label("EveryDays".Translate(action.searchInterval / 1000));
                    else
                        row.Label("TimesPerDay".Translate(1000 / action.searchInterval));
                    row.Label("CustomActions.SummaryTrigger".Translate(action.trigger.Name, QueryAction.ToString(action.compareType), action.countToTrigger));
                    rowRight.Label(
                        action.count == 0 ? "CustomActions.SummaryAll".Translate(action.filter.Name) : "CustomActions.Summary".Translate(action.count, action.filter.Name)
                    );
                }
                else
                {
                    row.Gap(ls.ColumnWidth + 1f);
                    row.Label("CustomActions.Filter".Translate());
                    row.ButtonChooseImportSearch(search => action.filter = search.CloneForUse());
                    if (row.ButtonIcon(FindTex.Edit))
                        PopUpEditor(action.filter);
                    row.Label(action.filter.Name);

                    rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);

                    var textRect = rowRight.GetRect(36);
                    textRect.height -= 4f;
                    textRect.width -= 4f;
                    string countStr = null;
                    Widgets.TextFieldNumeric(textRect, ref action.count, ref countStr, 0, 999);
                    rowRight.Label("CustomActions.Count".Translate());
                    row.Gap(ls.ColumnWidth + 1f);
                    row.Label("CustomActions.Trigger".Translate());
                    row.ButtonChooseImportSearch(search => action.trigger = search.CloneForUse());
                    if (row.ButtonIcon(FindTex.Edit))
                        PopUpEditor(action.trigger);
                    row.Label(action.trigger.Name);
                    if (row.ButtonText(QueryAction.ToString(action.compareType)))
                        action.compareType = (QueryAction.CompareType)(((int)action.compareType + 1) % 3);
                    textRect = row.GetRect(60);
                    textRect.height -= 4f;
                    textRect.width -= 4f;
                    string countToTriggerStr = null;
                    Widgets.TextFieldNumeric(textRect, ref action.countToTrigger, ref countToTriggerStr, 0, 999999);

                    rowRight = new WidgetRow(ls.ColumnWidth, row.FinalY, UIDirection.LeftThenDown);
                    textRect = rowRight.GetRect(60);
                    textRect.height -= 4f;
                    textRect.width -= 4f;
                    string searchIntervalStr = null;
                    Widgets.TextFieldNumeric(textRect, ref action.searchInterval, ref searchIntervalStr, 1, 999999);
                    rowRight.Label("CustomActions.SearchInterval".Translate());

                    row.Gap(ls.ColumnWidth + 1f);
                    row.Label("CustomActions.SubActions".Translate());
                    if (row.ButtonIcon(FindTex.Edit))
                        Find.WindowStack.Add(
                            new FloatMenu(
                                action
                                    .subactions.Select(subaction => new FloatMenuOption(subaction.label, () => action.subactions.Remove(subaction)))
                                    .Append(
                                        new FloatMenuOption(
                                            "CustomActions.AddSubAction".Translate(),
                                            () => Find.WindowStack.Add(new FloatMenu(SubAction.Options(action.subactions).ToList()))
                                        )
                                    )
                                    .ToList()
                            )
                        );
                    var count = action.subactions.Count();
                    if (count == 0)
                        row.Label("CustomActions.NoSubActions".Translate().Colorize(Color.red));
                    else
                    {
                        var firstSubaction = action.subactions[0];
                        if (count == 1)
                            row.Label("CustomActions.ListSubAction".Translate(firstSubaction.label));
                        else
                            row.Label("CustomActions.ListSubActions".Translate(firstSubaction.label, count - 1));
                    }
                }
                if (Widgets.ButtonInvisible(rowRect))
                {
                    if (Event.current.shift && i > 0)
                        comp.actions.Reverse(i - 1, 2);
                    else
                        action.folded = !action.folded;
                }
            }
            actionToRemove.ForEach(action => comp.actions.Remove(action));
            ls.EndScrollView(ref scrollViewHeight);
        }

        public void PopUpCreateAction()
        {
            Find.WindowStack.Add(
                new Dialog_Name("CustomActions.NewAction".Translate(), name => comp.actions.Add(new QueryAction() { label = name }), "CustomActions.NameForNewAction".Translate())
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

    public class MainButtonWorker_ToggleActionsWindow : MainButtonWorker
    {
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

        public override void Activate()
        {
            if (Find.WindowStack.WindowOfType<ActionsManagerWindow>() is ActionsManagerWindow w)
                w.Close();
            else
                Open();
        }
    }
}
