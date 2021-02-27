using System.Collections;
using System.Collections.Generic;
using Battle;

namespace Battle
{
    public abstract partial class BattleParticipant
    {

        public BattleManager battleManager;

        /// <summary>
        /// Whether the action has been chosen
        /// </summary>
        public bool actionHasBeenChosen = false;

        /// <summary>
        /// The action that has been chosen. This is only valid if actionHasBeenChosen is true
        /// </summary>
        public Action chosenAction;

        public abstract void StartChoosingAction();

    }
}
