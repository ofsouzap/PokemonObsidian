using Battle;
using Pokemon;

namespace Items.PokeBalls
{
    public class TimerBall : PokeBall
    {

        public const float maximumCatchChangeModifier = 4;

        public override float GetCatchChanceModifier(PokemonInstance target, BattleData battleData)
        {

            float calculatedModifier = (battleData.battleTurnNumber + 10) / (float)10;

            if (calculatedModifier > maximumCatchChangeModifier)
                return maximumCatchChangeModifier;
            else
                return calculatedModifier;

        }

    }
}
