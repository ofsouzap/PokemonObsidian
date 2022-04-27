using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Pokemon.Moves;
using Menus;

namespace Battle.PlayerUI.LearnMoveUI
{
    public class MenuLearnMoveController : MenuPartyPokemonMovesController
    {

        public Button newMoveButton;

        protected override MenuSelectableController[] GetSelectables()
        {

            MenuSelectableController[] output = new MenuSelectableController[moveButtons.Length + 1];

            Array.Copy(
                moveButtons.Select(x => x.GetComponent<MenuSelectableController>()).ToArray(),
                output,
                moveButtons.Length);

            output[output.Length - 1] = newMoveButton.GetComponent<MenuSelectableController>();

            return output;

        }

        protected override PokemonInstance GetSelectedPokemon()
            => selectedPokemon;

        private PokemonInstance selectedPokemon;
        private int newMoveId;
        private PokemonMove NewMove
        {
            get => PokemonMove.GetPokemonMoveById(newMoveId);
        }

        public override void SetUpSelectionListeners()
        {

            base.SetUpSelectionListeners();

            newMoveButton.GetComponent<MenuButtonMoveController>().MoveSelected.AddListener(() => SetMovePaneDetails(newMoveId, NewMove.maxPP));
            newMoveButton.GetComponent<MenuButtonMoveController>().MoveDeselected.AddListener(HideMovePane);

        }

        public override void Start()
        {

            base.Start();

            if (newMoveButton.GetComponent<MenuButtonMoveController>() == null)
                Debug.LogError("No MenuSelectableController found on newMoveButton");

        }

        public override void RefreshMoveButtons()
        {

            base.RefreshMoveButtons();

            newMoveButton.GetComponentInChildren<Text>().text = NewMove.name;

        }

        public void SetNewMoveId(int moveId) => newMoveId = moveId;

        public void SetSelectedPokemon(PokemonInstance pokemon) => selectedPokemon = pokemon;

    }
}
