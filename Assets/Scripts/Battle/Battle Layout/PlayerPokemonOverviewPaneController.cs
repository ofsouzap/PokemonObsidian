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

        public ExperienceBarScript experienceBar;

        public override void FullUpdate(PokemonInstance pokemon)
        {

            base.FullUpdate(pokemon);

            UpdateExperienceBar(pokemon);

        }

        public override void UpdateHealthBar(int amount, int maxValue)
        {

            base.UpdateHealthBar(amount, maxValue);

            textHealthAmount.text = amount.ToString() + healthAmountSeparator + maxValue.ToString();

        }

        public void UpdateExperienceBar(PokemonInstance pokemon)
        {

            if (pokemon.GetLevel() < 100)
            {

                int pokemonCurrentLevelExperience = GrowthTypeData.GetMinimumExperienceForLevel(pokemon.GetLevel(), pokemon.growthType);
                int pokemonNextLevelExperience = GrowthTypeData.GetMinimumExperienceForLevel((byte)(pokemon.GetLevel() + 1), pokemon.growthType);

                UpdateExperienceBar(pokemon.experience, pokemonCurrentLevelExperience, pokemonNextLevelExperience);

            }
            else
            {
                UpdateExperienceBar(1);
            }

        }

        public void UpdateExperienceBar(int pokemonExperience,
            int currentLevelMinimumExperience,
            int nextLevelMiniumExperience)
            => UpdateExperienceBar(
                Mathf.InverseLerp(
                    currentLevelMinimumExperience,
                    nextLevelMiniumExperience,
                    pokemonExperience
                )
            );

        public void UpdateExperienceBar(float value)
        {

            experienceBar.UpdateBar(value);

        }

    }
}
