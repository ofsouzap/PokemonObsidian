using UnityEngine;
using Pokemon;

#if UNITY_EDITOR

namespace Testing
{
    public class LoadPokemonSpecies : MonoBehaviour
    {

        private void Awake()
        {

            PokemonSpeciesData.LoadData();

        }

    }
}

#endif