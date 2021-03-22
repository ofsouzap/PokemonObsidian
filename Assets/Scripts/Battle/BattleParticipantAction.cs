using System.Collections.Generic;
using UnityEngine;
using Battle;

namespace Battle
{
    public abstract partial class BattleParticipant
    {

        public struct Action
        {

            public Action(BattleParticipant user) : this()
            {
                this.user = user;
            }

            public enum Type
            {
                Fight,
                //UseItem, TODO - include once items ready
                Flee,
                SwitchPokemon
            }

            public Type type;

            #region Priority Comparison

            //Lower priorities mean that the action will be run later; higher priorities mean the actions will be run earlier

            public static readonly Dictionary<Type, byte> typePriorities = new Dictionary<Type, byte>
            {
                { Type.Fight, 1 },
                { Type.Flee, 3 },
                { Type.SwitchPokemon, 2 }
            };

            public class PriorityComparer : Comparer<Action>
            {
                public override int Compare(Action x, Action y)
                {
                    
                    if (x.type != y.type)
                    {

                        byte xPriority = typePriorities[x.type];
                        byte yPriority = typePriorities[y.type];

                        return xPriority.CompareTo(yPriority);

                    }
                    else if (x.type == Type.Fight) //If both trying to fight
                    {

                        return x.user.ActivePokemon.GetBattleStats().speed.CompareTo(y.user.ActivePokemon.GetBattleStats().speed);

                    }
                    else if (x.type == Type.Flee)
                    {
                        return 0;
                    }
                    else if (x.type == Type.SwitchPokemon)
                    {
                        //TODO - check if there is a priority for switching pokemon
                        return 0;
                    }
                    //TODO - add other selection branch for items once prepared
                    else
                    {
                        Debug.LogError("Action type hasn't been prepared for when comparing action priorities");
                        return 0;
                    }

                }
            }

            #endregion

            /// <summary>
            /// The BattleParticipant that intends to do this action
            /// </summary>
            public BattleParticipant user;

            /// <summary>
            /// Whether the pokemon is going to use struggle. If so, fightMoveIndex should be ignored
            /// </summary>
            public bool fightUsingStruggle;

            /// <summary>
            /// The index of the move to use in the active pokemon's move list
            /// </summary>
            public int fightMoveIndex;

            /// <summary>
            /// A reference to the BattleParticipant that is being targeted by the move.
            /// </summary>
            public BattleParticipant fightMoveTarget;

            /// <summary>
            /// The index of the pokemon to change to in the participant's party
            /// </summary>
            public int switchPokemonIndex;

            //TODO - item-using properties

        }

    }
}