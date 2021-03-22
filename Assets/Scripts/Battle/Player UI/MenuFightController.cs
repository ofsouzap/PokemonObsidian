﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pokemon.Moves;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class MenuFightController : MenuController
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

        public Button buttonMoveStruggle;

        public Text textPPValue;
        public Text textPowerValue;
        public Text textAccuracyValue;

        public Image imageCategory;
        public Image imageType;

        public GameObject gameObjectMoveDetailsPane;

        /// <summary>
        /// The index of the pokemon currently selected to view the moves of
        /// </summary>
        private int currentPokemonIndex;

        public void SetCurrentPokemonIndex(int index) => currentPokemonIndex = index;

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

        private void Start()
        {

            foreach (Button button in moveButtons)
            {
                if (button.GetComponent<MenuButtonMoveController>() == null)
                    Debug.LogError("No MenuButtonMoveController in move button");
            }

            moveButtons[0].GetComponent<MenuButtonMoveController>().MoveSelected.AddListener(() => SetMovePaneDetails(0));
            moveButtons[0].GetComponent<MenuButtonMoveController>().MoveDeselected.AddListener(HideMovePane);

            moveButtons[1].GetComponent<MenuButtonMoveController>().MoveSelected.AddListener(() => SetMovePaneDetails(1));
            moveButtons[1].GetComponent<MenuButtonMoveController>().MoveDeselected.AddListener(HideMovePane);

            moveButtons[2].GetComponent<MenuButtonMoveController>().MoveSelected.AddListener(() => SetMovePaneDetails(2));
            moveButtons[2].GetComponent<MenuButtonMoveController>().MoveDeselected.AddListener(HideMovePane);

            moveButtons[3].GetComponent<MenuButtonMoveController>().MoveSelected.AddListener(() => SetMovePaneDetails(3));
            moveButtons[3].GetComponent<MenuButtonMoveController>().MoveDeselected.AddListener(HideMovePane);

        }

        private PokemonMove[] GetMoves() => PlayerData
            .singleton
            .partyPokemon[currentPokemonIndex]
            .moveIds
            .Where(x => !PokemonMove.MoveIdIsUnset(x))
            .Select(x => PokemonMove.GetPokemonMoveById(x))
            .ToArray();

        public void RefreshMoveButtons()
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

            bool showStruggleMoveButton = PlayerData.singleton
                .partyPokemon[currentPokemonIndex]
                .movePPs
                .All(x => x <= 0);

            buttonMoveStruggle.gameObject.SetActive(showStruggleMoveButton);

        }

        private void SetMovePaneDetails(int moveIndex)
        {

            PokemonMove move = GetMoves()[moveIndex];
            byte[] remainingPPs = PlayerData
                .singleton
                .partyPokemon[currentPokemonIndex]
                .movePPs;

            textPPValue.text = remainingPPs[moveIndex] + "/" + move.maxPP;
            textPowerValue.text = move.power != 0 ? move.power.ToString() : "-";
            textAccuracyValue.text = move.accuracy != 0 ? move.accuracy.ToString() : "-";

            //TODO - have below images set once their sprites are ready
            imageCategory.sprite = null;
            imageType.sprite = null;

            ShowMovePane();

        }

        private void ShowMovePane() => gameObjectMoveDetailsPane.SetActive(true);
        private void HideMovePane() => gameObjectMoveDetailsPane.SetActive(false);

    }
}
