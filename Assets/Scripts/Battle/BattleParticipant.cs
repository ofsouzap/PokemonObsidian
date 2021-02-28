using System.Collections;
using System.Collections.Generic;
using Battle;

namespace Battle
{
    public abstract partial class BattleParticipant
    {

        public BattleManager battleManager;

        #region Choose Action

        /// <summary>
        /// Whether the action has been chosen
        /// </summary>
        public bool actionHasBeenChosen = false;

        /// <summary>
        /// The action that has been chosen. This is only valid if actionHasBeenChosen is true
        /// </summary>
        public Action chosenAction;

        public abstract void StartChoosingAction(BattleData battleData);

        #endregion

        #region Pokemon

        /// <summary>
        /// The index of the participant's active pokemon in their party
        /// </summary>
        public int activePokemonIndex;

        public abstract Pokemon.PokemonInstance[] GetPokemon();

        #endregion

        #region Next Pokemon

        /// <summary>
        /// Whether the next pokemon has been chosen
        /// </summary>
        public bool nextPokemonHasBeenChosen = false;

        /// <summary>
        /// Index of pokemon chosen to replace current pokemon. This is only valid if nextPokemonHasBeenChosen is true
        /// </summary>
        public int chosenNextPokemonIndex;

        public abstract void StartChoosingNextPokemon();

        #endregion

        /// <summary>
        /// Check if the participant is defeated. This should always be implemented by checking whether all the participant's pokemon have fainted
        /// </summary>
        /// <returns>Whether the participant has been defeated</returns>
        public abstract bool CheckIfDefeated();

    }
}
