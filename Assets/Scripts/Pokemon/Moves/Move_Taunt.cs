using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Taunt : PokemonMove
    {

        public Move_Taunt()
        {

            id = 269; // ID here
            name = "Taunt"; // Full name here
            description = "The foe is taunted into a rage that allows it to use only attack moves for two to four turns."; // Description here
            type = Type.Dark; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override int CalculateTauntTurnCount(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => PokemonInstance.BattleProperties.VolatileStatusConditions.GetRandomTauntDuration(battleData);

    }
}
