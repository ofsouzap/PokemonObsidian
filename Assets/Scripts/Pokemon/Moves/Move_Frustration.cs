using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Frustration : PokemonMove
    {

        public Move_Frustration()
        {

            id = 218; // ID here
            name = "Frustration"; // Full name here
            description = "A full-power attack that grows more powerful the less the user likes its Trainer."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => user.GetFrustrationAttackPower();

    }
}
