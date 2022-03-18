using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus.PokedexMenu
{
    public class StatsAreaController : MonoBehaviour
    {

        public Text textHeight;
        public Text textWeight;

        public StatsHex baseStatsHex;

        public void SetSpecies(PokemonSpecies species)
        {

            baseStatsHex.values = new float[6]
            {
                species.baseStats.attack / PokemonSpecies.maxBaseStatValue,
                species.baseStats.defense / PokemonSpecies.maxBaseStatValue,
                species.baseStats.specialAttack / PokemonSpecies.maxBaseStatValue,
                species.baseStats.specialDefense / PokemonSpecies.maxBaseStatValue,
                species.baseStats.speed / PokemonSpecies.maxBaseStatValue,
                species.baseStats.health / PokemonSpecies.maxBaseStatValue
            };

            textHeight.text = species.height.ToString();
            textWeight.text = species.weight.ToString();

        }

        public void SetUnseenSpecies()
        {

            baseStatsHex.values = new float[6] { 0, 0, 0, 0, 0, 0 };
            textHeight.text = "";
            textWeight.text = "";

        }

    }
}