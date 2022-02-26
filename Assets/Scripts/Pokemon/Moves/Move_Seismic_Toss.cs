using Battle;

namespace Pokemon.Moves
{
    public class Move_Seismic_Toss : PokemonMove
    {

        public Move_Seismic_Toss()
        {

            id = 69;
            name = "Seismic Toss";
            description = "The target is thrown using the power of gravity. It inflicts damage equal to the user's level";
            type = Type.Fighting;
            moveType = MoveType.Physical;
            maxPP = 20;
            power = 0;
            accuracy = 100;

        }

        public override int CalculateDamageToDeal(float attackDefenseRatio, float modifiersValue, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            return user.GetLevel();
        }

    }
}
