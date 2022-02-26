using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Snore : PokemonMove
    {

        public Move_Snore()
        {

            id = 173; // ID here
            name = "Snore"; // Full name here
            description = "An attack that can be used only if the user is asleep. The harsh noise may also make the foe flinch."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 15; // PP here
            power = 40; // Power here
            accuracy = 100; // Accuracy here

            flinchChance = 0.3F;

        }

        public override bool RunFailureCheck(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => user.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.Asleep;

    }
}
