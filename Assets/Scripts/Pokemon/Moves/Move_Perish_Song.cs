using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Perish_Song : PokemonMove
    {

        public Move_Perish_Song()
        {

            id = 195; // ID here
            name = "Perish Song"; // Full name here
            description = "Any Pokémon that hears this song faints in three turns unless it switches out of battle."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override bool GetInflictsPerishSong(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
