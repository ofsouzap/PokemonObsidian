using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Pokemon;
using Menus;

namespace Trade.TradeUI
{
    [RequireComponent(typeof(Canvas))]
    public class TradeUIController : MenuController
    {

        private TradeManager manager;

        public GameObject defaultSelectedGameObject;

        public PokemonListAreaController listAreaController;

        public PokemonDetailsAreaController selectedPokemonDetails;
        public PokemonDetailsAreaController otherUserOfferedPokemonDetails;

        public Text otherUserOfferedPokemonDetailsLabel;
        public const string otherUserOfferedPokemonDetailsLabelSuffix = "'s Offered Pokemon";

        public Button offerPokemonButton;

        public Button closeTradeButton;

        public bool InteractionEnabled { get; private set; }

        private PlayerData Player => PlayerData.singleton;

        public PlayerData.PokemonLocator? selectedPokemonLocator;

        protected override MenuSelectableController[] GetSelectables()
        {
            
            List<MenuSelectableController> selectables = new List<MenuSelectableController>();
            selectables.AddRange(selectedPokemonDetails.GetSelectables());
            selectables.AddRange(otherUserOfferedPokemonDetails.GetSelectables());
            selectables.AddRange(listAreaController.GetSelectables());
            selectables.Add(offerPokemonButton.GetComponent<MenuSelectableController>());
            return selectables.ToArray();

        }

        public void SetUp(TradeManager tradeManager)
        {

            manager = tradeManager;

            RefreshOtherUserName();

            listAreaController.SetUp(this);

            selectedPokemonDetails.SetUp(this);
            otherUserOfferedPokemonDetails.SetUp(this);

            if (offerPokemonButton.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController on offer pokemon button");

            offerPokemonButton.onClick.RemoveAllListeners();
            offerPokemonButton.onClick.AddListener(OnOfferPokemonClicked);

            if (closeTradeButton.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController on close trade button");

            closeTradeButton.onClick.RemoveAllListeners();
            closeTradeButton.onClick.AddListener(OnCloseTradeClicked);

            RefreshOfferButtonInteractivity();

            TrySetDefaultSelectedGameObject();

        }

        public void TrySetDefaultSelectedGameObject()
        {

            if (defaultSelectedGameObject == null)
                Debug.LogWarning("No default selected game object set");
            else
                EventSystem.current.SetSelectedGameObject(defaultSelectedGameObject);

        }

        public void RefreshOtherUserName()
        {
            otherUserOfferedPokemonDetailsLabel.text = manager.otherUserName + otherUserOfferedPokemonDetailsLabelSuffix;
        }

        public void OnPokemonSelected(PlayerData.PokemonLocator locator)
        {

            selectedPokemonLocator = locator;
            selectedPokemonDetails.SetPokemon(selectedPokemonLocator?.Get(Player));

            RefreshOfferButtonInteractivity();

        }

        private void RefreshOfferButtonInteractivity()
        {

            if (!InteractionEnabled
                || selectedPokemonLocator == null
                || ((PlayerData.PokemonLocator)selectedPokemonLocator).Get(Player) == null)
                offerPokemonButton.interactable = false;
            else
                offerPokemonButton.interactable = true;

        }

        private void OnOfferPokemonClicked()
        {

            if (selectedPokemonLocator == null) //No locator set
            {
                Debug.LogError("No pokemon locator set but trying to offer pokemon");
            }
            else if (((PlayerData.PokemonLocator)selectedPokemonLocator).Get(Player) == null) //Null pointer
            {
                Debug.LogError("Trying to offer pokemon when locator points to null");
            }
            else
            {
                manager.TryConfirmOfferPokemon((PlayerData.PokemonLocator)selectedPokemonLocator);
            }

        }

        private void OnCloseTradeClicked()
        {

            manager.TryCloseTrading();

        }

        /// <param name="otherUserOfferedPokemonState">What to change the other user's offered pokemon's details area's interactivity state to or null if it should be the same as the rest</param>
        public void SetInteractionEnabled(bool state,
            bool? otherUserOfferedPokemonState = null)
        {

            InteractionEnabled = state;

            listAreaController.SetInteractivity(state);

            otherUserOfferedPokemonDetails.SetInteractivity(otherUserOfferedPokemonState ?? state);
            selectedPokemonDetails.SetInteractivity(state);

            RefreshOfferButtonInteractivity();

            closeTradeButton.interactable = state;

        }

        public void SetOtherUserOfferedPokemon(PokemonInstance pmon)
        {

            otherUserOfferedPokemonDetails.SetPokemon(pmon);

        }

    }
}