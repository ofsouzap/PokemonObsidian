using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Spit_Up : PokemonMove
    {

        public Move_Spit_Up()
        {

            id = 255; // ID here
            name = "Spit Up"; // Full name here
            description = "The power stored using the move Stockpile is released at once in an attack."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override bool RunFailureCheck(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => base.RunFailureCheck(battleData, user, target) || user.battleProperties.volatileBattleStatus.stockpileAmount <= 0;

        public override bool GetResetStockpile(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

        public override float CalculateRandomModifier(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => 1; // No random modifier to damage output

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte(user.battleProperties.volatileBattleStatus.stockpileAmount * 85);

    }
}
