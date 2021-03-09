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
                PokemonFactory.GenerateWild(new int[] { 1 }, 60, 74),
                PokemonFactory.GenerateWild(new int[] { 4 }, 1, 1),
                //PokemonFactory.GenerateWild(new int[] { 3 }, 100, 100),
                //PokemonFactory.GenerateWild(new int[] { 7 }, 90, 100)
            };

            PlayerData.singleton.profile.name = "Test";

        }

    }
}

#endif