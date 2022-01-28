using System.Collections;
using System.Collections.Generic;
using Battle;

namespace Battle
{
    public abstract partial class BattleParticipant
    {

        public BattleManager battleManager;

        public abstract string GetName();

        #region Choose Action

        /// <summary>
        /// Whether the action has been chosen
        /// </summary>
        protected bool actionHasBeenChosen = false;

        public virtual bool GetActionHasBeenChosen() => actionHasBeenChosen;

        /// <summary>
        /// The action that has been chosen. This is only valid if actionHasBeenChosen is true
        /// </summary>
        protected Action chosenAction;

        public virtual Action GetChosenAction() => chosenAction;

        public abstract void StartChoosingAction(BattleData battleData);

        /// <summary>
        /// Forcefully set a participant's chosen action. Made for cheat commands
        /// </summary>
        public virtual void ForceSetChosenAction(Action action)
        {
            actionHasBeenChosen = true;
            chosenAction = action;
        }

        #endregion

        #region Pokemon

        /// <summary>
        /// The index of the participant's active pokemon in their party
        /// </summary>
        public int activePokemonIndex;
        public Pokemon.PokemonInstance ActivePokemon
        {
            get
            {
                return GetPokemon()[activePokemonIndex];
            }
        }

        public abstract Pokemon.PokemonInstance[] GetPokemon();

        #endregion

        #region Next Pokemon

        /// <summary>
        /// Whether the next pokemon has been chosen
        /// </summary>
        protected bool nextPokemonHasBeenChosen = false;

        public virtual bool GetNextPokemonHasBeenChosen() => nextPokemonHasBeenChosen;

        /// <summary>
        /// Index of pokemon chosen to replace current pokemon. This is only valid if nextPokemonHasBeenChosen is true
        /// </summary>
        protected int chosenNextPokemonIndex;

        public virtual int GetChosenNextPokemonIndex() => chosenNextPokemonIndex;

        public abstract void StartChoosingNextPokemon();

        #endregion

        /// <summary>
        /// Check if the participant is defeated. This should always be implemented by checking whether all the participant's pokemon have fainted
        /// </summary>
        /// <returns>Whether the participant has been defeated</returns>
        public abstract bool CheckIfDefeated();

    }
}
