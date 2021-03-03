﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class MenuPartyController : MonoBehaviour
    {

        public Button buttonBack;

        public Button[] pokemonButtons;
        private Text[] pokemonButtonTexts;
        private Image[] pokemonButtonImages;
        private HealthBarScript[] pokemonButtonHealthBars;

        private void Start()
        {
            
            if (pokemonButtons.Length != 6)
            {
                Debug.LogError("Non-6-length pokemon buttons array");
                return;
            }

            for (int i = 0; i < pokemonButtons.Length; i++)
            {

                Button _button = pokemonButtons[i];

                Text _text = _button.GetComponentInChildren<Text>();
                Image _image = _button.GetComponentInChildren<Image>();
                HealthBarScript _healthBar = _button.GetComponentInChildren<HealthBarScript>();

                if (_text == null || _image == null || _healthBar == null)
                {
                    Debug.LogError("Invalid pokemon button child format for index " + i);
                }

                pokemonButtonTexts[i] = _text;
                pokemonButtonImages[i] = _image;
                pokemonButtonHealthBars[i] = _healthBar;

            }

        }

        public struct PokemonButtonProperties { public bool isSet; public string name; public string iconPath; public float healthProportion; }

        public void SetPokemonButtonProperties(PokemonButtonProperties[] properties)
        {

            if (properties.Length != pokemonButtons.Length)
            {
                Debug.LogError("Invalid properties length for setting pokemon button properties");
                return;
            }

            for (int i = 0; i < properties.Length; i++)
            {

                if (properties[i].isSet)
                {

                    pokemonButtonTexts[i].gameObject.SetActive(true);
                    pokemonButtonImages[i].gameObject.SetActive(true);
                    pokemonButtonHealthBars[i].gameObject.SetActive(true);

                    pokemonButtonTexts[i].text = properties[i].name;
                    pokemonButtonImages[i].sprite = (Sprite)Resources.Load(properties[i].iconPath);
                    pokemonButtonHealthBars[i].UpdateBar(properties[i].healthProportion);

                }
                else
                {

                    pokemonButtonTexts[i].gameObject.SetActive(false);
                    pokemonButtonImages[i].gameObject.SetActive(false);
                    pokemonButtonHealthBars[i].gameObject.SetActive(false);

                }

            }

        }

    }
}
