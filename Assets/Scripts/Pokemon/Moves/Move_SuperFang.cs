using UnityEngine;
using Battle;

namespace Pokemon.Moves
{
    public class Move_SuperFang : PokemonMove
    {
        
        public Move_SuperFang()
        {

            id = 162;
            name = "Super Fang";
            description = "The user chomps hard on the foe with its sharp front fangs. It cuts the target's HP to half";
            type = Type.Normal;
            moveType = MoveType.Physical;
            maxPP = 10;
            power = 0;
            accuracy = 90;

        }

        public override int CalculateDamageToDeal(float attackDefenseRatio, float modifiersValue, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            return Mathf.CeilToInt((float)target.health / 2);
        }

    }
}
