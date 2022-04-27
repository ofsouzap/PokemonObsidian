using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pokemon;
using Pokemon.Moves;

namespace Battle.PlayerUI.LearnMoveUI
{
    public class LearnMoveUIController : MonoBehaviour
    {

        public BattleAnimationSequencer battleAnimationSequencer;

        public MenuLearnMoveController menuLearnMoveController;
        public MenuConfirmLearnMoveSelectionController menuConfirmLearnMoveSelectionController;

        private TextBoxController textBoxController;

        /// <summary>
        /// If the UI is still being used and the player hasn't yet selected how they will continue
        /// </summary>
        public bool uiRunning { get; protected set; }

        #region UI Player Decision

        /// <summary>
        /// Details for what the player has chosen to do in regards to learning the new move
        /// </summary>
        public struct UIPlayerDecision
        {

            /// <summary>
            /// Whether the player decided to learn the move
            /// </summary>
            public bool learnMove;

            /// <summary>
            /// If the player decided to learn the move, this will be the index of the move that they decided to replace in the pokemon
            /// </summary>
            public int replacedMoveIndex;

        }

        /// <summary>
        /// Once the player has made their decision, uiRunning will be false and this will hold the details of their choice
        /// </summary>
        public UIPlayerDecision uiPlayerDecision;

        #endregion

        private PokemonInstance selectedPokemon;
        private int selectedMoveId;
        private PokemonMove SelectedMove => PokemonMove.GetPokemonMoveById(selectedMoveId);

        private void Start()
        {

            TextBoxController[] textBoxControllerCandidates = FindObjectsOfType<TextBoxController>()
                    .Where(x => x.gameObject.scene == gameObject.scene)
                    .ToArray();

            if (textBoxControllerCandidates.Length == 0)
                Debug.LogError("No valid TextBoxController found");
            else
                textBoxController = textBoxControllerCandidates[0];

        }

        public void StartUI(PokemonInstance pokemon, int newMoveId)
        {

            uiRunning = true;

            selectedPokemon = pokemon;
            selectedMoveId = newMoveId;

            menuConfirmLearnMoveSelectionController.learnMoveUIController = this;

            #region Learn Move Menu

            menuLearnMoveController.SetNewMoveId(selectedMoveId);
            menuLearnMoveController.SetSelectedPokemon(pokemon);

            menuLearnMoveController.newMoveButton.onClick.AddListener(() => OpenConfirmMenu(false));

            menuLearnMoveController.moveButtons[0].onClick.AddListener(() => OpenConfirmMenu(true, 0));
            menuLearnMoveController.moveButtons[1].onClick.AddListener(() => OpenConfirmMenu(true, 1));
            menuLearnMoveController.moveButtons[2].onClick.AddListener(() => OpenConfirmMenu(true, 2));
            menuLearnMoveController.moveButtons[3].onClick.AddListener(() => OpenConfirmMenu(true, 3));

            menuLearnMoveController.RefreshMoveButtons();

            #endregion

            HideConfirmLearnMoveMenu();
            ShowLearnMoveMenu();

        }

        private void ShowLearnMoveMenu()
        {

            menuLearnMoveController.Show();

            textBoxController.SetTextInstant("Which move do you not want?");

            if (!menuLearnMoveController.selectionListenersSetUp)
                menuLearnMoveController.SetUpSelectionListeners();

            EventSystem.current.SetSelectedGameObject(menuLearnMoveController.newMoveButton.gameObject);


        }
        private void HideLearnMoveMenu() => menuLearnMoveController.Hide();
        private void ShowConfirmLearnMoveMenu() => menuConfirmLearnMoveSelectionController.Show();
        private void HideConfirmLearnMoveMenu() => menuConfirmLearnMoveSelectionController.Hide();

        private void OpenConfirmMenu(bool replaceMove,
            int replacedMoveIndex = -1)
        {

            menuConfirmLearnMoveSelectionController.SetDetails(selectedPokemon,
                selectedMoveId,
                replaceMove,
                replacedMoveIndex);

            HideLearnMoveMenu();
            ShowConfirmLearnMoveMenu();

            EventSystem.current.SetSelectedGameObject(menuConfirmLearnMoveSelectionController.noButton.gameObject);

        }

        #region Confirm Actions

        public void ConfirmAction_ReturnToLearnMoveMenu()
        {
            ShowLearnMoveMenu();
            HideConfirmLearnMoveMenu();
        }

        public void ConfirmAction_ReplaceMove(int replacedMoveIndex)
        {

            uiRunning = false;
            uiPlayerDecision = new UIPlayerDecision()
            {
                learnMove = true,
                replacedMoveIndex = replacedMoveIndex
            };

            HideLearnMoveMenu();
            HideConfirmLearnMoveMenu();

        }

        public void ConfirmAction_CancelLearningMove()
        {

            uiRunning = false;
            uiPlayerDecision = new UIPlayerDecision()
            {
                learnMove = false
            };

            HideLearnMoveMenu();
            HideConfirmLearnMoveMenu();

        }

        #endregion

    }
}
