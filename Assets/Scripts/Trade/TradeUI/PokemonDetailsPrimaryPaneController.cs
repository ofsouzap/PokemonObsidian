using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items.PokeBalls;

namespace Trade.TradeUI
{
    public class PokemonDetailsPrimaryPaneController : PokemonDetailsPaneController
    {

        public const string noPokemonPromptMessage = "(Nothing Selected)";

        public Image imagePokeBall;
        public Text textName;
        public Image imageGender;
        public Image imageFront;
        public Image imageType1;
        public Image imageType2;
        public Image imageCheatPokemon;
        public Image imageShinyPokemon;

        public override void SetPokemon(PokemonInstance pokemon)
        {

            if (pokemon != null)
            {

                imagePokeBall.gameObject.SetActive(true);
                imagePokeBall.sprite = SpriteStorage.GetItemSprite(
                    PokeBall.GetPokeBallById(pokemon.pokeBallId)
                    .resourceName
                    );

                textName.text = pokemon.GetDisplayName();
                imageGender.gameObject.SetActive(true);
                imageGender.sprite = SpriteStorage.GetGenderSprite(pokemon.gender);

                imageFront.gameObject.SetActive(true);
                imageFront.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1);

                imageType1.gameObject.SetActive(true);
                imageType2.gameObject.SetActive(true);
                imageType1.sprite = SpriteStorage.GetTypeSymbolSprite(pokemon.species.type1);

                Type? pokemonType2 = pokemon.species.type2;
                if (pokemonType2 == null)
                    imageType2.gameObject.SetActive(false);
                else
                {
                    imageType2.gameObject.SetActive(true);
                    imageType2.sprite = SpriteStorage.GetTypeSymbolSprite((Type)pokemonType2);
                }

                imageCheatPokemon.gameObject.SetActive(pokemon.cheatPokemon);
                imageShinyPokemon.gameObject.SetActive(pokemon.IsShiny);

            }
            else
            {

                imageFront.gameObject.SetActive(false);

                imageType1.gameObject.SetActive(false);
                imageType2.gameObject.SetActive(false);

                imagePokeBall.gameObject.SetActive(false);

                textName.text = noPokemonPromptMessage;
                imageGender.gameObject.SetActive(false);

                imageCheatPokemon.gameObject.SetActive(false);
                imageShinyPokemon.gameObject.SetActive(false);

            }

        }

    }
}