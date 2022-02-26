using Battle;

namespace Pokemon.Moves
{
    public class Move_Night_Shade : PokemonMove
    {

        public Move_Night_Shade()
        {

            id = 101;
            name = "Night Shade";
            description = "The user makes the foe see a mirage. It inflicts damage matching the user's level.";
            type = Type.Ghost;
            moveType = MoveType.Special;
            maxPP = 15;
            power = 0;
            accuracy = 100;

        }

        public override int CalculateDamageToDeal(float attackDefenseRatio, float modifiersValue, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            if (TypeAdvantage.CalculateMultiplier(type, target.species) == 0)
                return 0;
            else
                return user.GetLevel();
        }

    }
}
