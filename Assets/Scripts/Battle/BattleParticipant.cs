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

        public virtual void StartChoosingAction(BattleData battleData)
        {

            actionHasBeenChosen = false;

            //Forced to recharge
            if (ActivePokemon.battleProperties.volatileBattleStatus.rechargingStage > 0)
            {

                chosenAction = new Action(this)
                {
                    type = Action.Type.Recharge
                };
                actionHasBeenChosen = true;

                return;

            }

            //Force to continue thrashing
            if (ActivePokemon.battleProperties.volatileBattleStatus.thrashTurns > 0)
            {

                ChooseActionFight(
                    battleData,
                    false,
                    ActivePokemon.GetMoveIndexById(ActivePokemon.battleProperties.volatileBattleStatus.thrashMoveId)
                );

                return;

            }

            //Force to use the move that has charged for this turn
            if (ActivePokemon.battleProperties.IsUsingChargedMove)
            {

                ChooseActionFight(
                    battleData,
                    false,
                    ActivePokemon.GetMoveIndexById(ActivePokemon.battleProperties.volatileBattleStatus.chargingMoveId)
                );

                return;

            }

        }

        /// <summary>
        /// Forcefully set a participant's chosen action. Made for cheat commands
        /// </summary>
        public virtual void ForceSetChosenAction(Action action)
        {
            SetChosenAction(action);
        }

        protected virtual void SetChosenAction(Action action)
        {
            chosenAction = action;
            actionHasBeenChosen = true;
        }

        /// <summary>
        /// Set the participant's action to fighting with the specified move or struggle
        /// </summary>
        public abstract void ChooseActionFight(BattleData battleData, bool useStruggle, int moveIndex);

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

        public virtual bool GetAllowedToFlee()
            => !(ActivePokemon.battleProperties.volatileStatusConditions.bound > 0)
            && !ActivePokemon.battleProperties.volatileStatusConditions.cantEscape;

        public virtual bool GetAllowedToSwitchPokemon()
            => !(ActivePokemon.battleProperties.volatileStatusConditions.bound > 0)
                && !ActivePokemon.battleProperties.volatileStatusConditions.cantEscape;

        public virtual bool GetAllowedToUseItem()
            => ActivePokemon.battleProperties.volatileStatusConditions.embargo <= 0;

    }
}
