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

            PlayerData.singleton.partyPokemon = new PokemonInstance[]
            {
                PokemonFactory.GenerateWild(new int[] { 7 }, 90, 100),
                PokemonFactory.GenerateWild(new int[] { 4 }, 60, 60),
                PokemonFactory.GenerateWild(new int[] { 6 }, 14, 14),
                PokemonFactory.GenerateWild(new int[] { 1 }, 100, 100),
                PokemonFactory.GenerateWild(new int[] { 12 }, 1, 100)
            };

            PlayerData.singleton.profile.name = "Test";

        }

    }
}

#endif