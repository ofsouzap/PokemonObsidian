using Battle;

namespace Pokemon.Moves
{
    public class Move_Growth : PokemonMove
    {

        public Move_Growth()
        {

            id = 74;
            name = "Growth";
            description = "The user's body grows all at once, raising the Attack and Special Attack stats.";
            maxPP = 20;
            power = 0;
            accuracy = 0;
            type = Type.Normal;
            moveType = MoveType.Status;

        }

        public override UsageResults CalculateStatChanges(UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            sbyte stageRaiseCount = battleData.currentWeatherId == 1 ? (sbyte)2 : (sbyte)1;

            usageResults.userStatChanges = new Stats<sbyte>()
            {
                attack = stageRaiseCount,
                specialAttack = stageRaiseCount
            };

            return usageResults;

        }

    }
}
