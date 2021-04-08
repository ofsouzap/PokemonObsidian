using System.Collections.Generic;
using UnityEngine;
using Battle;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

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
                UseItem,
                Flee,
                SwitchPokemon
            }

            public Type type;

            #region Priority Comparison

            //Lower priorities mean that the action will be run later; higher priorities mean the actions will be run earlier

            public static readonly Dictionary<Type, byte> typePriorities = new Dictionary<Type, byte>
            {
                { Type.Fight, 1 },
                { Type.Flee, 4 },
                { Type.SwitchPokemon, 3 },
                { Type.UseItem, 2 }
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
                    else if (x.type == Type.UseItem)
                    {
                        if (x.useItemItemToUse is PokeBall && y.useItemItemToUse is PokeBall)
                        {
                            Debug.LogError("Two participants trying to use poke ball");
                            return 0;
                        }
                        else if (x.useItemItemToUse is PokeBall)
                        {
                            return -1;
                        }
                        else if (y.useItemItemToUse is PokeBall)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
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

            /// <summary>
            /// The item to be used
            /// </summary>
            public Item useItemItemToUse;

            /// <summary>
            /// The index of the pokemon in the player's party to use the item on (if applicable)
            /// </summary>
            public int useItemTargetPartyIndex;

            /// <summary>
            /// The battle participant that the pokemon is being used on. This should always be the opponent participant
            /// </summary>
            public BattleParticipant useItemPokeBallTarget;

            /// <summary>
            /// The index of the move to use an item on (if applicable)
            /// </summary>
            public int useItemTargetMoveIndex;

            /// <summary>
            /// If the used item shouldn't be taken from the player's inventory if the player uses it
            /// </summary>
            public bool useItemDontConsumeItem;

        }

    }
}