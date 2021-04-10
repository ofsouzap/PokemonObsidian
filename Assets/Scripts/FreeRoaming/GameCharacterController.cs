using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FreeRoaming;

namespace FreeRoaming
{

    /// <summary>
    /// The class for any characters on the grid in a free-roaming scene. The player and NPCs will inherit from this
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class GameCharacterController : FreeRoamSprite, IOccupyPositions
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

        public const float movementSpriteChangeDelay = 0.175F;
        protected Coroutine movementSpriteCoroutine;

        protected const string walkingSpriteName = "walk";

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

        protected GridManager gridManager;
        
        public string spriteGroupName;

        protected override void Start()
        {

            base.Start();

            position = Vector2Int.RoundToInt(transform.position);

            RefreshGridManager();
            SceneChanged.AddListener(RefreshGridManager);

            SpriteStorage.TryLoadAll();

            RefreshNeutralSprite();
            
        }

        protected override void Update()
        {

            base.Update();

            if (sceneController.SceneIsActive)
            {
                if (isMoving)
                {
                    MovementUpdate();
                }
            }

        }

        protected void RefreshGridManager()
        {
            gridManager = GridManager.GetSceneGridManager(Scene);
        }

        public void SetPosition(Vector2Int newPosition)
        {
            
            transform.position = (Vector2)newPosition;
            position = newPosition;

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
                
                CompleteMovement();

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
        /// Stops the character moving by sending them to their destination immediately
        /// </summary>
        public void CompleteMovement()
        {

            isMoving = false;

            SetPosition(movementTargettedGridPosition);

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
        /// Whether the character is allowed to move forwards
        /// </summary>
        public bool CanMoveForward
        {
            get
            {

                if (isMoving)
                    return false;

                if (!gridManager.CheckPositionAvailability(GetPositionInFront()))
                    return false;

                return true;

            }
        }

        /// <summary>
        /// If the character is allowed to turn
        /// </summary>
        public bool CanTurn
        {
            get
            {

                if (isMoving)
                    return false;

                return true;

            }
        }

        /// <summary>
        /// Will change the character's sprite at an interval so show a movement animation
        /// </summary>
        /// <param name="spriteStateName">The sprite state name of the movement to be performed (eg. walking, running)</param>
        protected IEnumerator MovementSpriteCoroutine(string spriteStateName)
        {

            //How many stages of movement there are for any given movement state
            const int movementSpriteIndexCount = 4; //Left, neutral, right, neutral

            bool primedToQuit = false;

            float lastChange = 0;
            int currentSpriteIndex = 0;
            
            while (true)
            {

                if (sceneController.SceneIsActive)
                {

                    if (primedToQuit && isMoving)
                    {
                        primedToQuit = false;
                    }

                    if (!isMoving)
                    {

                        if (primedToQuit)
                        {
                            break;
                        }
                        else
                        {
                            primedToQuit = true;
                        }

                    }

                    if (Time.time - lastChange >= movementSpriteChangeDelay)
                    {

                        lastChange = Time.time;

                        bool flipSprite = false;

                        //N.B. sprite names are NOT 0-indexed
                        Sprite newSprite = currentSpriteIndex switch
                        {
                            0 => SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, spriteStateName, directionFacing, 1),
                            1 => SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, "idle", directionFacing),
                            2 => SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, spriteStateName, directionFacing, 2),
                            3 => SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, "idle", directionFacing),
                            _ => null
                        };

                        if (newSprite == null)
                            Debug.LogWarning($"Sprite fetched for movement was null ({spriteStateName} {directionFacing} {currentSpriteIndex})");
                        else
                        {
                            spriteRenderer.sprite = newSprite;
                            spriteRenderer.flipX = flipSprite;
                        }

                        currentSpriteIndex = (currentSpriteIndex + 1) % movementSpriteIndexCount;

                    }

                }

                yield return new WaitForFixedUpdate();

            }

            RefreshNeutralSprite();

            movementSpriteCoroutine = null;

        }

        /// <summary>
        /// Starts the process of the character moving forward
        /// </summary>
        public void MoveForward()
        {
            
            isMoving = true;
            movementTargettedGridPosition = GetPositionInFront();

            if (movementSpriteCoroutine == null)
            {
                movementSpriteCoroutine = StartCoroutine(MovementSpriteCoroutine(walkingSpriteName));
            }

        }

        /// <summary>
        /// If the character is allowed to move forward, moves them forward
        /// </summary>
        /// <returns></returns>
        public bool TryMoveForward()
        {

            if (CanMoveForward)
            {

                MoveForward();
                return true;

            }
            else
                return false;

        }

        /// <summary>
        /// Turns the character to face a direction
        /// </summary>
        /// <param name="newDirection">The direction to face</param>
        public void Turn(FacingDirection newDirection)
        {

            directionFacing = newDirection;

            RefreshNeutralSprite();

        }

        /// <summary>
        /// If the character is allowed to turn, turns them to face the direction specified. If the character is already facing that direction, the same code will be run
        /// </summary>
        /// <param name="newDirection">The direction to face in</param>
        /// <returns>Whether the character is now facing in the direction passed</returns>
        public bool TryTurn(FacingDirection newDirection)
        {

            if (CanTurn)
            {

                Turn(newDirection);

                return true;

            }
            else
                return false;

        }

        /// <summary>
        /// Sets the character's sprite to a neutral sprite in the direction that it is facing
        /// </summary>
        protected void RefreshNeutralSprite()
        {

            bool flipSprite;
            spriteRenderer.sprite = SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, "idle", directionFacing);
            spriteRenderer.flipX = flipSprite;

        }

        public bool TryInteractInFront()
        {

            GameObject objectInFront = gridManager.GetObjectInPosition(GetPositionInFront());

            if (objectInFront == null)
                return false;

            IInteractable inFrontInteractable = objectInFront.GetComponent<IInteractable>();

            if (inFrontInteractable != null)
            {

                inFrontInteractable.Interact(this);
                return true;

            }
            else
                return false;

        }

    }
}