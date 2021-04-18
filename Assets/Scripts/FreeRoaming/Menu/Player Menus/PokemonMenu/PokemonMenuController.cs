using Menus;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu
{
    public class PokemonMenuController : PlayerMenuController
    {

        public static PokemonMenuController singleton;

        public PokemonOptionsController pokemonOptionsController;
        public ToolbarController toolbarController;
        public DetailsPaneController detailsPaneController;

        protected byte currentPokemonIndex;
        public byte CurrentPokemonIndex => currentPokemonIndex;
        public PokemonInstance CurrentPokemon => PlayerData.singleton.partyPokemon[currentPokemonIndex];

        protected override GameObject GetDefaultSelectedGameObject()
            => pokemonOptionsController.optionControllers[0].gameObject;

        protected override MenuSelectableController[] GetSelectables()
        {
            List<MenuSelectableController> selectables = new List<MenuSelectableController>();

            selectables.AddRange(pokemonOptionsController.GetSelectables());
            selectables.AddRange(toolbarController.GetSelectables());

            return selectables.ToArray();
        }

        protected override void SetUp()
        {

            if (singleton != null)
            {
                Debug.LogError("Multiple PokemonMenuControllers found. Destroying self");
                Destroy(gameObject);
            }
            else
                singleton = this;

            pokemonOptionsController.SetUp();
            toolbarController.SetUp();
            detailsPaneController.SetUp();

            TrySetCurrentPokemonIndex(0);
            SetCurrentDetailsPaneIndex(0);

        }

        public void TrySetCurrentPokemonIndex(byte index)
        {

            if (PlayerData.singleton.partyPokemon[index] != null)
            {

                currentPokemonIndex = index;

                detailsPaneController.RefreshPanes();

            }

        }

        public void SetCurrentDetailsPaneIndex(int index)
        {

            detailsPaneController.SetCurrentPaneIndex(index);

        }

    }
}
