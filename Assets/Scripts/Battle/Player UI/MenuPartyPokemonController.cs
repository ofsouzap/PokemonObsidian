using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class MenuPartyPokemonController : MonoBehaviour
    {

        //I have addded headers to this class since there are so many properties to set in the inspector

        [Header("Buttons")]

        public Button buttonBack;

        public Button buttonNext;
        public Button buttonPrevious;

        public Button buttonSendOut;
        public Button buttonCheckMoves;

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

        [Header("Details - Stats")]

        public Text textAttackValue;
        public Text textDefenseValue;
        public Text textSpecialAttackValue;
        public Text textSpecialDefenseValue;
        public Text textSpeedValue;
        public HealthBarScript healthBar;

        public void SetPokemonDetails(Pokemon.PokemonInstance pokemon)
        {

            #region Images

            imageIcon.sprite = pokemon.LoadSprite(Pokemon.PokemonSpecies.SpriteType.Icon);
            imageFront.sprite = pokemon.LoadSprite(Pokemon.PokemonSpecies.SpriteType.Front1);
            imageBack.sprite = pokemon.LoadSprite(Pokemon.PokemonSpecies.SpriteType.Back);

            #endregion

            #region General Details

            textName.text = pokemon.GetDisplayName();

            imageGender.sprite = pokemon.LoadGenderSprite();

            textLevelValue.text = pokemon.GetLevel().ToString();

            //TODO - set values for items once implemented

            //TODO - set values for ability when and if implemented

            Sprite statusConditionSprite = Pokemon.PokemonInstance.LoadNonVolatileStatusConditionSprite(pokemon.nonVolatileStatusCondition);
            if (statusConditionSprite != null)
            {
                imageStatusCondition.gameObject.SetActive(true);
                imageStatusCondition.sprite = statusConditionSprite;
            }
            else
            {
                imageStatusCondition.gameObject.SetActive(false);
            }

            #endregion

            #region Stats

            Pokemon.Stats<int> stats = pokemon.GetStats();

            textAttackValue.text = stats.attack.ToString();
            textDefenseValue.text = stats.defense.ToString();
            textSpecialAttackValue.text = stats.specialAttack.ToString();
            textSpecialDefenseValue.text = stats.specialDefense.ToString();
            textSpeedValue.text = stats.speed.ToString();
            healthBar.UpdateBar(((float)pokemon.health) / pokemon.species.baseStats.health);

            #endregion

        }

    }
}
