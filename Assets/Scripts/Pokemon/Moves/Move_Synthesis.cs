using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Synthesis : PokemonMove
    {

        public Move_Synthesis()
        {

            id = 235; // ID here
            name = "Synthesis"; // Full name here
            description = "The user restores its own HP. The amount of HP regained varies with the weather."; // Description here
            type = Type.Grass; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override float GetUserMaxHealthRelativeHealthHealed(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => battleData.currentWeatherId switch
            {
                0 => 0.5F, // Clear sky
                1 => 0.667F, // Harsh sunlight
                _ => 0.25F // Otherwise
            };

    }
}
