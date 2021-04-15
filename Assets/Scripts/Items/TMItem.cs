﻿using System.Linq;
using System.Collections.Generic;
using Pokemon;
using Pokemon.Moves;

namespace Items
{
    public class TMItem : Item
    {

        #region Registry

        public static TMItem GetTMItemById(int id,
            bool addTypeId = false)
        {
            int queryId = addTypeId ? id + typeIdTM : id;
            return (TMItem)registry.LinearSearch(queryId);
        }

        public static Item[] GetRegistryItems()
        {

            List<TMItem> tmItems = new List<TMItem>();

            foreach (PokemonMove move in PokemonMove.registry)
            {

                tmItems.Add(new TMItem
                {
                    id = typeIdTM + move.id,
                    moveId = move.id,
                    itemName = "TM" + move.id + ' ' + move.name,
                    description = "Teaches a pokemon " + move.description
                }); //TODO - once TM itemsprite ready, set resourceName for each generated TM

            }

            return tmItems.ToArray();

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
