using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Attract : PokemonMove
    {

        public Move_Attract()
        {

            id = 213; // ID here
            name = "Attract"; // Full name here
            description = "The foe becomes infatuated and less likely to attack."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 15; // PP here
            power = 0; // Power here
            accuracy = 70; // Accuracy here

            // Instead of requiring target to be of opposite gender, accuracy of move has been reduced

        }

        public override bool GetInflictsInfatuated(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
