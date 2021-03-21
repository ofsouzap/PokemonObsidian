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

        /// <summary>
        /// Loads a pokemon species' sprite. The type and gender of sprite to load is specified in the parameters
        /// </summary>
        /// <param name="resourceName">The name of the sprite's resource. This tends to be the pokemon species' id</param>
        /// <param name="spriteType">The type of sprite to load</param>
        /// <param name="useFemale">Whether to load the female sprite instead of the "male" sprite. For genderless pokemon, the "male" sprite should be used</param>
        /// <returns>The loaded sprite if it could be found, otherwise, null</returns>
        public static Sprite LoadSprite(
            string resourceName,
            SpriteType spriteType,
            bool useFemale)
        {

            #region Path

            string resourcePath;
            string alternativeResourcePath = null; //The resource path to load if the main can't be found. (this is to use male sprites if female ones can't be found)

            string typeDirectory;
            string alternativeTypeDirectory = null;

            switch (spriteType)
            {

                case SpriteType.Front1:
                    typeDirectory = "Front 1";
                    break;

                case SpriteType.Front2:
                    typeDirectory = "Front 2";
                    break;

                case SpriteType.Back:
                    typeDirectory = "Back";
                    break;

                case SpriteType.Icon:
                    typeDirectory = "Icon";
                    break;

                default:
                    typeDirectory = "Icon";
                    Debug.LogError("Unknown sprite type provided - " + spriteType);
                    break;

            }

            if (spriteType != SpriteType.Icon)
            {

                if (!useFemale)
                {
                    typeDirectory += " Male";
                }
                else
                {
                    alternativeTypeDirectory = typeDirectory + " Male";
                    typeDirectory += " Female";
                }

            }

            resourcePath = "Sprites/Pokemon/" + typeDirectory + '/' + resourceName;

            if (alternativeTypeDirectory != null)
            {
                alternativeResourcePath = "Sprites/Pokemon/" + alternativeTypeDirectory + '/' + resourceName;
            }

            #endregion

            Sprite sprite = Resources.Load<Sprite>(resourcePath);

            if (sprite != null)
            {
                return sprite;
            }
            else
            {
                if (alternativeResourcePath != null)
                {
                    return Resources.Load<Sprite>(alternativeResourcePath);
                }
                else
                {
                    return null;
                }
            }

        }

        public static Sprite LoadSprite(
            string resourceName,
            SpriteType spriteType,
            bool? gender
            )
        {
            bool useFemale;

            switch (gender)
            {

                case true:
                case null:
                    useFemale = false;
                    break;

                case false:
                    useFemale = true;
                    break;

            }

            return LoadSprite(resourceName, spriteType, useFemale);
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

        //TODO - breeding stuff incl. egg groups, egg hatch step count, gender ratio

    }
}