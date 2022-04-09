using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Brine : PokemonMove
    {

        public Move_Brine()
        {

            id = 363; // ID here
            name = "Brine"; // Full name here
            description = "If the foe's HP is down to about half, this attack will hit with double the power."; // Description here
            type = Type.Water; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 10; // PP here
            power = 65; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte(base.GetUsagePower(battleData, user, target)
                * ((target.HealthProportion <= 0.5F) ? 2 : 1)); // Double if target health less than or equal to half of their max health

    }
}
