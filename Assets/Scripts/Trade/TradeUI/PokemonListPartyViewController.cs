using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

namespace Trade.TradeUI
{
    public class PokemonListPartyViewController : MonoBehaviour
    {

        private PokemonListAreaController listAreaController;

        private bool interactionEnabled;

        public PokemonPosition[] pokemonPositions;

        public MenuSelectableController[] GetSelectables()
        {

            List<MenuSelectableController> selectables = new List<MenuSelectableController>();
            selectables.AddRange(pokemonPositions);
            return selectables.ToArray();

        }

        public void Show()
            => gameObject.SetActive(true);

        public void Hide()
            => gameObject.SetActive(false);

        public void SetUp(PokemonListAreaController listAreaController)
        {

            this.listAreaController = listAreaController;

            if (pokemonPositions.Length < PlayerData.partyCapacity)
            {
                Debug.LogError("Not enough party pokemon positions");
            }

            for (byte i = 0; i < pokemonPositions.Length; i++)
            {

                int posIndex = i;

                pokemonPositions[i].SetOnClickAction(() => OnPositionSelected(posIndex));

            }

        }

        private void OnPositionSelected(int index)
        {

            listAreaController.OnPokemonSelected(new PlayerData.PokemonLocator(partyIndex: index));

        }

        public void SetPokemon(PlayerData player)
            => SetPokemon(player.partyPokemon);

        public void SetPokemon(PokemonInstance[] pokemon)
        {

            if (pokemon.Length > pokemonPositions.Length)
            {
                Debug.LogError("Too many pokemon provided for number of party positions");
                return;
            }

            for (byte i = 0; i < pokemonPositions.Length; i++)
            {

                if (i >= pokemonPositions.Length)
                    pokemonPositions[i].SetPokemon(null);
                else
                    pokemonPositions[i].SetPokemon(pokemon[i]);

            }

        }

        public void SetInteractivity(bool state)
        {

            interactionEnabled = state;

            foreach (PokemonPosition pmonPos in pokemonPositions)
                pmonPos.GetComponent<Selectable>().interactable = interactionEnabled;

        }

    }
}