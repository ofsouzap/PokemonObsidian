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
                PokemonFactory.GenerateWild(new int[] { 1, 2, 3, 4, 5 }, 10, 14),
                PokemonFactory.GenerateWild(new int[] { 1, 2, 3, 4, 5 }, 10, 14),
                PokemonFactory.GenerateWild(new int[] { 1, 2, 3, 4, 5 }, 10, 14),
                PokemonFactory.GenerateWild(new int[] { 1, 2, 3, 4, 5 }, 10, 14),
                PokemonFactory.GenerateWild(new int[] { 1, 2, 3, 4, 5 }, 10, 14),
                PokemonFactory.GenerateWild(new int[] { 1, 2, 3, 4, 5 }, 10, 14)
            };

            PlayerData.singleton.profile.name = "Test";

        }

    }
}

#endif