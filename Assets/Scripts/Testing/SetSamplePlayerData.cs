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
                PokemonFactory.GenerateWild(new int[] { 15 }, 60, 60),
                PokemonFactory.GenerateWild(new int[] { 18 }, 60, 60),
                PokemonFactory.GenerateWild(new int[] { 20 }, 60, 60),
                PokemonFactory.GenerateWild(new int[] { 22 }, 60, 60)
            };

            PlayerData.singleton.profile.name = "Test";

        }

    }
}

#endif