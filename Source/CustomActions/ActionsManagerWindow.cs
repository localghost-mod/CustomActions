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
            windowRect.x = Window.StandardMargin;
            windowRect.y =
                UI.screenHeight
                - MainButtonDef.ButtonHeight
                - this.windowRect.height
                - Window.StandardMargin;
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

            addRow.ButtonChooseImportSearch(
                comp.AddAction,
                SearchActionTransfer.TransferTag,
                QuerySearch.CloneArgs.use
            );

            addRow.ButtonChooseImportSearchGroup(
                comp.AddActions,
                SearchActionTransfer.TransferTag,
                QuerySearch.CloneArgs.use
            );

            addRow.ButtonChooseExportSearchGroup(
                comp.actions.AsSearchGroup(),
                SearchActionTransfer.TransferTag
            );

            addRow.ButtonOpenLibrary();

            //Check off
            Rect enableRect = inRect.RightHalf().BottomPartPixels(Text.LineHeight);
            Widgets.CheckboxLabeled(
                enableRect,
                "CustomActions.EnableActions".Translate(),
                ref Action_QuerySearch.enableAll
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
                    n =>
                    {
                        QuerySearch search = new QuerySearch() { name = n };
                        search.SetSearchAllMaps();
                        QuerySearchAction searchAction = new QuerySearchAction(search);
                        comp.AddAction(searchAction);

                        PopUpEditor(searchAction);
                    },
                    "CustomActions.NameForNewAction".Translate(),
                    name => comp.HasSavedAction(name)
                )
            );
        }

        public void PopUpEditor(QuerySearchAction searchAction)
        {
            var editor = new SearchEditorWindow(searchAction);

            Find.WindowStack.Add(editor);
            editor.windowRect.x = Window.StandardMargin;
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

        public override void DrawRowButtons(WidgetRow row, QuerySearchAction searchAction, int i)
        {
            if (row.ButtonIcon(FindTex.Edit, "TD.EditThisSearch".Translate()))
                parent.PopUpEditor(searchAction);

            if (row.ButtonIcon(TexButton.Rename))
                comp.RenameAction(searchAction);

            if (row.ButtonIcon(FindTex.Trash))
            {
                if (Event.current.shift)
                    comp.RemoveAction(searchAction);
                else
                    Find.WindowStack.Add(
                        Dialog_MessageBox.CreateConfirmation(
                            "TD.Delete0".Translate(searchAction.search.name),
                            () => comp.RemoveAction(searchAction),
                            true
                        )
                    );
            }

            SearchStorage.ButtonChooseExportSearch(
                row,
                searchAction.search,
                SearchActionTransfer.TransferTag
            );
        }

        public override void DrawExtraRowRect(Rect rowRect, QuerySearchAction searchAction, int i)
        {
            WidgetRow row = new WidgetRow(rowRect.xMax, rowRect.y, UIDirection.LeftThenDown);

            //Check off
            row.Checkbox(ref searchAction.action.enabled);

            //Show when (backwards right to left O_o)
            Rect textRect = row.GetRect(60);
            textRect.height -= 4;
            textRect.width -= 4;
            string dummyStr = null;
            Widgets.TextFieldNumeric(
                textRect,
                ref searchAction.action.countToAction,
                ref dummyStr,
                0,
                999999
            );
            if (row.ButtonText(SymbolFor(searchAction.action.compareType)))
                searchAction.action.compareType = (CompareType)(
                    (int)(searchAction.action.compareType + 1) % 3
                );
            row.Label("CustomActions.DoWhen".Translate());

            //actionInterval
            textRect = row.GetRect(60);
            textRect.height -= 4;
            textRect.width -= 4;
            dummyStr = null;
            Widgets.TextFieldNumeric(
                textRect,
                ref searchAction.action.actionInterval,
                ref dummyStr,
                1,
                999999
            );
            TooltipHandler.TipRegion(
                textRect,
                "CustomActions.Tip1000SecondsInARimworldDay".Translate()
            );
            row.Label("CustomActions.ActionInterval".Translate());

            //edit subaction
            if (row.ButtonText("CustomActions.EditSubActions".Translate()))
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
                        () => Find.WindowStack.Add(new FloatMenu(SubActionOptions(searchAction)))
                    )
                );
                Find.WindowStack.Add(new FloatMenu(Options));
            }
        }

        public List<FloatMenuOption> SubActionOptions(QuerySearchAction searchAction)
        {
            var subActions = searchAction.action.subActions;
            var MedicalRecipeOption = MedicalRecipesUtility
                .recipesWithPart.Select(
                    recipeWithPart =>
                        new FloatMenuOption(
                            MedicalRecipesUtility.ToString(recipeWithPart),
                            () =>
                                subActions.Add(
                                    new SubAction(
                                        "CustomActions.MedicalRecipesUtility",
                                        "MedicalRecipe",
                                        new List<string>()
                                        {
                                            recipeWithPart.Item1.defName,
                                            recipeWithPart.Item2?.Label
                                        },
                                        MedicalRecipesUtility.ToString(recipeWithPart)
                                    )
                                )
                        )
                )
                .ToList();
            var designatorNames = new List<string>
            {
                "Harvest",
                "Mine",
                "Deconstruct",
                "HaulThings",
                "Tame",
                "Hunt"
            };
            var _result = designatorNames
                .Select(
                    designatorName =>
                        new FloatMenuOption(
                            ("Designator" + designatorName).Translate(),
                            () =>
                                subActions.Add(
                                    new SubAction(
                                        "CustomActions.DesignatorUtility",
                                        designatorName,
                                        new List<string>(),
                                        ("Designator" + designatorName).Translate()
                                    )
                                )
                        )
                )
                .ToList();
            _result = _result.Concat(
                new List<FloatMenuOption>
                {
                    new FloatMenuOption(
                        "CustomActions.ClearBillStack".Translate(),
                        () =>
                            subActions.Add(
                                new SubAction(
                                    "CustomActions.MedicalRecipesUtility",
                                    "ClearBillStack",
                                    new List<string>(),
                                    "CustomActions.ClearBillStack".Translate()
                                )
                            )
                    ),
                    new FloatMenuOption(
                        "MedicalOperations".Translate(),
                        () => Find.WindowStack.Add(new FloatMenu(MedicalRecipeOption))
                    )
                }
            ).ToList();
            return _result;
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
