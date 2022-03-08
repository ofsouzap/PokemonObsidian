using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Eruption : PokemonMove
    {

        public Move_Eruption()
        {

            id = 284; // ID here
            name = "Eruption"; // Full name here
            description = "The user attacks in an explosive fury. The lower the user's HP, the less powerful this attack becomes."; // Description here
            type = Type.Fire; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 5; // PP here
            power = 150; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte((user.health / user.GetBattleStats().health) * 150);

    }
}
