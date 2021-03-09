using System;
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

            PlayerData playerData = PlayerData.singleton;
            Pokemon.PokemonInstance[] pokemon = playerData.partyPokemon;

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

            #region Navigation

            Selectable[] buttonDownTargets = new Selectable[4];
            Button[] buttons = pokemonButtons.Select(x => x.GetComponent<Button>()).ToArray();
            Selectable buttonBackUpTarget = buttons[4];

            #region Setting Button Down Targets

            if (playerData.GetNumberOfPartyPokemon() > 2)
            {
                buttonDownTargets[0] = buttons[2];
                buttonDownTargets[1] = buttons[3];
            }
            else
            {
                buttonDownTargets[0] = buttonDownTargets[1] = buttonBack;
            }

            if (playerData.GetNumberOfPartyPokemon() > 4)
            {
                buttonDownTargets[2] = buttons[4];
                buttonDownTargets[3] = buttons[5];
            }
            else
            {
                buttonDownTargets[2] = buttonDownTargets[3] = buttonBack;
            }

            #endregion

            #region Setting Back Button Up Target

            if (playerData.GetNumberOfPartyPokemon() <= 2)
            {
                buttonBackUpTarget = buttons[0];
            }
            else if (playerData.GetNumberOfPartyPokemon() <= 4)
            {
                buttonBackUpTarget = buttons[2];
            }
            else
            {
                buttonBackUpTarget = buttons[4];
            }

            #endregion

            #region Setting Button Navigations

            buttons[0].navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[0].navigation.selectOnUp,
                selectOnRight = buttons[0].navigation.selectOnRight,
                selectOnLeft = buttons[0].navigation.selectOnLeft,
                selectOnDown = buttonDownTargets[0]
            };

            buttons[1].navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[1].navigation.selectOnUp,
                selectOnRight = buttons[1].navigation.selectOnRight,
                selectOnLeft = buttons[1].navigation.selectOnLeft,
                selectOnDown = buttonDownTargets[1]
            };

            buttons[2].navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[2].navigation.selectOnUp,
                selectOnRight = buttons[2].navigation.selectOnRight,
                selectOnLeft = buttons[2].navigation.selectOnLeft,
                selectOnDown = buttonDownTargets[2]
            };

            buttons[3].navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[3].navigation.selectOnUp,
                selectOnRight = buttons[3].navigation.selectOnRight,
                selectOnLeft = buttons[3].navigation.selectOnLeft,
                selectOnDown = buttonDownTargets[3]
            };

            #region Back Button Navigation

            buttonBack.navigation = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttonBackUpTarget,
                selectOnRight = buttonBack.navigation.selectOnRight,
                selectOnLeft = buttonBack.navigation.selectOnLeft,
                selectOnDown = buttonBack.navigation.selectOnDown
            };

            #endregion

            #endregion

            #endregion

        }

    }
}
