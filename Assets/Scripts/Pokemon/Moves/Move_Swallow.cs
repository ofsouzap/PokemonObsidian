using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Swallow : PokemonMove
    {

        public Move_Swallow()
        {

            id = 256; // ID here
            name = "Swallow"; // Full name here
            description = "The power stored using the move Stockpile is absorbed by the user to heal its HP."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override bool RunFailureCheck(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => base.RunFailureCheck(battleData, user, target) || user.battleProperties.volatileBattleStatus.stockpileAmount <= 0;

        public override float GetUserMaxHealthRelativeHealthHealed(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => user.battleProperties.volatileBattleStatus.stockpileAmount switch
            {
                1 => 0.25F,
                2 => 0.5F,
                3 => 1F,
                _ => 0F
            };

        public override bool GetResetStockpile(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
