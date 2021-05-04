using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Items;

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
            /// Id of item that needs to be used to evolve if applicable else null (this id *excludes* the general item type id)
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

            /// <summary>
            /// Checks if a particular PokemonInstance can use this evolution
            /// </summary>
            /// <param name="pokemon">The pokemon being considered</param>
            /// <param name="trading">Whether this is being checked after having traded</param>
            /// <param name="itemIdUsed">An item that is being *used* (not held!) on the pokemon. This id *includes* the type id of the item</param>
            public bool PokemonCanUseEvolution(PokemonInstance pokemon,
                bool trading = false,
                int? itemIdUsed = null)
            {

                bool tradeCondition = trading == requireTrade;
                bool levelCondition = level == null ? true : pokemon.GetLevel() >= level;

                bool itemCondition;
                if (itemId == null)
                    itemCondition = true;
                else
                {
                    if (itemIdUsed == null) //If an item wasn't used but an item was required
                        itemCondition = false;
                    else //Check whether the used item is the item required for the evolution
                        itemCondition = (itemIdUsed - Item.GetItemIdTypeId((int)itemIdUsed)) == itemId; //The type id of the item isn't included in Evolution.itemId
                }

                bool specialCondition = condition == null ? true : condition(pokemon);

                return tradeCondition && levelCondition && itemCondition && specialCondition;

            }

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

        public bool CanLearnMove(int moveId)
            => baseMoves.Contains(moveId) || discMoves.Contains(moveId) || levelUpMoves.Values.Count(x => x.Contains(moveId)) > 0;

        #endregion

        public GrowthType growthType;

        public byte baseFriendship;

        #region Egg Properties

        public EggGroup? eggGroup1;
        public EggGroup? eggGroup2;

        /// <summary>
        /// Gets any of the pokemon species's egg groups that aren't null
        /// </summary>
        public EggGroup[] GetEggGroups()
        {

            List<EggGroup> groups = new List<EggGroup>();

            if (eggGroup1 != null)
                groups.Add((EggGroup)eggGroup1);

            if (eggGroup2 != null)
                groups.Add((EggGroup)eggGroup2);

            return groups.ToArray();

        }

        /// <summary>
        /// Checks whether two pokemon have any matching egg groups
        /// </summary>
        public bool CheckEggGroupCompatibility(PokemonSpecies other)
        {

            //Ditto can breed with any other pokemon
            if (id == 132 || other.id == 132)
                return true;

            EggGroup[] selfEggGroups = GetEggGroups();
            EggGroup[] otherEggGroups = other.GetEggGroups();

            return otherEggGroups.Where(x => selfEggGroups.Contains(x)).Count() > 0;

        }

        public byte eggCycles;

        #endregion

        //TODO - abilities

    }
}