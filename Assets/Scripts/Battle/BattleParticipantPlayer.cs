using System;
using System.Linq;
using UnityEngine;
using Battle;
using Pokemon;
using Battle.PlayerUI;

namespace Battle
{
    public class BattleParticipantPlayer : BattleParticipant
    {

        public PlayerBattleUIController playerBattleUIController;
        public PlayerPokemonSelectUIController playerPokemonSelectUIController;

        public override PokemonInstance[] GetPokemon()
        {
            return PlayerData.singleton.partyPokemon;
        }

        public override void StartChoosingAction(BattleData battleData)
        {
            actionHasBeenChosen = false;
            OpenBattleUIRoot();
        }

        public override void StartChoosingNextPokemon()
        {
            StartPokemonSelectUI(PokemonSelectUIPurpose.ReplacingPokemon,
                (x) => x.health > 0,
                false);
        }

        public override bool CheckIfDefeated()
        {
            return GetPokemon().All((x) => x.health <= 0);
        }

        #region Player Battle UI

        private void OpenBattleUIRoot()
        {
            playerBattleUIController.OpenRootMenu();
        }

        private void HideBattleUI()
        {
            playerBattleUIController.DisableAllMenus();
        }

        /// <summary>
        /// Choose to attempt to flee the battle
        /// </summary>
        public void ChooseActionFlee()
        {

            chosenAction = new Action
            {
                type = Action.Type.Flee
            };

            actionHasBeenChosen = true;

            HideBattleUI();

        }

        //TODO - function for choosinng to use a specified item. Open pokemon choosing UI (using StartPokemonSelectUI), hide battle UI

        /// <summary>
        /// Choose to fight using the move with index moveIndex in the active pokemon's moves
        /// </summary>
        /// <param name="partyIndex">The indexx of the move in the active pokemon's moves</param>
        public void ChooseActionFight(int moveIndex)
        {

            if (moveIndex < 0 || moveIndex > 3)
            {
                Debug.LogError("Invalid move index sent for choosing to fight - " + moveIndex);
                return;
            }

            chosenAction = new Action
            {
                type = Action.Type.Fight,
                fightMoveIndex = moveIndex
            };

            actionHasBeenChosen = true;

            HideBattleUI();

        }

        /// <summary>
        /// Choose to switch pokemon to the pokemon with index partyIndex in the player's party
        /// </summary>
        /// <param name="partyIndex">The index of the pokemon to switch to in the player's party</param>
        public void ChooseActionSwitchPokemon(int partyIndex)
        {

            if (partyIndex < 0 || partyIndex > 5)
            {
                Debug.LogError("Invalid party index sent for choosing to switch pokemon - " + partyIndex);
                return;
            }

            chosenAction = new Action
            {
                type = Action.Type.SwitchPokemon,
                switchPokemonIndex = partyIndex
            };

            actionHasBeenChosen = true;

            HideBattleUI();

        }

        #endregion

        #region Player Pokemon Selection UI

        //TODO - find way to react to back button on pokemon select UI depending on pokemonSelectUIPurpose. If item, should go back to correct item category menu

        private enum PokemonSelectUIPurpose
        {
            ReplacingPokemon,
            ItemTarget
        }

        private PokemonSelectUIPurpose pokemonSelectUIPurpose;
        private Predicate<PokemonInstance> pokemonSelectUIValidityPredicate;

        /// <summary>
        /// Start the pokemon selection UI and specify settings for starting it
        /// </summary>
        /// <param name="purpose">The purpose for which the UI is being opened</param>
        /// <param name="validityCheck">A predicate to check if a chosen pokemon is a valid target for selection (eg. a pokemon can't be selected as the next pokemon if it has no health)</param>
        /// <param name="showBackButton">Whether to show the back button (eg. when selecting a next pokemon, the player shouldn't be allowed to go back)</param>
        private void StartPokemonSelectUI(PokemonSelectUIPurpose purpose,
            Predicate<PokemonInstance> validityCheck,
            bool showBackButton = true)
        {

            pokemonSelectUIPurpose = purpose;
            pokemonSelectUIValidityPredicate = validityCheck;
            ShowPokemonSelectUI(showBackButton);

        }

        private void ShowPokemonSelectUI(bool showBackButton = true)
        {

            playerPokemonSelectUIController.Show();

            if (showBackButton)
                playerPokemonSelectUIController.ShowBackButton();
            else
                playerPokemonSelectUIController.HideBackButton();

        }

        private void HidePokemonSelectUI()
        {
            playerPokemonSelectUIController.Hide();
        }

        private bool PokemonSelectUICheckValidity(PokemonInstance pokemon)
        {

            if (pokemonSelectUIValidityPredicate != null)
                return pokemonSelectUIValidityPredicate(pokemon);
            else
            {
                Debug.LogError("Pokemon select UI validity predicate unset");
                return true;
            }

        }

        public void PokemonSelectUISelectPokemon(int partyIndex)
        {

            if (partyIndex < 0 || partyIndex > 5)
            {
                Debug.LogError("Party index out of range - " + partyIndex);
                return;
            }

            PokemonInstance selectedPokemon = PlayerData.singleton.partyPokemon[partyIndex];

            if (pokemonSelectUIPurpose == PokemonSelectUIPurpose.ReplacingPokemon)
            {

                if (!PokemonSelectUICheckValidity(selectedPokemon))
                {
                    PokemonSelectDisplayInvalidMessage();
                    return;
                }

                nextPokemonHasBeenChosen = true;
                chosenNextPokemonIndex = partyIndex;

                HidePokemonSelectUI();

            }
            else if (pokemonSelectUIPurpose == PokemonSelectUIPurpose.ItemTarget)
            {

                if (!PokemonSelectUICheckValidity(selectedPokemon))
                {
                    PokemonSelectDisplayInvalidMessage();
                    return;
                }

                actionHasBeenChosen = true;
                chosenAction = new Action
                {
                    //TODO - once item action ready, choose to use item on selected pokemon
                };

                HidePokemonSelectUI();

            }
            else
            {
                Debug.LogError("Unknown pokemon select UI purpose - " + pokemonSelectUIPurpose);
            }

        }

        private void PokemonSelectDisplayInvalidMessage()
        {
            //TODO - display message saying that selected pokemon isn't valid
        }

        #endregion

    }
}