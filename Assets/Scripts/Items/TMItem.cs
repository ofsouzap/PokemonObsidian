using System.Linq;
using System.Collections.Generic;
using Pokemon;
using Pokemon.Moves;

namespace Items
{
    public class TMItem : Item
    {

        #region Registry

        public static Registry<TMItem> registry = new Registry<TMItem>();

        public static TMItem GetTMItemById(int id)
        {
            if (!registrySet)
                CreateRegistry();
            return registry.StartingIndexSearch(id, id - 1);
        }

        private static bool registrySet = false;

        public static void TrySetRegistry()
        {
            if (!registrySet)
                CreateRegistry();
        }

        private static void CreateRegistry()
        {

            List<TMItem> tmItems = new List<TMItem>();

            foreach (PokemonMove move in PokemonMove.registry)
            {

                tmItems.Add(new TMItem
                {
                    id = move.id,
                    moveId = move.id,
                    itemName = "TM" + move.id + ' ' + move.name,
                    description = "Teaches a pokemon " + move.description
                }); //TODO - once TM itemsprite ready, set resourceName for each generated TM

            }

            registry.SetValues(tmItems.ToArray());

            registrySet = true;

        }

        #endregion

        public int moveId;

        public PokemonMove Move => PokemonMove.GetPokemonMoveById(moveId);

        public override ItemUsageEffects GetUsageEffects(PokemonInstance pokemon)
            => null;

        public override bool CheckCompatibility(PokemonInstance pokemon)
            => !pokemon.moveIds.Contains(moveId) && pokemon.species.discMoves.Contains(moveId);

    }
}
