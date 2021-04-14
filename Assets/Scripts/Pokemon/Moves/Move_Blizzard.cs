using System.Collections.Generic;
using Battle;

namespace Pokemon.Moves
{
    public class Move_Blizzard : PokemonMove
    {

        public Move_Blizzard()
        {

            id = 59;
            name = "Blizzard";
            description = "A howling blizzard is summoned to strike the opposing Pokemon. This may also leave the opposing Pokemon frozen";
            maxPP = 5;
            power = 110;
            accuracy = 70;
            type = Type.Ice;
            moveType = MoveType.Special;
            nonVolatileStatusConditionChances = new Dictionary<PokemonInstance.NonVolatileStatusCondition, float>()
            {
                { PokemonInstance.NonVolatileStatusCondition.Frozen, 0.1F }
            };

        }

        public override ushort CalculateAccuracyValue(PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            if (battleData.currentWeatherId == 4) //If in hail
                return 100;
            else
                return base.CalculateAccuracyValue(user, target, battleData);

        }

    }
}
