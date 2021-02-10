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

        public enum FacingDirection
        {
            Down,
            Up,
            Left,
            Right
        }

        /// <summary>
        /// The dirction being faced
        /// </summary>
        public FacingDirection directionFacing { get; protected set; }

        [Min(0)]
        [Tooltip("The speed (in units per second) that this character moves at")]
        public float moveSpeed = 4;

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

        protected Manager gridManager;
        
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
                FacingDirection direction,
                int index = -1)
            {

                string directionIdentifier;

                switch (direction)
                {

                    case FacingDirection.Down:
                        directionIdentifier = "down";
                        break;

                    case FacingDirection.Left:
                        directionIdentifier = "left";
                        break;

                    case FacingDirection.Up:
                        directionIdentifier = "up";
                        break;

                    case FacingDirection.Right:
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
            /// Gets a sprite referenced by a state identifier and a FacingDirection direction. The full identifier will then be formed from these parameters
            /// </summary>
            /// <param name="stateIdentifier">The name of the state that is being requested (eg. "neutral")</param>
            /// <param name="direction">The direction of sprite to request</param>
            /// <param name="index">The index of the sprite to request</param>
            /// <returns>The sprite as specified if found, otherwise null</returns>
            public Sprite Get(string stateIdentifier,
                FacingDirection direction,
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
                FacingDirection direction,
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

            gridManager = Manager.GetManager();

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

        protected virtual void Update()
        {

            if (isMoving)
                MovementUpdate();

        }

        /// <summary>
        /// Moves the character towards its destination
        /// </summary>
        protected virtual void MovementUpdate()
        {

            if (!isMoving)
                Debug.LogWarning("MovementUpdate was called even though character wasn't moving");

            float remainingDistance;

            switch (directionFacing)
            {

                case FacingDirection.Up:
                    remainingDistance = movementTargettedGridPosition.y - transform.position.y;
                    break;

                case FacingDirection.Down:
                    remainingDistance = transform.position.y - movementTargettedGridPosition.y;
                    break;

                case FacingDirection.Right:
                    remainingDistance = movementTargettedGridPosition.x - transform.position.x;
                    break;

                case FacingDirection.Left:
                    remainingDistance = transform.position.x - movementTargettedGridPosition.x;
                    break;

                default:
                    remainingDistance = 0;
                    Debug.LogError("Invalid direction facing when calculating remaining distance");
                    break;

            }

            float moveDistance = moveSpeed * Time.deltaTime;

            if (moveDistance >= remainingDistance)
            {

                isMoving = false;

                transform.position = (Vector2)movementTargettedGridPosition;
                position = movementTargettedGridPosition;

                return;
                
            }

            Vector2 displacement;

            switch (directionFacing)
            {

                case FacingDirection.Up:
                    displacement = Vector2.up * moveDistance;
                    break;

                case FacingDirection.Down:
                    displacement = -Vector2.up * moveDistance;
                    break;

                case FacingDirection.Left:
                    displacement = -Vector2.right * moveDistance;
                    break;

                case FacingDirection.Right:
                    displacement = Vector2.right * moveDistance;
                    break;

                default:
                    Debug.LogError("Invalid directionFacing when trying to move towards target");
                    displacement = Vector2.up * moveDistance;
                    break;

            }

            transform.position += (Vector3)displacement;

        }

        /// <summary>
        /// Get the position in front of a character
        /// </summary>
        /// <returns>The grid position in front of the character</returns>
        public Vector2Int GetPositionInFront()
        {

            Vector2Int offset;

            switch (directionFacing)
            {

                case FacingDirection.Up:
                    offset = Vector2Int.up;
                    break;

                case FacingDirection.Down:
                    offset = -Vector2Int.up;
                    break;

                case FacingDirection.Left:
                    offset = -Vector2Int.right;
                    break;

                case FacingDirection.Right:
                    offset = Vector2Int.right;
                    break;

                default:
                    Debug.LogWarning($"Invalid directionFacing was found ({directionFacing})");
                    offset = Vector2Int.up;
                    break;

            }

            return position + offset;

        }

        /// <summary>
        /// Wether the character is allowed to move forwards
        /// </summary>
        public bool CanMoveForward()
        {

            if (isMoving)
                return false;

            if (!gridManager.CheckPositionAvailability(GetPositionInFront()))
                return false;

            return true;

        }

        /// <summary>
        /// If the character is allowed to turn
        /// </summary>
        public bool CanTurn()
        {

            if (isMoving)
                return false;

            return true;

        }

    }
}