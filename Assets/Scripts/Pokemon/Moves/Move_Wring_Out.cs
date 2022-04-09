using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Wring_Out : PokemonMove
    {

        public Move_Wring_Out()
        {

            id = 378; // ID here
            name = "Wring Out"; // Full name here
            description = "The user powerfully wrings the foe. The more HP the foe has, the greater this attack's power."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte(1 + (120 * (target.HealthProportion)));

    }
}
