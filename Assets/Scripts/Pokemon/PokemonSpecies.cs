using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Pokemon
{
    public struct PokemonSpecies
    {

        #region Registry

        //TODO - have registry always sorted by id
        public static PokemonSpecies[] registry;

        public static PokemonSpecies GetPokemonSpeciesById(int id)
        {

            //TODO - code a binary search
            return registry.First((x) => x.id == id);

        }

        #endregion

        #region Basic Properties

        public int id;
        public string name;
        public string description;
        public byte catchRate;

        /// <summary>
        /// Path for the sound file for the pokemon's cry
        /// </summary>
        public string cryResourcePath;

        #endregion

        #region Stats

        public struct Stats
        {

            public byte attack;
            public byte defense;
            public byte specialAttack;
            public byte specialDefense;
            public byte health;
            public byte speed;

        }

        public Stats baseStats;
        public Stats evYield;

        #endregion

        #region Evolution

        public class Evolution
        {

            /// <summary>
            /// The id of the pokemon that is evolved into
            /// </summary>
            public int targetId;

            /// <summary>
            /// Id of item that needs to be used to evolve if applicable else null
            /// </summary>
            public int? itemId;

            /// <summary>
            /// A condition that must be met by a pokemon to evolve if applicable else null
            /// </summary>
            public Predicate<PokemonSpecies> condition;

        }

        public readonly Evolution[] evolutions;

        #endregion

        #region Moves

        /// <summary>
        /// { level , moveId }. Describes moves the pokemon can learn and the level at which they learn them
        /// </summary>
        public Dictionary<int, int> levelUpMoves;

        /// <summary>
        /// Ids of moves the pokemon can learn by TM/HM
        /// </summary>
        public int[] discMoves;

        /// <summary>
        /// Ids of moves the pokemon can learn when hatching from an egg
        /// </summary>
        public int[] eggMoves;

        /// <summary>
        /// Ids of moves the pokemon can learn with a tutor
        /// </summary>
        public int[] tutorMoves;

        #endregion

        //TODO - abilities

        //TODO - growth rate

        //TODO - breeding stuff incl. egg groups, egg hatch step count, gender ratio

    }
}