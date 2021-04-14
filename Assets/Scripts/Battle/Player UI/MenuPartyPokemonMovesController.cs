using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pokemon.Moves;
using Battle.PlayerUI;
using Menus;

namespace Battle.PlayerUI
{
    public class MenuPartyPokemonMovesController : MenuController
    {

        public Button buttonBack;

        public Button[] moveButtons;

        protected override MenuSelectableController[] GetSelectables()
        {
            MenuSelectableController[] output = new MenuSelectableController[moveButtons.Length + 1];
            Array.Copy(
                moveButtons.Select(x => x.GetComponent<MenuSelectableController>()).ToArray(),
                output,
                moveButtons.Length);
            output[output.Length - 1] = buttonBack.GetComponent<MenuSelectableController>();
            return output;
        }

        public Text textName;
        public Text textPPValue;
        public Text textDescription;
        public Text textPowerValue;
        public Text textAccuracyValue;

        public Image imageCategory;
        public Image imageType;

        public GameObject gameObjectMoveDetailsPane;

        public void SetUp()
        {

            if (moveButtons.Length != 4)
            {
                Debug.LogError("Number of move buttons not 4");
            }

            foreach (Button button in moveButtons)
            {

                if (button.GetComponent<MenuButtonMoveController>() == null)
                {
                    Debug.LogError("Move button doesn't have MenuButtonMoveController component");
                }

                if (button.GetComponentInChildren<Text>() == null)
                {
                    Debug.LogError("Move button doesn't have Text component in children");
                }

            }

            gameObjectMoveDetailsPane.SetActive(false);

        }

        public bool selectionListenersSetUp
        {
            get;
            protected set;
        }

        public virtual void SetUpSelectionListeners()
        {

            moveButtons[0].GetComponent<MenuButtonMoveController>().MoveSelected.AddListener(() => SetMovePaneDetailsFromIndex(0));
            moveButtons[0].GetComponent<MenuButtonMoveController>().MoveDeselected.AddListener(HideMovePane);

            moveButtons[1].GetComponent<MenuButtonMoveController>().MoveSelected.AddListener(() => SetMovePaneDetailsFromIndex(1));
            moveButtons[1].GetComponent<MenuButtonMoveController>().MoveDeselected.AddListener(HideMovePane);

            moveButtons[2].GetComponent<MenuButtonMoveController>().MoveSelected.AddListener(() => SetMovePaneDetailsFromIndex(2));
            moveButtons[2].GetComponent<MenuButtonMoveController>().MoveDeselected.AddListener(HideMovePane);

            moveButtons[3].GetComponent<MenuButtonMoveController>().MoveSelected.AddListener(() => SetMovePaneDetailsFromIndex(3));
            moveButtons[3].GetComponent<MenuButtonMoveController>().MoveDeselected.AddListener(HideMovePane);

            selectionListenersSetUp = true;

        }

        public virtual void Start()
        {

            if (!selectionListenersSetUp)
                SetUpSelectionListeners();

        }

        public void CloseMenu()
        {
            HideMovePane();
        }

        protected PokemonMove[] GetMoves() => PlayerData
            .singleton
            .partyPokemon[PlayerBattleUIController.singleton.currentSelectedPartyPokemonIndex]
            .moveIds
            .Where(x => !PokemonMove.MoveIdIsUnset(x))
            .Select(x => PokemonMove.GetPokemonMoveById(x))
            .ToArray();

        public virtual void RefreshMoveButtons()
        {

            PokemonMove[] moves = GetMoves();

            for (int i = 0; i < moves.Length; i++)
            {

                moveButtons[i].GetComponentInChildren<Text>().text = moves[i].name;

            }

            if (moves.Length < 4)
                moveButtons[3].gameObject.SetActive(false);
            else
                moveButtons[3].gameObject.SetActive(true);

            if (moves.Length < 3)
                moveButtons[2].gameObject.SetActive(false);
            else
                moveButtons[2].gameObject.SetActive(true);

            if (moves.Length < 2)
                moveButtons[1].gameObject.SetActive(false);
            else
                moveButtons[1].gameObject.SetActive(true);

        }

        protected void SetMovePaneDetailsFromIndex(int moveIndex)
        {

            int moveId = GetMoves()[moveIndex].id;
            byte remainingPP = PlayerData
                .singleton
                .partyPokemon[PlayerBattleUIController.singleton.currentSelectedPartyPokemonIndex]
                .movePPs
                [moveIndex];

            SetMovePaneDetails(moveId, remainingPP);

        }

        protected void SetMovePaneDetails(int moveId, byte remainingPP)
        {

            PokemonMove move = PokemonMove.GetPokemonMoveById(moveId);

            textName.text = move.name;
            textPPValue.text = remainingPP + "/" + move.maxPP;
            textDescription.text = move.description;
            textPowerValue.text = move.power != 0 ? move.power.ToString() : "-";
            textAccuracyValue.text = move.accuracy != 0 ? move.accuracy.ToString() : "-";

            imageCategory.sprite = SpriteStorage.GetMoveTypeSprite(move.moveType);
            imageType.sprite = SpriteStorage.GetTypeSymbolSprite(move.type);

            ShowMovePane();

        }

        protected void ShowMovePane() => gameObjectMoveDetailsPane.SetActive(true);
        protected void HideMovePane() => gameObjectMoveDetailsPane.SetActive(false);

    }
}
