using UnityEngine;
using Pokemon;

#if UNITY_EDITOR

namespace Testing
{
    public class LoadPokemonMoves : MonoBehaviour
    {

        private void Awake()
        {

            PokemonMoveData.LoadData();

        }

    }
}

#endif