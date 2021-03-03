﻿using System;
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

        public Image imageMini;
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

        [Header("Details - Stats")]

        public Text textAttackValue;
        public Text textDefenseValue;
        public Text textSpecialAttackValue;
        public Text textSpecialDefenseValue;
        public Text textSpeedValue;
        public HealthBarOldScript healthBar;

        public void SetPokemonDetails(Pokemon.PokemonInstance pokemon)
        {

            #region Images

            //TODO - set images once method made to get a pokemon species' images

            #endregion

            #region General Details

            textName.text =
                pokemon.nickname == ""
                ? pokemon.species.name
                : pokemon.nickname;

            //TODO - set images for gender once method to get them made

            textLevelValue.text = pokemon.GetLevel().ToString();

            //TODO - set values for items once implemented

            //TODO - set values for ability when and if implemented

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
