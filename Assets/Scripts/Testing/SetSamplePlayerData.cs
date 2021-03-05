using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;

#if UNITY_EDITOR

namespace Testing
{
    public class SetSamplePlayerData : MonoBehaviour
    {

        private void Start()
        {

            PlayerData.singleton.partyPokemon = new PokemonInstance[6]
            {
                PokemonFactory.GenerateWild(new int[] { 1 }, 60, 74),
                PokemonFactory.GenerateWild(new int[] { 2 }, 1, 1),
                PokemonFactory.GenerateWild(new int[] { 3 }, 100, 100),
                PokemonFactory.GenerateWild(new int[] { 5 }, 90, 100),
                PokemonFactory.GenerateWild(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 1, 100),
                PokemonFactory.GenerateWild(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 10, 14)
            };

            PlayerData.singleton.profile.name = "Test";

        }

    }
}

#endif