using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus.PokedexMenu
{

    public class PokedexPokemonListItemController : ScrollListItemController<KeyValuePair<PokemonSpecies, bool>>
    {

        // Template class value of KeyValuePair<PokemonSpecies, bool> describes the pokemon species and whether it has been seen yet by the player

        public const string undiscoveredSpeciesNamePlaceholder = "???";

        public Image imageIcon;
        public Text textId;
        public Text textName;

        public override void SetValues(KeyValuePair<PokemonSpecies, bool> vs)
        {

            PokemonSpecies species = vs.Key;
            
            if (vs.Value)
            {

                imageIcon.gameObject.SetActive(true);
                imageIcon.sprite = species.LoadSprite(PokemonSpecies.SpriteType.Icon, null);

                textId.text = species.id.ToString();
                textName.text = species.name;

            }
            else
            {

                imageIcon.gameObject.SetActive(false);

                textId.text = species.id.ToString();
                textName.text = undiscoveredSpeciesNamePlaceholder;

            }

        }

    }

}