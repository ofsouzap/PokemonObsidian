using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Facade : PokemonMove
    {

        public Move_Facade()
        {

            id = 263; // ID here
            name = "Facade"; // Full name here
            description = "An attack move that doubles its power if the user is poisoned, paralyzed, or has a burn."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 20; // PP here
            power = 70; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte(base.GetUsagePower(battleData, user, target) * user.nonVolatileStatusCondition switch
            {
                PokemonInstance.NonVolatileStatusCondition.Burn => 2,
                PokemonInstance.NonVolatileStatusCondition.Paralysed => 2,
                PokemonInstance.NonVolatileStatusCondition.Poisoned => 2,
                PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned => 2,
                _ => 1
            }); // Double power if burnt, paralysed, poisoned or badly poisoned

    }
}
