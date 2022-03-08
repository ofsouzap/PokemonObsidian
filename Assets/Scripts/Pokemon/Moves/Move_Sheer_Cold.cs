using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Sheer_Cold : PokemonMove
    {

        public Move_Sheer_Cold()
        {

            id = 329; // ID here
            name = "Sheer Cold"; // Full name here
            description = "The foe is attacked with a blast of absolute-zero cold. The foe instantly faints if it hits."; // Description here
            type = Type.Ice; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 30; // Accuracy here

            isInstantKO = true;

        }

        public override ushort CalculateAccuracyValue(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => Convert.ToUInt16(user.GetLevel() - target.GetLevel() + 30);

    }
}
