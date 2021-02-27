using System.Collections;
using System.Collections.Generic;
using Battle;

namespace Battle
{
    public abstract partial class BattleParticipant
    {

        public struct Action
        {

            public enum Type
            {
                Fight,
                //UseItem, TODO - include once items ready
                Flee,
                SwitchPokemon
            }

            public Type type;

            /// <summary>
            /// The index of the move to use in the active pokemon's move list
            /// </summary>
            public int fightMoveIndex;

            /// <summary>
            /// The index of the pokemon to change to in the participant's party
            /// </summary>
            public int switchPokemonIndex;

            //TODO - item-using properties

        }

    }
}