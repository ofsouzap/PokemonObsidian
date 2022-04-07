using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items;

namespace Trade.TradeUI
{
    public class PokemonDetailsItemPaneController : PokemonDetailsPaneController
    {

        public Image imageIcon;
        public Text textName;
        public Text textDescription;

        public override void SetPokemon(PokemonInstance pokemon)
        {

            if (pokemon == null)
            {

                imageIcon.enabled = false;
                textName.text = "No pokemon selected";
                textDescription.text = "";

            }
            else
            {

                Item heldItem = pokemon.heldItem;

                if (heldItem != null)
                {

                    imageIcon.enabled = true;
                    imageIcon.sprite = heldItem.LoadSprite();
                    textName.text = heldItem.itemName;
                    textDescription.text = heldItem.description;

                }
                else
                {

                    imageIcon.enabled = false;
                    textName.text = "No held item";
                    textDescription.text = "";

                }

            }

        }

    }
}