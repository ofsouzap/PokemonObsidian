using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Pokemon
{
    public class PokemonMove
    {

        #region Registry

        //TODO - have registry always sorted by id
        public static PokemonMove[] registry;

        public static PokemonMove GetPokemonMoveById(int id)
        {

            //TODO - code a binary search
            return registry.First((x) => x.id == id);

        }

        #endregion

        #region Properties

        public int id;

        public Type type;

        public enum MoveType
        {
            Physical,
            Special,
            Status
        }

        public MoveType moveType;

        /// <summary>
        /// The power of the move [0,100]
        /// </summary>
        public byte power;

        /// <summary>
        /// The chance of the move hitting its target [0,100]
        /// </summary>
        public byte accuracy;

        #endregion

        #region Move Using

        /// <summary>
        /// Calculates the damage that should be done by a move based on the attacking and defending pokemon using just the formula
        /// </summary>
        /// <param name="user">The pokemon using the move</param>
        /// <param name="target">The pokemon being hit by the move</param>
        /// <param name="effectiveness">false if the move is not very effective, null if it is "effective" (multiplier of 1) or true if it is super effective</param>
        /// <returns>The damage that should be dealt by the move</returns>
        public byte CalculateNormalDamage(PokemonInstance user,
            PokemonInstance target,
            out bool? effectiveness,
            out bool criticalHit)
        {

            //https://bulbapedia.bulbagarden.net/wiki/Damage#Damage_calculation

            Type userType1 = user.species.type1;
            Type? userType2 = user.species.type2;

            float stabMultipler = userType1 == type || userType2 == type ? 1.5f : 1f;
            float typeMultipler = userType2 == null ?
                TypeAdvantage.CalculateMultiplier(type, userType1)
                : TypeAdvantage.CalculateMultiplier(type, userType1, (Type)userType2
                );

            if (typeMultipler == 1)
                effectiveness = null;
            else if (typeMultipler < 1)
                effectiveness = false;
            else if (typeMultipler > 1)
                effectiveness = true;
            else
            {
                effectiveness = null;
                Debug.LogError("Erroneous situation reached");
            }

            int attack, defense;

            switch (moveType)
            {

                case MoveType.Physical:
                    attack = user.GetStats().attack;
                    defense = target.GetStats().defense;
                    break;

                case MoveType.Special:
                    attack = user.GetStats().specialAttack;
                    defense = target.GetStats().specialDefense;
                    break;

                default:
                    Debug.LogWarning("Invalid move type - " + moveType);
                    attack = 1;
                    defense = 1;
                    break;

            }

            float ad = ((float)attack) / defense;

            float modifier, weatherMultiplier, criticalMultiplier, randomMultipler, burnMultiplier;

            weatherMultiplier = 1; //TODO - once battle conditions can be known

            //TODO - calculate whether critical landed
            criticalMultiplier = 1;
            criticalHit = false;

            randomMultipler = Random.Range(85, 100) / 100;

            burnMultiplier =
                user.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Burn && moveType == MoveType.Physical
                ? 0.5f
                : 1f;

            modifier = weatherMultiplier * criticalMultiplier * randomMultipler * stabMultipler * typeMultipler * burnMultiplier;

            int rawDamage = Mathf.FloorToInt((( ( ( ( (2 * user.GetLevel()) / ((float)5) ) + 2 ) * power * ad ) / ((float)50) ) + 2 ) * modifier);

            return rawDamage > byte.MaxValue ? byte.MaxValue : (byte)rawDamage;

        }

        /// <summary>
        /// Calculates the damage that should be dealt by a move based on PokemonMove.CalculateNormalDamage. This should only be used for non-status moves
        /// </summary>
        public virtual byte CalculateDamage(PokemonInstance user,
            PokemonInstance target,
            out bool? effectiveness,
            out bool criticalHit)
        {

            if (moveType == MoveType.Status)
            {
                effectiveness = null;
                criticalHit = false;
                return 0;
            }

            //TODO - include details about the battle once a class containing this has been created

            return CalculateNormalDamage(user, target, out effectiveness, out criticalHit);

        }

        public byte CalculateDamage(PokemonInstance user,
            PokemonInstance target)
        {
            bool? _;
            bool _1;
            return CalculateDamage(user, target, out _, out _1);
        }

        /// <summary>
        /// A function to use a status move. Shouldn't be used for non-status moves
        /// </summary>
        /// <param name="user">The user of the move</param>
        public virtual void UseStatus(PokemonInstance user)
        {

            //TODO - include reference to battle state when can

            if (moveType != MoveType.Status)
                return;

        }

        #endregion

    }
}