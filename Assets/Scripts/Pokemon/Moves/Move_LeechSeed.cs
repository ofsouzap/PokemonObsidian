using Battle;

namespace Pokemon.Moves
{
    public class Move_LeechSeed : PokemonMove
    {

        public Move_LeechSeed()
        {

            id = 73;
            name = "Leech Seed";
            description = "A seed is planted on the foe. It steals some HP from the foe to heal the user on every turn.";
            maxPP = 10;
            power = 0;
            accuracy = 90;
            type = Type.Grass;
            moveType = MoveType.Status;

        }

        public override UsageResults CalculateLeechSeedChanges(UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            if (target.battleProperties.volatileStatusConditions.leechSeed || target.species.type1 == Type.Grass || target.species.type2 == Type.Grass)
            {
                usageResults.failed = true;
                return usageResults;
            }
            else
            {
                usageResults.inflictLeechSeed = true;
                return usageResults;
            }

        }

    }
}
