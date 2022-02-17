using System.Collections.Generic;
using Battle;

namespace Pokemon.Moves
{
    public class Move_Thrash : PokemonMove
    {

        public Move_Thrash()
        {

            id = 37;
            name = "Thrash";
            description = "The user rampages and attacks for two to three turns. It then becomes confused, however.";
            maxPP = 20;
            power = 90;
            accuracy = 100;
            type = Type.Normal;
            moveType = MoveType.Physical;

        }

        public override void DoThrashingUpdate(ref UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            base.DoThrashingUpdate(ref usageResults, user, target, battleData);

            //Only set thrashing if not already thrashing
            if (user.battleProperties.volatileBattleStatus.thrashTurns <= 0)
                usageResults.thrashingTurns = PokemonInstance.BattleProperties.VolatileBattleStatus.GetRandomThrashingDuration(battleData);

        }

    }
}
