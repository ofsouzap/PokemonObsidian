using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus.PokedexMenu
{
    public class StatsAreaController : MonoBehaviour
    {

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

        }

        public void SetUnseenSpecies()
        {

            baseStatsHex.values = new float[6] { 0, 0, 0, 0, 0, 0 };

        }

    }
}