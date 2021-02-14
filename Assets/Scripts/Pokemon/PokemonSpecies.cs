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

        public Type type1;
        public Type? type2;

        /// <summary>
        /// Path for the sound file for the pokemon's cry
        /// </summary>
        public string cryResourcePath;

        #endregion

        #region Stats

        public Stats<byte> baseStats;
        public Stats<byte> evYield;

        #endregion

        #region Evolution

        public class Evolution
        {

            /// <summary>
            /// The id of the pokemon that is evolved into
            /// </summary>
            public int targetId;

            /// <summary>
            /// A level that needs to be reached to level up
            /// </summary>
            public byte? level;

            /// <summary>
            /// Id of item that needs to be used to evolve if applicable else null
            /// </summary>
            public int? itemId;

            /// <summary>
            /// Whether the pokemon must be traded to evolve
            /// </summary>
            public bool requireTrade;

            /// <summary>
            /// A condition that must be met by a pokemon to evolve if applicable else null
            /// </summary>
            public Predicate<PokemonInstance> condition;

        }

        public readonly Evolution[] evolutions;

        #endregion

        #region Moves

        /// <summary>
        /// { level , moveId }. Describes moves the pokemon can learn and the level at which they learn them
        /// </summary>
        public Dictionary<byte, int> levelUpMoves;

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

        public GrowthType growthType;

        //TODO - abilities

        //TODO - breeding stuff incl. egg groups, egg hatch step count, gender ratio

    }
}