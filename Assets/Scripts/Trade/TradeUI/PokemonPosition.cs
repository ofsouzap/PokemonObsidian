using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pokemon;
using Menus;

namespace Trade.TradeUI
{
    [RequireComponent(typeof(Button))]
    public class PokemonPosition : MenuSelectableController
    {

        public Image iconImage;
        public Image heldItemIcon;

        public void SetOnClickAction(Action action)
        {
            GetComponent<Button>().onClick.RemoveAllListeners();
            GetComponent<Button>().onClick.AddListener(() => action?.Invoke());
        }

        public void SetPokemon(PokemonInstance pokemon)
        {

            if (pokemon != null)
            {

                iconImage.enabled = true;
                iconImage.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Icon);

                if (pokemon.heldItem != null)
                    heldItemIcon.enabled = true;
                else
                    heldItemIcon.enabled = false;

            }
            else
            {
                iconImage.enabled = false;
                heldItemIcon.enabled = false;
            }

        }

    }
}