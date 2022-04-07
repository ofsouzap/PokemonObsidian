using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu.DetailsPanes
{
    public class StatsPane : DetailsPane
    {

        public StatsHex evHex;
        public StatsHex ivHex;

        public Text textAttackValue;
        public Text textDefenseValue;
        public Text textSpecialAttackValue;
        public Text textSpecialDefenseValue;
        public Text textSpeedValue;
        public Text textHealthValue;

        public Text textLevel;
        public Text textExperience;
        public Text textExperienceToNextLevel;

        public override void RefreshDetails(PokemonInstance pokemon)
        {
            
            evHex.values = new float[]
            {
                (float)pokemon.effortValues.attack / PokemonInstance.maximumEffortValue,
                (float)pokemon.effortValues.defense / PokemonInstance.maximumEffortValue,
                (float)pokemon.effortValues.specialAttack / PokemonInstance.maximumEffortValue,
                (float)pokemon.effortValues.specialDefense / PokemonInstance.maximumEffortValue,
                (float)pokemon.effortValues.speed / PokemonInstance.maximumEffortValue,
                (float)pokemon.effortValues.health / PokemonInstance.maximumEffortValue
            };

            ivHex.values = new float[]
            {
                (float)pokemon.individualValues.attack / PokemonInstance.maximumIndividualValue,
                (float)pokemon.individualValues.defense / PokemonInstance.maximumIndividualValue,
                (float)pokemon.individualValues.specialAttack / PokemonInstance.maximumIndividualValue,
                (float)pokemon.individualValues.specialDefense / PokemonInstance.maximumIndividualValue,
                (float)pokemon.individualValues.speed / PokemonInstance.maximumIndividualValue,
                (float)pokemon.individualValues.health / PokemonInstance.maximumIndividualValue
            };

            textAttackValue.text = pokemon.GetStats().attack.ToString();
            textDefenseValue.text = pokemon.GetStats().defense.ToString();
            textSpecialAttackValue.text = pokemon.GetStats().specialAttack.ToString();
            textSpecialDefenseValue.text = pokemon.GetStats().specialDefense.ToString();
            textSpeedValue.text = pokemon.GetStats().speed.ToString();
            textHealthValue.text = pokemon.health + "/" + pokemon.GetStats().health;

            byte pokemonLevel = pokemon.GetLevel();
            textLevel.text = pokemonLevel.ToString();
            textExperience.text = pokemon.experience.ToString();

            if (pokemonLevel < 100)
                textExperienceToNextLevel.text = (GrowthTypeData.GetMinimumExperienceForLevel((byte)(pokemonLevel + 1), pokemon.species.growthType) - pokemon.experience).ToString();
            else
                textExperienceToNextLevel.text = "-";

        }

    }
}
