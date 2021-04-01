using Battle;
using Pokemon;

namespace Items.PokeBalls
{
    public class NetBall : PokeBall
    {

        private static bool CheckPokemonSpeciesQualifies(PokemonSpecies pokemon)
        {

            if (pokemon.type1 == Type.Water)
                return true;

            if (pokemon.type1 == Type.Bug)
                return true;

            if (pokemon.type2 == Type.Water)
                return true;

            if (pokemon.type2 == Type.Bug)
                return true;

            return false;

        }

        public override float GetCatchChanceModifier(PokemonInstance target, BattleData battleData)
            => CheckPokemonSpeciesQualifies(target.species) ? 3.5F : 1;

    }
}
