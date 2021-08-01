using System;
using System.Collections;
using Pokemon;

namespace StartUp.ChoosePlayerData.ChoosePlayerStarterPokemon
{
    public class ChoosePlayerStarterPokemonController : GenericImageOptionsChoiceController<int>
    {

        public const byte starterPokemonInitialLevel = 5;

        protected override void SetPlayerDataValue(int value)
        {

            //Deletes all the pokemon
            PlayerData.singleton.ClearAllPokemon();

            //Generate the starter pokemon
            PokemonInstance starterPokemon = PokemonFactory.GenerateWild(
                new int[] { value },
                starterPokemonInitialLevel,
                starterPokemonInitialLevel);

            //Adds the starter pokemon
            PlayerData.singleton.AddNewPartyPokemon(starterPokemon);

        }

    }
}