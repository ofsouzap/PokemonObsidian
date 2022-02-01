using UnityEngine;
using Pokemon;

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
