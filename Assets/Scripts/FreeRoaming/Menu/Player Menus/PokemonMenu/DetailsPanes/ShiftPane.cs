using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items;
using UnityEngine.EventSystems;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu.DetailsPanes
{
    public class ShiftPane : DetailsPane
    {

        public Button buttonShiftUp;
        public Button buttonShiftDown;

        public override void SetUp()
        {

            base.SetUp();

            buttonShiftUp.onClick.AddListener(() => ShiftPokemonUp());
            buttonShiftDown.onClick.AddListener(() => ShiftPokemonDown());

        }

        public override void RefreshDetails(PokemonInstance pokemon) { }

        private void SwapPokemonIndexes(byte index1, byte index2)
        {

            PokemonInstance tmp = PlayerData.singleton.partyPokemon[index1];

            PlayerData.singleton.partyPokemon[index1] = PlayerData.singleton.partyPokemon[index2];
            PlayerData.singleton.partyPokemon[index2] = tmp;

        }

        private void ShiftPokemonUp()
        {

            byte currentIndex = PokemonMenuController.singleton.CurrentPokemonIndex;
            byte prevIndex = currentIndex != 0 ? (byte)(currentIndex - 1) : (byte)5;

            SwapPokemonIndexes(currentIndex, prevIndex);

            PokemonMenuController.singleton.pokemonOptionsController.SetUpOptions();
            PokemonMenuController.singleton.TrySetCurrentPokemonIndex(prevIndex);

        }

        private void ShiftPokemonDown()
        {

            byte currentIndex = PokemonMenuController.singleton.CurrentPokemonIndex;
            byte nextIndex = currentIndex != 5 ? (byte)(currentIndex + 1) : (byte)0;

            SwapPokemonIndexes(currentIndex, nextIndex);

            PokemonMenuController.singleton.pokemonOptionsController.SetUpOptions();
            PokemonMenuController.singleton.TrySetCurrentPokemonIndex(nextIndex);

        }

    }
}