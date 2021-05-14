using System;

namespace FreeRoaming.AreaEntranceArguments
{
    public static class PokeMartEntranceArguments
    {

        public static bool argumentsSet = false;

        public static void SetArguments(int pokeMartId)
        {
            PokeMartEntranceArguments.pokeMartId = pokeMartId;
            argumentsSet = true;
        }

        /// <summary>
        /// An id for the pokemon center to launch. This will be used when launching the Poke Mart scene
        /// </summary>
        public static int pokeMartId = 0;

    }
}
