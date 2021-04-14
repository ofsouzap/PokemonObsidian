using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pokemon.Moves;
using Menus;

namespace Battle.PlayerUI
{
    public class PlayerMoveSelectUIController : MenuController
    {

        public BattleParticipantPlayer playerBattleParticipant;

        public Button buttonBack;

        public MenuButtonMoveController[] moveButtons;

        protected override MenuSelectableController[] GetSelectables()
        {
            MenuSelectableController[] output = new MenuSelectableController[moveButtons.Length + 1];
            Array.Copy(
                moveButtons,
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
                Debug.LogError("Non-4-length move buttons array");
                return;
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

            moveButtons[0].Button.onClick.AddListener(() => playerBattleParticipant.MoveSelectUISelectMove(0));
            moveButtons[1].Button.onClick.AddListener(() => playerBattleParticipant.MoveSelectUISelectMove(1));
            moveButtons[2].Button.onClick.AddListener(() => playerBattleParticipant.MoveSelectUISelectMove(2));
            moveButtons[3].Button.onClick.AddListener(() => playerBattleParticipant.MoveSelectUISelectMove(3));

            moveButtons[0].MoveSelected.AddListener(() => SetMovePaneDetailsFromIndex(0));
            moveButtons[0].MoveDeselected.AddListener(HideMovePane);

            moveButtons[1].MoveSelected.AddListener(() => SetMovePaneDetailsFromIndex(1));
            moveButtons[1].MoveDeselected.AddListener(HideMovePane);

            moveButtons[2].MoveSelected.AddListener(() => SetMovePaneDetailsFromIndex(2));
            moveButtons[2].MoveDeselected.AddListener(HideMovePane);

            moveButtons[3].MoveSelected.AddListener(() => SetMovePaneDetailsFromIndex(3));
            moveButtons[3].MoveDeselected.AddListener(HideMovePane);

            selectionListenersSetUp = true;

        }

        public virtual void Start()
        {

            if (!selectionListenersSetUp)
                SetUpSelectionListeners();

        }

        public void RefreshButtons()
        {

            int[] moveIds = playerBattleParticipant.ActivePokemon.moveIds;

            for (int i = 0; i < moveIds.Length; i++)
            {

                if (PokemonMove.MoveIdIsUnset(moveIds[i]))
                {
                    moveButtons[i].SetInteractable(false);
                }
                else
                {

                    moveButtons[i].SetInteractable(true);

                    moveButtons[i].SetName(PokemonMove.GetPokemonMoveById(moveIds[i]).name);

                }

            }

        }

        protected void SetMovePaneDetailsFromIndex(int moveIndex)
        {

            int moveId = PokemonMove.GetPokemonMoveById(playerBattleParticipant.ActivePokemon.moveIds[moveIndex]).id;
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

        public void ShowBackButton()
        {
            buttonBack.gameObject.SetActive(true);
        }

        public void HideBackButton()
        {
            buttonBack.gameObject.SetActive(false);
        }

        public void ShowMovePane() => gameObjectMoveDetailsPane.SetActive(true);
        public void HideMovePane() => gameObjectMoveDetailsPane.SetActive(false);

    }
}
