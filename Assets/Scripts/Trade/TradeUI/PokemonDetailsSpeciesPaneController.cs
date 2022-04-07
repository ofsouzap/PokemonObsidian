using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items.PokeBalls;

namespace Trade.TradeUI
{
    public class PokemonDetailsSpeciesPaneController : PokemonDetailsPaneController
    {

        public Text textSpeciesName;
        public Text textSpeciesNumber;
        public Text textAbilityName;
        public Image imageFront2;
        public Image imageBack;

        public override void SetPokemon(PokemonInstance pokemon)
        {

            if (pokemon != null)
            {

                textSpeciesName.text = pokemon.species.name;
                textSpeciesNumber.text = pokemon.species.id.ToString();

                textAbilityName.text = ""; //TODO - once abilities made, set ability name text to ability's name

                imageFront2.enabled = true;
                imageFront2.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Front2);

                imageBack.enabled = true;
                imageBack.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Back);

            }
            else
            {

                textSpeciesName.text = "";
                textSpeciesNumber.text = "";

                textAbilityName.text = "";

                imageFront2.enabled = false;
                imageBack.enabled = false;

            }

        }

    }
}