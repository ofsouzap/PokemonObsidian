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
                SwitchPokemon,
                Recharge //When the participant's pokemon is recharging and so the participant is not allowed to act in order to wait for this to happen
            }

            public Type type;

            #region Priority Comparison

            //Lower priorities mean that the action will be run later; higher priorities mean the actions will be run earlier

            public static readonly Dictionary<Type, byte> typePriorities = new Dictionary<Type, byte>
            {
                { Type.Fight, 1 },
                { Type.Flee, 5 },
                { Type.SwitchPokemon, 4 },
                { Type.UseItem, 3 },
                { Type.Recharge, 2 }
            };

            public class PriorityComparer : Comparer<Action>
            {

                public BattleData battleData = null;

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

                        sbyte xMovePriority = Pokemon.Moves.PokemonMove.GetPokemonMoveById(x.user.ActivePokemon.moveIds[x.fightMoveIndex]).movePriority switch
                        {
                            true => 1,
                            null => 0,
                            false => -1
                        };
                        sbyte yMovePriority = Pokemon.Moves.PokemonMove.GetPokemonMoveById(y.user.ActivePokemon.moveIds[y.fightMoveIndex]).movePriority switch
                        {
                            true => 1,
                            null => 0,
                            false => -1
                        };

                        if (xMovePriority == yMovePriority)
                        {

                            if (battleData != null && battleData.stageModifiers.trickRoomRemainingTurns > 0)
                                return y.user.ActivePokemon.GetBattleStats().speed.CompareTo(x.user.ActivePokemon.GetBattleStats().speed); //Reverse speed comparison

                            else
                                return x.user.ActivePokemon.GetBattleStats().speed.CompareTo(y.user.ActivePokemon.GetBattleStats().speed); //Normal speed comparison

                        }
                        else
                            return xMovePriority.CompareTo(yMovePriority);

                    }
                    else if (x.type == Type.Flee)
                    {
                        return 0;
                    }
                    else if (x.type == Type.SwitchPokemon)
                    {
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
                            return 1;
                        }
                        else if (y.useItemItemToUse is PokeBall)
                        {
                            return -1;
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