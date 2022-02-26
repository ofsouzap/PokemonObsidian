using System.Collections.Generic;
using Battle;

namespace Pokemon.Moves
{
    public class Move_Petal_Dance : PokemonMove
    {

        public Move_Petal_Dance()
        {

            id = 80;
            name = "Petal Dance";
            description = "The user attacks by scattering petals for two to three turns. The user then becomes confused.";
            maxPP = 20;
            power = 90;
            accuracy = 100;
            type = Type.Grass;
            moveType = MoveType.Special;

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
