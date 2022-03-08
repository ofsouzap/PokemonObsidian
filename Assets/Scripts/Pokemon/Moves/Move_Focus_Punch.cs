using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Focus_Punch : PokemonMove
    {

        public Move_Focus_Punch()
        {

            id = 264; // ID here
            name = "Focus Punch"; // Full name here
            description = "	The user focuses its mind before launching a punch. It will fail if the user is hit before it is used"; // Description here
            type = Type.Fighting; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 20; // PP here
            power = 150; // Power here
            accuracy = 100; // Accuracy here

            movePriority = false;

        }

        public override bool RunFailureCheck(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => base.RunFailureCheck(battleData, user, target) || user.battleProperties.GetDamageThisTurn() > 0;

    }
}
