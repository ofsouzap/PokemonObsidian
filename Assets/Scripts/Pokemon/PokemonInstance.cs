using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;
using Items;

namespace Pokemon
{
    public class PokemonInstance
    {

        #region Species

        public PokemonSpecies species;

        #endregion

        public string nickname;

        public Item heldItem;

        #region Stats

        public Stats effortValues;
        public readonly Stats individualValues;

        public int health;

        #endregion

        #region Moves

        public PokemonMove[] moves = new PokemonMove[4];
        public int[] movePPs = new int[4];

        #endregion

        #region Experience

        public int experience;
        //TODO - function to calculate level

        #endregion

        #region NonVolatileStatusConditions

        public enum NonVolatileStatusCondition
        {
            None,
            Burn,
            Frozen,
            Paralysed,
            Poisoned,
            BadlyPoisoned,
            Asleep
        }

        public NonVolatileStatusCondition nonVolatileStatusCondition = NonVolatileStatusCondition.None;

        #endregion

        #region BattleProperties

        public class BattleProperties
        {

            //TODO - add properties

        }

        public BattleProperties battleProperties;

        #endregion

        //TODO - function to check if should evolve

    }
}