using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Return : PokemonMove
    {

        public Move_Return()
        {

            id = 216; // ID here
            name = "Return"; // Full name here
            description = "A full-power attack that grows more powerful the more the user likes its Trainer."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => user.GetReturnAttackPower();

    }
}
