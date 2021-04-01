using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;

namespace Battle.BattleLayout
{
    public class PokemonOverviewPaneController : MonoBehaviour
    {

        public HealthBarScript healthBar;
        public Text textName;
        public Image imageGender;
        public Text textLevel;
        public Image imageNonVolatileStatusCondition;

        public const string textLevelPrefix = "Lv";

        public void Hide() => gameObject.SetActive(false);

        /// <summary>
        /// Fully update the values on the overview pane using a specified pokemon
        /// </summary>
        /// <param name="pokemon">The pokemon to use</param>
        public virtual void FullUpdate(PokemonInstance pokemon)
        {

            UpdateHealthBar(pokemon.health, pokemon.GetStats().health);
            UpdateName(pokemon.GetDisplayName());
            UpdateGender(pokemon.gender);
            UpdateLevel(pokemon.GetLevel());
            UpdateNonVolatileStatsCondition(pokemon.nonVolatileStatusCondition);

        }

        public virtual void UpdateHealthBar(int amount, int maxValue)
        {

            //For the parameters, I would have used just one proportion parameter but the PlayerPokemonOverviewPaneController will need access to both for its override method

            healthBar.UpdateBar(amount / (float)maxValue);

        }

        public void UpdateName(string name) => textName.text = name;
        public void UpdateGender(bool? gender) => imageGender.sprite = SpriteStorage.GetGenderSprite(gender);
        public void UpdateLevel(int level) => textLevel.text = textLevelPrefix + level.ToString();

        public void UpdateNonVolatileStatsCondition(PokemonInstance.NonVolatileStatusCondition nvsc)
        {

            if (nvsc == PokemonInstance.NonVolatileStatusCondition.None)
            {
                imageNonVolatileStatusCondition.gameObject.SetActive(false);
            }
            else
            {
                imageNonVolatileStatusCondition.gameObject.SetActive(true);
                imageNonVolatileStatusCondition.sprite = SpriteStorage.GetNonVolatileStatusConditionSprite(nvsc);
            }

        }

    }
}