using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Avalanche : PokemonMove
    {

        public Move_Avalanche()
        {

            id = 419; // ID here
            name = "Avalanche"; // Full name here
            description = "An attack move that inflicts double the damage if the user has been hurt by the foe in the same turn."; // Description here
            type = Type.Ice; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 10; // PP here
            power = 60; // Power here
            accuracy = 100; // Accuracy here
            movePriority = false;

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte(base.GetUsagePower(battleData, user, target) * (user.battleProperties.GetDamageThisTurn() > 0 ? 2 : 1));

    }
}
