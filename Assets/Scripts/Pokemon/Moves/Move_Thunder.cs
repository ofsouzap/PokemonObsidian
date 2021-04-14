using System.Collections.Generic;
using Battle;

namespace Pokemon.Moves
{
    public class Move_Thunder : PokemonMove
    {

        public Move_Thunder()
        {

            id = 87;
            name = "Thunder";
            description = "A wicked thunderbolt is dropped on the target to inflict damage. This may also leave the target with paralysis.";
            maxPP = 10;
            power = 110;
            accuracy = 70;
            type = Type.Electric;
            moveType = MoveType.Special;
            nonVolatileStatusConditionChances = new Dictionary<PokemonInstance.NonVolatileStatusCondition, float>()
            {
                { PokemonInstance.NonVolatileStatusCondition.Paralysed, 0.3F }
            };

        }

        public override ushort CalculateAccuracyValue(PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            if (battleData.currentWeatherId == 2)
                return 100;
            else if (battleData.currentWeatherId == 1)
                return 50;
            else
                return base.CalculateAccuracyValue(user, target, battleData);
        }

    }
}
