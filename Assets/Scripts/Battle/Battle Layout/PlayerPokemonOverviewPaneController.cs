using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;

namespace Battle.BattleLayout
{
    public class PlayerPokemonOverviewPaneController : PokemonOverviewPaneController
    {

        public Text textHealthAmount;

        public const string healthAmountSeparator = "/";

        [Tooltip("The game object which has the image for the player's pokemon's experience bar to be stretched as needed")]
        public GameObject gameObjectExperienceBar;

        private void Start()
        {

            if (gameObjectExperienceBar.GetComponent<Image>() == null)
            {
                Debug.LogError("No Image component found for experience bar");
            }

            if (gameObjectExperienceBar.GetComponent<RectTransform>() == null)
            {
                Debug.LogError("No RectTransform component found for experience bar");
            }

        }

        public override void FullUpdate(PokemonInstance pokemon)
        {

            base.FullUpdate(pokemon);

            #region Experience Bar

            float experienceBarValue;

            if (pokemon.GetLevel() < 100)
            {

                int pokemonCurrentLevelExperience = GrowthTypeData.GetMinimumExperienceForLevel(pokemon.GetLevel(), pokemon.growthType);
                int pokemonNextLevelExperience = GrowthTypeData.GetMinimumExperienceForLevel((byte)(pokemon.GetLevel() + 1), pokemon.growthType);

                //Find the proportion of the way that the pokemon is between its level and the next level
                experienceBarValue = Mathf.InverseLerp(
                    pokemonCurrentLevelExperience,
                    pokemonNextLevelExperience,
                    pokemon.experience);

            }
            else
            {
                experienceBarValue = 1;
            }

            UpdateExperienceBar(experienceBarValue);

            #endregion

        }

        public override void UpdateHealthBar(int amount, int maxValue)
        {

            base.UpdateHealthBar(amount, maxValue);

            textHealthAmount.text = amount.ToString() + healthAmountSeparator + maxValue.ToString();

        }

        public void UpdateExperienceBar(float value)
        {

            if (value < 0 || value > 1)
            {
                Debug.LogError("Value out of range (" + value + ")");
                return;
            }

            gameObjectExperienceBar.GetComponent<RectTransform>().anchorMax = new Vector2(
                value,
                gameObjectExperienceBar.GetComponent<RectTransform>().anchorMax.y
                );

        }

    }
}
