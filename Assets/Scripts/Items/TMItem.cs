using System.Linq;
using System.Collections.Generic;
using Pokemon;
using Pokemon.Moves;

namespace Items
{
    public class TMItem : Item
    {

        #region Registry

        /// <summary>
        /// The id of a TM for a move with id x is this value plus x
        /// </summary>
        public const int tmItemStart = 1000;

        public static TMItem GetTMItemById(int id)
        {
            return (TMItem)registry.LinearSearch(id);
        }

        public static Item[] GetRegistryItems()
        {

            List<TMItem> tmItems = new List<TMItem>();

            foreach (PokemonMove move in PokemonMove.registry)
            {

                tmItems.Add(new TMItem
                {
                    id = tmItemStart + move.id,
                    moveId = move.id,
                    itemName = "TM" + move.id + ' ' + move.name,
                    resourceName = "tm_" + TypeFunc.GetTypeResourceName(move.type),
                    description = move.description
                });

            }

            return tmItems.ToArray();

        }

        #endregion

        public static readonly ItemPrices defaultPrices = new ItemPrices(3000, 1500);

        public override bool CanBeUsedFromBag()
            => true;

        public int moveId;

        public PokemonMove Move => PokemonMove.GetPokemonMoveById(moveId);

        public override ItemUsageEffects GetUsageEffects(PokemonInstance pokemon)
            => null;

        public override bool CheckCompatibility(PokemonInstance pokemon)
            => pokemon.CanLearnMove(moveId);

    }
}
