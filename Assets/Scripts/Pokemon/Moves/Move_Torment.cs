using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Torment : PokemonMove
    {

        public Move_Torment()
        {

            id = 259; // ID here
            name = "Torment"; // Full name here
            description = "The user torments and enrages the foe, making it incapable of using the same move twice in a row."; // Description here
            type = Type.Dark; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 15; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override bool GetInflictsTorment(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
