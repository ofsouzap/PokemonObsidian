using System.Linq;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Beat_Up : PokemonMove
    {

        public Move_Beat_Up()
        {

            id = 251; // ID here
            name = "Beat Up"; // Full name here
            description = ""; // Description here
            type = Type.Dark; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 10; // PP here
            power = 10; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetHitCount(PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            bool userIsPlayer = battleData.participantPlayer.GetPokemon().Contains(user);

            if (userIsPlayer)
                return (byte)battleData.participantPlayer.GetPokemon().Where(p => p != null).Count();
            else
                return (byte)battleData.participantOpponent.GetPokemon().Where(p => p != null).Count();
        }

    }
}
