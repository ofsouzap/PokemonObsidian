using System;

namespace FreeRoaming.AreaEntranceArguments
{
    public static class PokemonCenterEntranceArguments
    {

        public static bool argumentsSet = false;

        public static void SetArguments(int pokemonCenterId)
        {
            PokemonCenterEntranceArguments.pokemonCenterId = pokemonCenterId;
            argumentsSet = true;
        }

        /// <summary>
        /// An id for the pokemon center to launch. This will be used when launching the Pokemon Center scene
        /// </summary>
        public static int pokemonCenterId = -1;

    }
}
