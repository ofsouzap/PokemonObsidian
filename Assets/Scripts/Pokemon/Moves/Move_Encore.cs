using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Encore : PokemonMove
    {

        public Move_Encore()
        {

            id = 227; // ID here
            name = "Encore"; // Full name here
            description = "The user compels the foe to keep using only the move it last used for three to seven turns."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override int CalculateEncoreTurnCount(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => PokemonInstance.BattleProperties.VolatileStatusConditions.GetRandomEncoreDuration(battleData);

    }
}
