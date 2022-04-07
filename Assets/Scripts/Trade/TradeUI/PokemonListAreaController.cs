using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace Trade.TradeUI
{
    public class PokemonListAreaController : MonoBehaviour
    {

        private TradeUIController tradeUIController;
        private bool interactionEnabled;

        public PokemonListPartyViewController partyView;
        public PokemonListBoxViewController boxView;

        public Button partyBoxesSwitchButton;

        public const string partyBoxesSwitchButtonText_OnParty = "Boxes";
        public const string partyBoxesSwitchButtonText_OnBoxes = "Party";

        public Button prevBoxButton;
        public Button nextBoxButton;

        private PlayerData player;

        private PlayerData.PokemonBox[] Boxes => player.boxPokemon.boxes;
        private int BoxCount => Boxes.Length;

        /// <summary>
        /// Whether the menu is currently showing the boxes instead of the party
        /// </summary>
        private bool showingBoxes = false;

        /// <summary>
        /// The index of the storage system box the menu is currently showing (only relevant if currently showing boxes and not party)
        /// </summary>
        public int boxIndex { get; private set; }

        public MenuSelectableController[] GetSelectables()
        {

            List<MenuSelectableController> selectables = new List<MenuSelectableController>();
            selectables.AddRange(partyView.GetSelectables());
            selectables.AddRange(boxView.GetSelectables());
            selectables.Add(partyBoxesSwitchButton.GetComponent<MenuSelectableController>());
            selectables.Add(prevBoxButton.GetComponent<MenuSelectableController>());
            selectables.Add(nextBoxButton.GetComponent<MenuSelectableController>());
            return selectables.ToArray();

        }

        public void SetUp(TradeUIController tradeUIController,
            PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            this.player = player;

            this.tradeUIController = tradeUIController;

            showingBoxes = false;
            boxIndex = 0;

            if (partyBoxesSwitchButton.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController on party/boxes switch button");

            if (prevBoxButton.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController on previous box button");

            if (nextBoxButton.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController on next box button");

            prevBoxButton.onClick.RemoveAllListeners();
            prevBoxButton.onClick.AddListener(PrevBox);

            nextBoxButton.onClick.RemoveAllListeners();
            nextBoxButton.onClick.AddListener(NextBox);

            partyBoxesSwitchButton.onClick.RemoveAllListeners();
            partyBoxesSwitchButton.onClick.AddListener(SwapListViews);

            partyView.SetUp(this);
            boxView.SetUp(this);

            RefreshAll();

        }

        public void OnPokemonSelected(PlayerData.PokemonLocator locator)
        {

            tradeUIController.OnPokemonSelected(locator);

        }

        private void SwapListViews()
        {

            //Swap views setting
            showingBoxes = !showingBoxes;

            RefreshAll();

        }

        private void RefreshAll()
        {

            //Set swap button text
            if (!showingBoxes)
                partyBoxesSwitchButton.GetComponentInChildren<Text>().text = partyBoxesSwitchButtonText_OnParty;
            else
                partyBoxesSwitchButton.GetComponentInChildren<Text>().text = partyBoxesSwitchButtonText_OnBoxes;

            //Refresh the box navigation buttons' interactability
            RefreshBoxNavButtonInteractivity();

            //Refresh the views
            RefreshCurrentView();

        }

        private void RefreshCurrentView()
        {

            RefreshPartyView();
            RefreshCurrentBox();

            RefreshViewVisibility();

        }

        private void RefreshViewVisibility()
        {

            if (!showingBoxes)
            {
                partyView.Show();
                boxView.Hide();
            }
            else
            {
                partyView.Hide();
                boxView.Show();
            }

        }

        private void RefreshPartyView()
        {

            partyView.SetPokemon(player);

        }

        private void RefreshCurrentBox()
        {

            boxView.RefreshPokemon(player);

        }

        private void RefreshBoxNavButtonInteractivity()
        {

            prevBoxButton.interactable = showingBoxes && interactionEnabled;
            nextBoxButton.interactable = showingBoxes && interactionEnabled;

        }

        private void PrevBox()
        {
            boxIndex = (boxIndex - 1 + BoxCount) % BoxCount;
            RefreshCurrentBox();
        }

        private void NextBox()
        {
            boxIndex = (boxIndex + 1) % BoxCount;
            RefreshCurrentBox();
        }

        public void SetInteractivity(bool state)
        {

            interactionEnabled = state;

            partyView.SetInteractivity(state);
            boxView.SetInteractivity(state);

            partyBoxesSwitchButton.interactable = interactionEnabled;

            RefreshBoxNavButtonInteractivity();

        }

    }
}