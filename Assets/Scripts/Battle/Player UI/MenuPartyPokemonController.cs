using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

namespace Battle.PlayerUI
{
    public class MenuPartyPokemonController : MenuController
    {

        //I have addded headers to this class since there are so many properties to set in the inspector

        [Header("Buttons")]

        public Button buttonBack;

        public Button buttonNext;
        public Button buttonPrevious;

        public Button buttonSendOut;
        public Button buttonCheckMoves;

        protected override MenuSelectableController[] GetSelectables() => new MenuSelectableController[]
        {
            buttonBack.GetComponent<MenuSelectableController>(),
            buttonNext.GetComponent<MenuSelectableController>(),
            buttonPrevious.GetComponent<MenuSelectableController>(),
            buttonSendOut.GetComponent<MenuSelectableController>(),
            buttonCheckMoves.GetComponent<MenuSelectableController>()
        };

        [Header("Images")]

        public Image imageIcon;
        public Image imageFront;
        public Image imageBack;

        [Header("Details - General")]

        public Image imageType1;
        public Image imageType2;

        public Text textName;

        public Image imageGender;

        public Text textLevelValue;
        
        public Text textItemName;
        public Image imageItemIcon;

        public Text textAbilityName;
        public Text textAbilityDescription;

        public Image imageStatusCondition;

        public Image imageCheatPokemon;
        public Image imageShinyPokemon;

        [Header("Details - Stats")]

        public Text textAttackValue;
        public Text textDefenseValue;
        public Text textSpecialAttackValue;
        public Text textSpecialDefenseValue;
        public Text textSpeedValue;
        public HealthBarScript healthBar;

        public void SetPokemonDetails(PokemonInstance pokemon)
        {

            #region Images

            imageIcon.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Icon);
            imageFront.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1);
            imageBack.sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Back);

            #endregion

            #region General Details

            imageType1.sprite = SpriteStorage.GetTypeSymbolSprite(pokemon.species.type1);

            Pokemon.Type? pokemonType2 = pokemon.species.type2;
            if (pokemonType2 == null)
            {
                imageType2.gameObject.SetActive(false);
            }
            else
            {
                imageType2.gameObject.SetActive(true);
                imageType2.sprite = SpriteStorage.GetTypeSymbolSprite((Pokemon.Type)pokemon.species.type2);
            }

            textName.text = pokemon.GetDisplayName();

            imageGender.sprite = pokemon.LoadGenderSprite();

            textLevelValue.text = pokemon.GetLevel().ToString();

            if (pokemon.heldItem != null)
            {
                textItemName.text = pokemon.heldItem.itemName;
                imageItemIcon.enabled = true;
                imageItemIcon.sprite = pokemon.heldItem.LoadSprite();
            }
            else
            {
                textItemName.text = "No held item";
                imageItemIcon.enabled = false;
            }

            //TODO - set values for ability when and if implemented
            textAbilityName.text = "";
            textAbilityDescription.text = "";

            if (pokemon.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None)
            {

                imageStatusCondition.gameObject.SetActive(true);

                Sprite statusConditionSprite = SpriteStorage.GetNonVolatileStatusConditionSprite(pokemon.nonVolatileStatusCondition);
                if (statusConditionSprite != null)
                {
                    imageStatusCondition.gameObject.SetActive(true);
                    imageStatusCondition.sprite = statusConditionSprite;
                }
                else
                {
                    imageStatusCondition.gameObject.SetActive(false);
                }

            }
            else
            {

                imageStatusCondition.gameObject.SetActive(false);

            }

            imageCheatPokemon.gameObject.SetActive(pokemon.cheatPokemon);
            imageShinyPokemon.gameObject.SetActive(pokemon.IsShiny);

            #endregion

            #region Stats

            Pokemon.Stats<int> stats = pokemon.GetStats();

            textAttackValue.text = stats.attack.ToString();
            textDefenseValue.text = stats.defense.ToString();
            textSpecialAttackValue.text = stats.specialAttack.ToString();
            textSpecialDefenseValue.text = stats.specialDefense.ToString();
            textSpeedValue.text = stats.speed.ToString();
            healthBar.UpdateBar(pokemon.HealthProportion);

            #endregion

        }

    }
}
