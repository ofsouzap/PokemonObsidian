using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Revenge : PokemonMove
    {

        public Move_Revenge()
        {

            id = 279; // ID here
            name = "Revenge"; // Full name here
            description = "An attack move that inflicts double the damage if the user has been hurt by the foe in the same turn."; // Description here
            type = Type.Fighting; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 10; // PP here
            power = 60; // Power here
            accuracy = 100; // Accuracy here

            movePriority = false;

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte(base.GetUsagePower(battleData, user, target) * (user.battleProperties.GetDamageThisTurn() > 0 ? 2 : 1)); // Double power if has been hit this turn

    }
}
