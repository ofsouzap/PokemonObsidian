using UnityEngine;
using Pokemon;

#if UNITY_EDITOR

namespace Testing
{
    public class LoadPokemonMoves : MonoBehaviour
    {

        private void Start()
        {

            PokemonMoveData.LoadData();

        }

    }
}

#endif