﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class MenuPartyController : MenuController
    {

        public Button buttonBack;

        public MenuButtonPokemonController[] pokemonButtons;

        protected override MenuSelectableController[] GetSelectables()
        {
            MenuSelectableController[] output = new MenuSelectableController[pokemonButtons.Length + 1];
            Array.Copy(
                pokemonButtons,
                output,
                pokemonButtons.Length);
            output[output.Length - 1] = buttonBack.GetComponent<MenuSelectableController>();
            return output;
        }

        public void SetUp()
        {
            
            if (pokemonButtons.Length != 6)
            {
                Debug.LogError("Non-6-length pokemon buttons array");
                return;
            }

        }

        public void RefreshButtons()
        {

            Pokemon.PokemonInstance[] pokemon = PlayerData.singleton.partyPokemon;

            for (int i = 0; i < pokemon.Length; i++)
            {

                if (pokemon[i] == null)
                {
                    pokemonButtons[i].gameObject.SetActive(false);
                    continue;
                }

                pokemonButtons[i].SetValues(
                    pokemon[i].GetDisplayName(),
                    pokemon[i].LoadSprite(Pokemon.PokemonSpecies.SpriteType.Icon),
                    ((float)pokemon[i].health) / pokemon[i].GetStats().health);

            }

        }

    }
}
