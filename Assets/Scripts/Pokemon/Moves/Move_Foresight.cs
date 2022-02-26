using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Foresight : PokemonMove
    {

        public Move_Foresight()
        {

            id = 193; // ID here
            name = "Foresight"; // Full name here
            description = "Enables the user to hit an evasive foe."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 40; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override bool GetInflictsIdentified(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
