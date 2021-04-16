using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using FreeRoaming.Menu.PlayerMenus.PokemonMenu.DetailsPanes;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu
{
    public class DetailsPaneController : MonoBehaviour
    {

        public PokemonMenuController pokemonMenuController;

        public DetailsPane[] detailsPanes;

        public void SetUp()
        {
            SetCurrentPaneIndex(0);
        }

        private void HideAllPanes()
        {
            foreach (DetailsPane pane in detailsPanes)
                pane.gameObject.SetActive(false);
        }

        public void SetCurrentPaneIndex(int index)
        {

            HideAllPanes();
            detailsPanes[index].gameObject.SetActive(true);

        }

        public void RefreshPanes()
        {

            PokemonInstance pokemon = pokemonMenuController.CurrentPokemon;

            foreach (DetailsPane pane in detailsPanes)
                pane.RefreshDetails(pokemon);

        }

    }
}