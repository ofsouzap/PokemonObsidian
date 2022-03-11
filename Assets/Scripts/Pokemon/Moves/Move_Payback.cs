using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Payback : PokemonMove
    {

        public Move_Payback()
        {

            id = 371; // ID here
            name = "Payback"; // Full name here
            description = "If the user can use this attack after the foe attacks, its power is doubled."; // Description here
            type = Type.Dark; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 10; // PP here
            power = 50; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte(base.GetUsagePower(battleData, user, target) * (user.battleProperties.GetDamageThisTurn() > 0 ? 2 : 1));

    }
}
