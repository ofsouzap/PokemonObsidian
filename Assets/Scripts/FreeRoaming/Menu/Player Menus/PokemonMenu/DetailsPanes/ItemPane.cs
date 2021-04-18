using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items;
using UnityEngine.EventSystems;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu.DetailsPanes
{
    public class ItemPane : DetailsPane
    {

        public Image imageIcon;
        public Text textName;
        public Text textDescription;
        public Button buttonTake;

        public override void SetUp()
        {

            base.SetUp();

            buttonTake.onClick.AddListener(() => TakeCurrentPokemonItem());

        }

        public override void RefreshDetails(PokemonInstance pokemon)
        {

            Item heldItem = pokemon.heldItem;

            if (heldItem != null)
            {

                imageIcon.enabled = true;
                imageIcon.sprite = heldItem.LoadSprite();
                textName.text = heldItem.itemName;
                textDescription.text = heldItem.description;
                buttonTake.gameObject.SetActive(true);

            }
            else
            {

                imageIcon.enabled = false;
                textName.text = "No held item";
                textDescription.text = "";
                buttonTake.gameObject.SetActive(false);

            }

        }

        private void TakeCurrentPokemonItem()
        {

            int currentPokemonIndex = PokemonMenuController.singleton.CurrentPokemonIndex;
            Item item = PlayerData.singleton.partyPokemon[currentPokemonIndex].heldItem;

            PlayerData.singleton.inventory.AddItem(item, 1);
            PlayerData.singleton.partyPokemon[currentPokemonIndex].heldItem = null;

            RefreshDetails(PokemonMenuController.singleton.CurrentPokemon);
            EventSystem.current.SetSelectedGameObject(PokemonMenuController.singleton.toolbarController.detailsPaneSelectables[0].gameObject);

            textBoxController.Show();
            textBoxController.SetTextInstant("Item has been taken back");
            textBoxController.SetHideDelay(1F);

        }

    }
}