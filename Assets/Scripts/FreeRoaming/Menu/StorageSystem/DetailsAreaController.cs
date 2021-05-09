using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

namespace FreeRoaming.Menu.StorageSystem
{
    public class DetailsAreaController : MonoBehaviour
    {

        public Text textDisplayName;
        public Image imageGender;
        public Text textLevel;
        public const string levelPrefix = "Lvl. ";
        public Image imageSprite;
        public Text textSpeciesName;
        public Text textSpeciesNumber;
        public Image imageType1;
        public Image imageType2;
        public Text textNature;
        public Text textItemName;
        public Text textStatAttackValue;
        public Text textStatDefenseValue;
        public Text textStatSpecialAttackValue;
        public Text textStatSpecialDefenseValue;
        public Text textStatSpeedValue;
        public Text textStatHealthValue;
        public StatsHex statHexEV;
        public StatsHex statHexIV;

        private void SetPokemonShownState(bool state)
        {

            imageGender.enabled = state;
            imageSprite.enabled = state;
            imageType1.enabled = state;
            imageType2.enabled = state;
            statHexEV.enabled = state;
            statHexIV.enabled = state;

            if (!state)
            {
                textDisplayName.text = "";
                textLevel.text = levelPrefix;
                textSpeciesName.text = "";
                textSpeciesNumber.text = "";
                textNature.text = "";
                textItemName.text = "";
                textStatAttackValue.text = "";
                textStatDefenseValue.text = "";
                textStatSpecialAttackValue.text = "";
                textStatSpecialDefenseValue.text = "";
                textStatSpeedValue.text = "";
                textStatHealthValue.text = "";
            }

        }

        public void SetPokemon(PokemonInstance pokemon)
        {

            if (pokemon == null)
            {
                SetPokemonShownState(false);
            }
            else
            {

                SetPokemonShownState(true);

                textDisplayName.text = pokemon.GetDisplayName();
                imageGender.sprite = pokemon.LoadGenderSprite();
                textLevel.text = levelPrefix + pokemon.GetLevel().ToString();
                imageSprite.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1);

                textSpeciesName.text = pokemon.species.name;
                textSpeciesNumber.text = pokemon.species.id.ToString();
                imageType1.sprite = SpriteStorage.GetTypeSymbolSprite(pokemon.species.type1);
                if (pokemon.species.type2 != null)
                    imageType2.sprite = SpriteStorage.GetTypeSymbolSprite((Type)pokemon.species.type2);
                else
                    imageType2.enabled = false;

                //This capitalises first letter of the nature's name
                textNature.text = pokemon.nature.name[0].ToString().ToUpper() + pokemon.nature.name.Substring(1);
                textItemName.text = pokemon.heldItem != null ? pokemon.heldItem.itemName : "";

                textStatAttackValue.text = pokemon.GetStats().attack.ToString();
                textStatDefenseValue.text = pokemon.GetStats().defense.ToString();
                textStatSpecialAttackValue.text = pokemon.GetStats().specialAttack.ToString();
                textStatSpecialDefenseValue.text = pokemon.GetStats().specialDefense.ToString();
                textStatSpeedValue.text = pokemon.GetStats().speed.ToString();
                textStatHealthValue.text = pokemon.GetStats().health.ToString();

                statHexEV.values = new float[]
                {
                    (float)pokemon.effortValues.attack / PokemonInstance.maximumEffortValue,
                    (float)pokemon.effortValues.defense / PokemonInstance.maximumEffortValue,
                    (float)pokemon.effortValues.specialAttack / PokemonInstance.maximumEffortValue,
                    (float)pokemon.effortValues.specialDefense / PokemonInstance.maximumEffortValue,
                    (float)pokemon.effortValues.speed / PokemonInstance.maximumEffortValue,
                    (float)pokemon.effortValues.health / PokemonInstance.maximumEffortValue
                };

                statHexIV.values = new float[]
                {
                    (float)pokemon.individualValues.attack / PokemonInstance.maximumIndividualValue,
                    (float)pokemon.individualValues.defense / PokemonInstance.maximumIndividualValue,
                    (float)pokemon.individualValues.specialAttack / PokemonInstance.maximumIndividualValue,
                    (float)pokemon.individualValues.specialDefense / PokemonInstance.maximumIndividualValue,
                    (float)pokemon.individualValues.speed / PokemonInstance.maximumIndividualValue,
                    (float)pokemon.individualValues.health / PokemonInstance.maximumIndividualValue
                };

            }

        }

    }
}