using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Stockpile : PokemonMove
    {

        public Move_Stockpile()
        {

            id = 254; // ID here
            name = "Stockpile"; // Full name here
            description = "The user charges up power, and raises both its Defense and Sp. Def. The move can be used three times."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override bool GetIncrementStockpile(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
