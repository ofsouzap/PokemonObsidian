using Battle;
using System.Collections.Generic;

namespace Pokemon.Moves
{
    public class Move_Toxic : PokemonMove
    {

        public Move_Toxic()
        {

            id = 92;
            name = "Toxic";
            description = "A move that leaves the target badly poisoned. Its poison damage worsens every turn.";
            maxPP = 10;
            power = 0;
            accuracy = 90;
            type = Type.Poison;
            moveType = MoveType.Status;
            nonVolatileStatusConditionChances = new Dictionary<PokemonInstance.NonVolatileStatusCondition, float>()
            {
                { PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned, 1 }
            };

        }

        public override ushort CalculateAccuracyValue(PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            if (user.HasType(Type.Poison))
                return 100;
            else
                return base.CalculateAccuracyValue(user, target, battleData);
        }

    }
}
