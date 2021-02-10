using System.Collections.Generic;
using UnityEngine;
using Gridwise;

namespace Gridwise
{

    /// <summary>
    /// The class for any characters on the grid in a free-roaming scene. The player and NPCs will inherit from this
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class GameCharacterController : MonoBehaviour, IOccupyPositions
    {

        public enum DirectionFacing
        {
            Down,
            Up,
            Left,
            Right
        }

        /// <summary>
        /// The dirction being faced
        /// </summary>
        public DirectionFacing directionFacing { get; protected set; }

        /// <summary>
        /// The position the character is currently in
        /// </summary>
        public Vector2Int position { get; protected set; }

        /// <summary>
        /// The position the character may be moving into which may also be reserved
        /// </summary>
        public Vector2Int movementTargettedGridPosition { get; protected set; }

        /// <summary>
        /// Whether the character is moving. Also used to check whether the movementTargettedPosition should be counted when returning occupied positions
        /// </summary>
        public bool isMoving { get; protected set; }

        /// <summary>
        /// If the character is moving, return both of its positions else only current position
        /// </summary>
        /// <returns>The positions that the game character is occupying</returns>
        public Vector2Int[] GetPositions() => isMoving ?
            new Vector2Int[] { position, movementTargettedGridPosition } :
            new Vector2Int[] { position };
        
        [Tooltip("Sprites for this character will be fetched as Resources/{spriteGroupName}/{sprite needed}")]
        public string spriteGroupName;

        /// <summary>
        /// Stores each sprite that the character may use whilst in the main, free-roaming scene including sprites for standing still in each direction and moving in each direction
        /// </summary>
        protected class Sprites
        {

            /// <summary>
            /// A dictionary storing all the sprites for a character.
            /// The keys are identifiers for the sprites in the format (using the '?' regex quantifier)
            ///     "{state}(_{direction})?(_{index})?"
            ///     Where everything is lower case, {state} is the state of the sprite (eg. walking, neutral), {direction} is the direction of the sprite which may be "up","down","left" or "right" and {index} is the index of that sprite. for example, there are two sprites for a walking character
            /// The values are the sprite instances
            /// </summary>
            protected Dictionary<string, Sprite> sprites;

            protected string GenerateIdentifier(string stateIdentifier,
                DirectionFacing direction,
                int index = -1)
            {

                string directionIdentifier;

                switch (direction)
                {

                    case DirectionFacing.Down:
                        directionIdentifier = "down";
                        break;

                    case DirectionFacing.Left:
                        directionIdentifier = "left";
                        break;

                    case DirectionFacing.Up:
                        directionIdentifier = "up";
                        break;

                    case DirectionFacing.Right:
                        directionIdentifier = "right";
                        break;

                    default:
                        Debug.LogWarning($"Invalid direction facing ({direction})");
                        directionIdentifier = "right";
                        break;

                }

                return stateIdentifier + '_' + directionIdentifier + (index == -1 ? "" : '_' + index.ToString());

            }

            /// <summary>
            /// Get a sprite referenced by a full identifier
            /// </summary>
            /// <param name="fullIdentifier">The full identifier as explained for Sprite.sprites</param>
            /// <returns>The sprite as specified if found, otherwise null</returns>
            public Sprite Get(string fullIdentifier)
            {

                if (sprites.ContainsKey(fullIdentifier))
                    return sprites[fullIdentifier];
                else
                    return null;

            }

            /// <summary>
            /// Gets a sprite referenced by a state identifier and a DirectionFacing direction. The full identifier will then be formed from these parameters
            /// </summary>
            /// <param name="stateIdentifier">The name of the state that is being requested (eg. "neutral")</param>
            /// <param name="direction">The direction of sprite to request</param>
            /// <param name="index">The index of the sprite to request</param>
            /// <returns>The sprite as specified if found, otherwise null</returns>
            public Sprite Get(string stateIdentifier,
                DirectionFacing direction,
                int index = -1)
            {

                return Get(GenerateIdentifier(stateIdentifier, direction, index));

            }

            /// <summary>
            /// Adds a sprite to the loaded sprites
            /// </summary>
            /// <param name="sprite">The sprite to add</param>
            /// <param name="fullIdentifier">The identifier for the sprite</param>
            public void Add(Sprite sprite, string fullIdentifier)
            {

                if (sprites.ContainsKey(fullIdentifier))
                    sprites[fullIdentifier] = sprite;
                else
                    sprites.Add(fullIdentifier, sprite);

            }

            /// <summary>
            /// Adds a sprite to the loaded sprites
            /// </summary>
            /// <param name="sprite">The sprite to add</param>
            /// <param name="stateIdentifier">The name od the state that should be added (eg. "neutral")</param>
            /// <param name="direction">The direction of the sprite to add</param>
            /// <param name="index">The index of the sprite to request</param>
            public void Add(Sprite sprite,
                string stateIdentifier,
                DirectionFacing direction,
                int index = -1)
            {

                Add(sprite, GenerateIdentifier(stateIdentifier, direction, index));

            }

        }

        /// <summary>
        /// The game character's sprites
        /// </summary>
        protected Sprites sprites;

        protected virtual void Start()
        {

            position = Vector2Int.RoundToInt(transform.position);

            if ((!spriteGroupName.Equals(""))
                && spriteGroupName != null)
                LoadSprites();
            
        }

        /// <summary>
        /// Loads the character's sprites into memory using spriteGroupName and the Resources class
        /// </summary>
        protected void LoadSprites()
        {

            //TODO

        }

    }
}