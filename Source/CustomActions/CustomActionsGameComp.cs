using System.Collections.Generic;
using Verse;

namespace CustomActions
{
    class CustomActionsGameComp : GameComponent
    {
        public List<QueryAction> actions = new List<QueryAction>();

        public CustomActionsGameComp(Game g)
            : base() { }

        public override void ExposeData() => Scribe_Collections.Look(ref actions, "actions");

        public override void GameComponentTick() => actions.ForEach(action => action.DoAction());
    }
}
