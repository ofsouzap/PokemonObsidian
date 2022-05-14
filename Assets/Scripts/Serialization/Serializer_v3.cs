using Pokemon;
using System.IO;

namespace Serialization
{
    public class Serializer_v3 : Serializer_v2
    {

        public override ushort GetVersionCode()
            => 0x0003;

        public override void SerializePokemonInstance(Stream stream, PokemonInstance pokemon)
        {

            base.SerializePokemonInstance(stream, pokemon);

            SerializeBool(stream, pokemon.IsShiny);

        }

        public override PokemonInstance DeserializePokemonInstance(Stream stream)
        {

            PokemonInstance pmon = base.DeserializePokemonInstance(stream);

            bool shinyPokemon = DeserializeBool(stream);
            
            pmon.SetIsShiny(shinyPokemon);

            return pmon;

        }

    }
}
