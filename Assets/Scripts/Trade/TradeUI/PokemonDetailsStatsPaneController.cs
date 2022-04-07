using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Menus;
using Pokemon;

namespace Trade.TradeUI
{
    public class PokemonDetailsStatsPaneController : PokemonDetailsPaneController
    {

        public StatsHex evHex;
        public StatsHex ivHex;

        public Text textAttackValue;
        public Text textDefenseValue;
        public Text textSpecialAttackValue;
        public Text textSpecialDefenseValue;
        public Text textSpeedValue;
        public Text textHealthValue;

        public override void SetPokemon(PokemonInstance pokemon)
        {

            if (pokemon != null)
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
                textHealthValue.text = pokemon.GetStats().health.ToString();

            }

            else
            {

                evHex.values = new float[6] { 0, 0, 0, 0, 0, 0 };
                evHex.SetVerticesDirty();

                ivHex.values = new float[6] { 0, 0, 0, 0, 0, 0 };
                ivHex.SetVerticesDirty();

                textAttackValue.text = "";
                textDefenseValue.text = "";
                textSpecialAttackValue.text = "";
                textSpecialDefenseValue.text = "";
                textSpeedValue.text = "";
                textHealthValue.text = "";

            }

        }

    }
}