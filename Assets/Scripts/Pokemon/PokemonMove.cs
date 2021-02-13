using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pokemon;

namespace Pokemon
{
    public class PokemonMove
    {

        //TODO - have registry always sorted by id
        public static PokemonMove[] registry;

        public static PokemonMove GetPokemonMoveById(int id)
        {

            //TODO - code a binary search
            return registry.First((x) => x.id == id);

        }

        public int id;

        //TODO

    }
}