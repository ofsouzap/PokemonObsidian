using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;
using Menus;

namespace Battle.PlayerUI
{
    public class PlayerPokemonSelectUIController : MenuController
    {

        public BattleParticipantPlayer playerBattleParticipant;

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

            pokemonButtons[0].Button.onClick.AddListener(() => { playerBattleParticipant.PokemonSelectUISelectPokemon(0); });

            pokemonButtons[1].Button.onClick.AddListener(() => { playerBattleParticipant.PokemonSelectUISelectPokemon(1); });

            pokemonButtons[2].Button.onClick.AddListener(() => { playerBattleParticipant.PokemonSelectUISelectPokemon(2); });

            pokemonButtons[3].Button.onClick.AddListener(() => { playerBattleParticipant.PokemonSelectUISelectPokemon(3); });

            pokemonButtons[4].Button.onClick.AddListener(() => { playerBattleParticipant.PokemonSelectUISelectPokemon(4); });

            pokemonButtons[5].Button.onClick.AddListener(() => { playerBattleParticipant.PokemonSelectUISelectPokemon(5); });

        }

        public void RefreshButtons()
        {

            Pokemon.PokemonInstance[] pokemon = playerBattleParticipant.GetPokemon();

            for (int i = 0; i < pokemon.Length; i++)
            {

                if (pokemon[i] == null)
                {
                    pokemonButtons[i].SetInteractable(false);
                }
                else
                {

                    pokemonButtons[i].SetInteractable(true);

                    pokemonButtons[i].SetValues(
                        pokemon[i].GetDisplayName(),
                        pokemon[i].LoadSprite(Pokemon.PokemonSpecies.SpriteType.Icon),
                        ((float)pokemon[i].health) / pokemon[i].GetStats().health,
                        pokemon[i].nonVolatileStatusCondition,
                        pokemon[i].heldItem != null
                    );

                }

            }

        }

        public void ShowBackButton()
        {
            buttonBack.gameObject.SetActive(true);
        }

        public void HideBackButton()
        {
            buttonBack.gameObject.SetActive(false);
        }

    }
}