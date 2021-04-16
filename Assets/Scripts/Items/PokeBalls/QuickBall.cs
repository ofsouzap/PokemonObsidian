using Battle;
using Pokemon;

namespace Items.PokeBalls
{
    public class QuickBall : PokeBall
    {

        public override float GetCatchChanceModifier(PokemonInstance target, BattleData battleData)
        {
            if (battleData.battleTurnNumber == 0)
                return 4;
            else
                return 1;
        }

    }
}
