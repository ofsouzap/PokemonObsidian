using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Yawn : PokemonMove
    {

        public Move_Yawn()
        {

            id = 281; // ID here
            name = "Yawn"; // Full name here
            description = "The user lets loose a huge yawn that lulls the foe into falling asleep on the next turn."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override bool GetInflictsDrowsy(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
