using Pokemon;
using Battle;

namespace Items.PokeBalls
{
    public class BasicPokeBall : PokeBall
    {

        public float catchRateModifier;

        public override float GetCatchChanceModifier(PokemonInstance target, BattleData battleData)
            => catchRateModifier;

    }
}
