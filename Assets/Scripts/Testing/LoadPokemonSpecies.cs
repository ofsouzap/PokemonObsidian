using UnityEngine;
using Pokemon;

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
