using UnityEngine;
using Battle;

namespace Pokemon.Moves
{
    public class Move_Defense_Curl : PokemonMove
    {

        public Move_Defense_Curl()
        {

            id = 111;
            name = "Defense Curl";
            description = "The user curls up to conceal weak spots and raise its Defense stat.";
            type = Type.Normal;
            moveType = MoveType.Status;
            maxPP = 40;
            power = 0;
            accuracy = 0;

            userStatChanges = new Stats<sbyte>()
            {
                attack = 0,
                defense = 1,
                specialAttack = 0,
                specialDefense = 0,
                speed = 0,
                health = 0
            };

        }

        public override void DoDefenseCurlUpdate(ref UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            base.DoDefenseCurlUpdate(ref usageResults, user, target, battleData);

            if (usageResults.Succeeded)
                usageResults.setDefenseCurl = true;

        }

    }
}
