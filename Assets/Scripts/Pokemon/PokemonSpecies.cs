using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Pokemon
{
    public struct PokemonSpecies : IHasId
    {

        #region Registry

        public static Registry<PokemonSpecies> registry = new Registry<PokemonSpecies>();

        public static PokemonSpecies GetPokemonSpeciesById(int id)
        {
            return registry.StartingIndexSearch(id, id - 1);
        }

        #endregion

        #region Sprites

        public enum SpriteType
        {
            Front1,
            Front2,
            Back,
            Icon
        }

        #endregion

        #region Basic Properties

        public int id;
        public int GetId() => id;

        public string name;
        public string description;

        public Type type1;
        public Type? type2;

        /// <summary>
        /// Name of the resources (eg. sprites and audio) for the pokemon. Will be used as eg. "Resources/Sprites/Pokemon/{resourceName}". If this is empty, the pokemon's id will be used instead
        /// </summary>
        public string resourceName;

        #endregion

        #region Gender Ratio

        //These are the *relative* proportions of each gender. They need not add up to any number; they are relative to each other

        public byte maleRelativeGenderProportion;
        public byte femaleRelativeGenderProportion;
        public byte genderlessRelativeGenderProportion;

        /// <summary>
        /// The total of the relative proportions of each gender
        /// </summary>
        public ushort GenderRelativeProportionTotal => (ushort)(maleRelativeGenderProportion + femaleRelativeGenderProportion + genderlessRelativeGenderProportion);

        //The proportions of each species that total 1

        public float MaleProportion => (float)maleRelativeGenderProportion / GenderRelativeProportionTotal;
        public float FemaleProportion => (float)femaleRelativeGenderProportion / GenderRelativeProportionTotal;
        public float GenderlessProportion => (float)genderlessRelativeGenderProportion / GenderRelativeProportionTotal;

        public bool? GetRandomWeightedGender()
        {

            float rand = UnityEngine.Random.Range(0F, 1F);

            if (rand <= MaleProportion)
                return true;
            else if (rand <= FemaleProportion + MaleProportion)
                return false;
            else
                return null;

        }

        #endregion

        #region Stats

        public Stats<byte> baseStats;

        public Stats<byte> evYield;
        public ushort baseExperienceYield;
        public byte catchRate;

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

        public Evolution[] evolutions;

        #endregion

        #region Moves

        /// <summary>
        /// Moves the pokemon could have at any level. A.k.a moves learnt at level 1/level 0
        /// </summary>
        public int[] baseMoves;

        /// <summary>
        /// { level , moveId }. Describes moves the pokemon can learn and the level at which they learn them
        /// </summary>
        public Dictionary<byte, int[]> levelUpMoves;

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

        //TODO - breeding stuff incl. egg groups, egg hatch step count

    }
}