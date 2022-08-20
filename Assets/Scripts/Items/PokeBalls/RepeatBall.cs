using Battle;
using Pokemon;
using System;

namespace Items.PokeBalls
{
    public class RepeatBall : PokeBall
    {

        public override float GetCatchChanceModifier(PokemonInstance target, BattleData battleData)
            => PlayerData.singleton.pokedex.GetCaughtCount(target.speciesId) > 0 ? 3 : 1;

    }
}
