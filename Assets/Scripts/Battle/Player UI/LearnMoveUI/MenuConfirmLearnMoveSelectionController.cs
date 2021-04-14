using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Pokemon.Moves;
using Menus;

namespace Battle.PlayerUI.LearnMoveUI
{
    public class MenuConfirmLearnMoveSelectionController : MenuController
    {

        public Button yesButton;
        public Button noButton;

        protected override MenuSelectableController[] GetSelectables() => new MenuSelectableController[] { yesButton.GetComponent<MenuSelectableController>(), noButton.GetComponent<MenuSelectableController>() };

        public LearnMoveUIController learnMoveUIController;

        #region Constants

        /// <summary>
        /// The prompt to show when confirming whether the player wants to replace a move. {pokemonName} is for the pokemon's name. {oldMoveName} is for the name of the move to maybe forget. {newMoveName} is for the name of the move to replace with
        /// </summary>
        public const string replaceMovePrompt = "Are you sure you want {pokemonName} to forget {oldMoveName} and instead learn {newMoveName}?";

        /// <summary>
        /// The prompt to show when confirming whether the player doesn't want to learn a move. {pokemonName} is for the pokemon's name. {moveName} is for the name of the move to maybe forget
        /// </summary>
        public const string cancelLearnMovePrompt = "Are you sure you don't want {pokemonName} to learn {moveName}?";

        #endregion

        private TextBoxController textBoxController;

        private void Start()
        {
            if (textBoxController == null)
                SetTextBoxController();
        }

        private void SetTextBoxController()
        {

            TextBoxController[] textBoxControllerCandidates = FindObjectsOfType<TextBoxController>()
                .Where(x => x.gameObject.scene == gameObject.scene)
                .ToArray();

            if (textBoxControllerCandidates.Length == 0)
                Debug.LogError("No valid TextBoxController found");
            else
                textBoxController = textBoxControllerCandidates[0];

        }

        /// <summary>
        /// Sets the details of the menu including actions to take when each button is used
        /// </summary>
        /// <param name="pokemon">The pokmeon to use for consideration</param>
        /// <param name="replaceMove">Whether the menu is being used for replacing a move (instead of for cancelling learning the move)</param>
        /// <param name="replacedMoveIndex">(Only relevant if replaceMove is true) The index of the move that is being considered for replacement</param>
        public void SetDetails(PokemonInstance pokemon,
            int newMoveId,
            bool replaceMove,
            int replacedMoveIndex = -1)
        {

            if (textBoxController == null)
                SetTextBoxController();

            PokemonMove newMove = PokemonMove.GetPokemonMoveById(newMoveId);

            noButton.onClick.AddListener(() => learnMoveUIController.ConfirmAction_ReturnToLearnMoveMenu());

            if (replaceMove)
            {

                if (replacedMoveIndex < 0 || replacedMoveIndex > 3)
                    Debug.LogError("replacedMoveIndex was invalid when setting details for replacing a move");

                string prompt = replaceMovePrompt
                    .Replace("{pokemonName}", pokemon.GetDisplayName())
                    .Replace("{oldMoveName}", PokemonMove.GetPokemonMoveById(pokemon.moveIds[replacedMoveIndex]).name)
                    .Replace("{newMoveName}", newMove.name);

                textBoxController.SetTextInstant(prompt);

                yesButton.onClick.AddListener(() => learnMoveUIController.ConfirmAction_ReplaceMove(replacedMoveIndex));

            }
            else
            {

                string prompt = cancelLearnMovePrompt
                    .Replace("{pokemonName}", pokemon.GetDisplayName())
                    .Replace("{moveName}", newMove.name);

                textBoxController.SetTextInstant(prompt);

                yesButton.onClick.AddListener(() => learnMoveUIController.ConfirmAction_CancelLearningMove());

            }

        }

    }
}
