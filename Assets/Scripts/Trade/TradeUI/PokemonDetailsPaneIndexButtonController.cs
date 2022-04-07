using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace Trade.TradeUI
{
    [RequireComponent(typeof(Button))]
    public class PokemonDetailsPaneIndexButtonController : MenuSelectableController
    {

        private Button Button => GetComponent<Button>();

        public int paneIndex;

        private PokemonDetailsAreaController detailsAreaController;

        public void SetUp(PokemonDetailsAreaController detailsAreaController)
        {

            this.detailsAreaController = detailsAreaController;

            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(OnClick);

        }

        private void OnClick()
        {
            detailsAreaController.SetCurrentPaneIndex(paneIndex);
        }

    }
}