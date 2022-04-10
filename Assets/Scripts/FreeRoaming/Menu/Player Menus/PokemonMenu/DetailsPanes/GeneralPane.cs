using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items.PokeBalls;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu.DetailsPanes
{
    public class GeneralPane : DetailsPane
    {

        public Image imagePokeBall;
        public Text textName;
        public Image imageGender;
        public Image imageFront;
        public Text textOriginalTrainer;
        public Text textSpeciesName;
        public Text textSpeciesNumber;
        public Image imageType1;
        public Image imageType2;
        public Text textAbilityName;
        public Text textAbilityDescription;
        public Image imageCheatPokemon;

        public override void RefreshDetails(PokemonInstance pokemon)
        {

            imagePokeBall.sprite = SpriteStorage.GetItemSprite(
                PokeBall.GetPokeBallById(pokemon.pokeBallId)
                .resourceName
                );

            textName.text = pokemon.GetDisplayName();
            imageGender.sprite = SpriteStorage.GetGenderSprite(pokemon.gender);

            textSpeciesName.text = pokemon.species.name;
            textSpeciesNumber.text = pokemon.species.id.ToString();
            imageType1.sprite = SpriteStorage.GetTypeSymbolSprite(pokemon.species.type1);

            imageFront.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1);

            Type? pokemonType2 = pokemon.species.type2;
            if (pokemonType2 == null)
                imageType2.gameObject.SetActive(false);
            else
            {
                imageType2.gameObject.SetActive(true);
                imageType2.sprite = SpriteStorage.GetTypeSymbolSprite((Type)pokemonType2);
            }

            textOriginalTrainer.text = pokemon.originalTrainerName;

            //TODO - once abilities made, set ability Text texts
            textAbilityName.text = "";
            textAbilityDescription.text = "";

            imageCheatPokemon.gameObject.SetActive(pokemon.cheatPokemon);

        }

    }
}
