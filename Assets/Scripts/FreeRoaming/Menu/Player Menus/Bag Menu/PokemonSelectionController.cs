using System;
using Menus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pokemon;
using Items;

namespace FreeRoaming.Menu.PlayerMenus.BagMenu
{
    public class PokemonSelectionController : MenuController
    {

        private BagMenuController bagMenuController;

        public PokemonButtonController[] pokemonButtons;

        protected override MenuSelectableController[] GetSelectables()
            => pokemonButtons;

        public void SetUp(BagMenuController bagMenuController)
        {

            this.bagMenuController = bagMenuController;

            if (pokemonButtons.Length != 6)
            {
                Debug.LogError("Non-6 number of pokemon buttons provided");
            }

            SetUpListeners();

        }

        private void SetUpListeners()
        {

            pokemonButtons[0].Button.onClick.AddListener(() => OnPokemonSelected(0));
            pokemonButtons[1].Button.onClick.AddListener(() => OnPokemonSelected(1));
            pokemonButtons[2].Button.onClick.AddListener(() => OnPokemonSelected(2));
            pokemonButtons[3].Button.onClick.AddListener(() => OnPokemonSelected(3));
            pokemonButtons[4].Button.onClick.AddListener(() => OnPokemonSelected(4));
            pokemonButtons[5].Button.onClick.AddListener(() => OnPokemonSelected(5));

        }

        public void SetSelectableAsSelection()
        {
            EventSystem.current.SetSelectedGameObject(pokemonButtons[0].gameObject);
        }

        public override void Show()
        {

            base.Show();

            SetPokemon(PlayerData.singleton.partyPokemon);

        }

        private void SetPokemon(PokemonInstance[] pokemon)
        {

            if (pokemon.Length > 6)
            {
                Debug.LogError("Too many pokemon provided");
                return;
            }

            PokemonInstance[] pokemonToSet = new PokemonInstance[6];

            Array.Copy(pokemon, pokemonToSet, pokemon.Length);

            for (int i = 0; i < pokemonToSet.Length; i++)
                pokemonButtons[i].SetPokemon(pokemonToSet[i]);

        }

        private void OnPokemonSelected(int index)
        {
            bagMenuController.OnPokemonSelected(index);
        }

    }
}