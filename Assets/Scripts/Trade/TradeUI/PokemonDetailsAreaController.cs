using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

namespace Trade.TradeUI
{
    public class PokemonDetailsAreaController : MonoBehaviour
    {

        private TradeUIController tradeUIController;
        private bool interactionEnabled;

        public PokemonDetailsPaneController primaryDetailsPane;

        public PokemonDetailsPaneController[] detailsPanes;
        public PokemonDetailsPaneIndexButtonController[] paneIndexButtons;

        public MenuSelectableController[] GetSelectables()
        {

            List<MenuSelectableController> selectables = new List<MenuSelectableController>();

            selectables.AddRange(paneIndexButtons.Select(x => x.GetComponent<MenuSelectableController>()));

            return selectables.ToArray();

        }

        private int paneIndex = 0;

        public void SetUp(TradeUIController tradeUIController)
        {

            this.tradeUIController = tradeUIController;

            SetCurrentPaneIndex(0);

            SetPokemon(null);

            foreach (PokemonDetailsPaneIndexButtonController btn in paneIndexButtons)
                btn.SetUp(this);

        }

        private void HideAllPanes()
        {
            foreach (PokemonDetailsPaneController pane in detailsPanes)
                pane.Hide();
        }

        public void SetCurrentPaneIndex(int index)
        {
            paneIndex = index;
            HideAllPanes();
            detailsPanes[paneIndex].Show();
        }

        public void SetPokemon(PokemonInstance pmon)
        {

            primaryDetailsPane.SetPokemon(pmon);

            foreach (PokemonDetailsPaneController pane in detailsPanes)
                pane.SetPokemon(pmon);

        }

        public void SetInteractivity(bool state)
        {

            interactionEnabled = state;

            foreach (PokemonDetailsPaneIndexButtonController indexBtn in paneIndexButtons)
                indexBtn.GetComponent<Selectable>().interactable = interactionEnabled;

        }

    }
}