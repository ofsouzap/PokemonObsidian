using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class MenuPartyController : MonoBehaviour
    {

        public Button buttonBack;

        public MenuButtonPokemonController[] pokemonButtons;

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

                pokemonButtons[i].SetValues(
                    pokemon[i].GetDisplayName(),
                    pokemon[i].LoadSprite(Pokemon.PokemonSpecies.SpriteType.Icon),
                    ((float)pokemon[i].health) / pokemon[i].GetStats().health);

            }

        }

    }
}
