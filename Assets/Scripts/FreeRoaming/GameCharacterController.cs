using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FreeRoaming.WildPokemonArea;

namespace FreeRoaming
{

    /// <summary>
    /// The class for any characters on the grid in a free-roaming scene. The player and NPCs will inherit from this
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))] //For entering triggers
    [RequireComponent(typeof(Collider2D))]
    public abstract class GameCharacterController : FreeRoamSprite, IOccupyPositions
    {

        public const string exclaimPrefabResourcesPath = "Prefabs/Free Roaming/Exclaim";

        [Tooltip("How long the character's exclaiming should last")]
        public float exclaimDuration = 1F;

        #region Direction

        public enum FacingDirection
        {
            Down,
            Up,
            Left,
            Right
        }

        #region Opposite Direction

        public static FacingDirection GetOppositeDirection(FacingDirection direction)
            => direction switch
            {
                FacingDirection.Up => FacingDirection.Down,
                FacingDirection.Right => FacingDirection.Left,
                FacingDirection.Down => FacingDirection.Up,
                FacingDirection.Left => FacingDirection.Right,
                _ => FacingDirection.Up
            };

        #endregion

        /// <summary>
        /// The dirction being faced
        /// </summary>
        public FacingDirection directionFacing { get; protected set; }

        [Tooltip("The direction the this should be facing to start with")]
        public FacingDirection initialDirectionFacing;

        #endregion

        #region Movement

        public enum MovementType
        {
            Walk,
            Run
        }

        protected MovementType currentMovementType = MovementType.Walk;

        [SerializeField]
        [Min(0)]
        [Tooltip("The speed (in units per second) that this character walks at")]
        private float _walkSpeed = 4;

        [SerializeField]
        [Min(0)]
        [Tooltip("The speed (in units per second) that this character runs at")]
        private float _runSpeed = 8;

        protected Dictionary<MovementType, float> movementTypeSpeeds;

        /// <summary>
        /// The name of the movement sprites to use if another sprite can't be found (eg. if the character doesn't have running sprites but needs to run)
        /// </summary>
        protected const string defaultMovementTypeSprite = "walk";

        protected static readonly Dictionary<MovementType, string> movementTypeSpriteNames = new Dictionary<MovementType, string>()
        {
            { MovementType.Walk, "walk" },
            { MovementType.Run, "run" }
        };

        public const float movementSpriteChangeDelay = 0.175F;
        protected Coroutine movementSpriteCoroutine;

        #endregion

        /// <summary>
        /// The position the character is currently in
        /// </summary>
        public Vector2Int position { get; protected set; }

        /// <summary>
        /// The position the character may be moving into which may also be reserved
        /// </summary>
        public Vector2Int movementTargettedGridPosition { get; protected set; }
        public MovementType currentMovementMovementType { get; protected set; }

        protected bool ignoreScenePaused = false;

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

        protected virtual bool AllowedToAct => sceneController.SceneIsActive
                //Below line allows character to move if the scene is just paused and they have let themselves ignore the scene pausing
                || (!sceneController.SceneIsActive && !sceneController.SceneIsRunning && sceneController.SceneIsEnabled && ignoreScenePaused);

        protected TextBoxController textBoxController;

        /// <summary>
        /// A wild pokemon area that the character is currently in. If null, the character isn't in a wild pokemon area
        /// </summary>
        protected WildPokemonAreaController currentWildPokemonArea = null;

        protected override void Start()
        {

            base.Start();

            textBoxController = TextBoxController.GetTextBoxController(Scene);

            position = Vector2Int.RoundToInt(transform.position);
            directionFacing = initialDirectionFacing;

            movementTypeSpeeds = new Dictionary<MovementType, float>();
            movementTypeSpeeds.Add(MovementType.Walk, _walkSpeed);
            movementTypeSpeeds.Add(MovementType.Run, _runSpeed);

            RefreshGridManager();
            SceneChanged.AddListener(RefreshGridManager);

            currentWildPokemonArea = null;

            SpriteStorage.TryLoadAll();

            RefreshNeutralSprite();
            
        }

        protected override void Update()
        {

            base.Update();

            if (AllowedToAct)
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

        #region Position and Movement

        public delegate void OnComplete();
        public event OnComplete MovementCompleted;

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

            float moveDistance = CurrentMovementSpeed * Time.deltaTime;

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

        protected float CurrentMovementSpeed
            => movementTypeSpeeds[currentMovementType];

        /// <summary>
        /// Stops the character moving by sending them to their destination immediately
        /// </summary>
        public void CompleteMovement()
        {

            isMoving = false;

            SetPosition(movementTargettedGridPosition);

            MovementCompleted?.Invoke();

        }

        /// <summary>
        /// Stops the character moving by sending them back to their initial position before their current movement
        /// </summary>
        public void CancelMovement()
        {

            isMoving = false;

            SetPosition(position);

            RefreshNeutralSprite();

        }

        /// <summary>
        /// Get the position in front of a character
        /// </summary>
        /// <returns>The grid position in front of the character</returns>
        public Vector2Int GetPositionInFront() => GridManager.GetPositionInDirection(position, directionFacing);

        /// <summary>
        /// Get the positions in front of a character
        /// </summary>
        /// <returns>The grid positions in front of the character</returns>
        public Vector2Int[] GetPositionsInFront(ushort count) => GridManager.GetPositionsInDirection(position, directionFacing, count);

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

        //How many stages of movement there are for any given movement state
        const int movementSpriteIndexCount = 4; //Left, neutral, right, neutral

        /// <summary>
        /// Will change the character's sprite at an interval so show a movement animation
        /// </summary>
        /// <param name="spriteStateName">The sprite state name of the movement to be performed (eg. walking, running)</param>
        protected IEnumerator MovementSpriteCoroutine()
        {
            
            bool primedToQuit = false;

            float lastChange = 0;
            int currentSpriteIndex = 0;
            
            while (true)
            {

                if (AllowedToAct)
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

                        //N.B. sprite names are NOT 0-indexed
                        Sprite newSprite = GetMovementStageSprite(currentSpriteIndex, movementTypeSpriteNames[currentMovementType], out bool flipSprite);

                        //If sprite can't be found, try use default movement sprite
                        if (newSprite == null)
                        {
                            newSprite = GetMovementStageSprite(currentSpriteIndex, defaultMovementTypeSprite, out flipSprite);
                        }
                        
                        if (newSprite == null)
                            Debug.LogWarning($"Sprite fetched for movement was null ({movementTypeSpriteNames[currentMovementType]} {directionFacing} {currentSpriteIndex})");
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

        protected Sprite GetMovementStageSprite(int movementStage, string spriteStateName, out bool flipSprite)
        {

            flipSprite = false;

            if (this is PlayerController)
            {

                //Final is same as second
                if (movementStage == 3)
                    movementStage = 1;

                //Players have sprites for each movement stage and don't hav eto use the idle sprites for middle movement stage sprites
                return SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, spriteStateName, directionFacing, movementStage + 1);

            }
            else
            {

                return movementStage switch
                {
                    0 => SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, spriteStateName, directionFacing, 1),
                    1 => SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, "idle", directionFacing),
                    2 => SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, spriteStateName, directionFacing, 2),
                    3 => SpriteStorage.GetCharacterSprite(out flipSprite, spriteGroupName, "idle", directionFacing),
                    _ => null
                };

            }

        }

        /// <summary>
        /// Starts the process of the character moving forward
        /// </summary>
        public void MoveForward(MovementType movementType = MovementType.Walk)
        {
            
            isMoving = true;
            movementTargettedGridPosition = GetPositionInFront();
            currentMovementType = movementType;

            if (movementSpriteCoroutine == null)
            {
                movementSpriteCoroutine = StartCoroutine(MovementSpriteCoroutine());
            }

        }

        /// <summary>
        /// If the character is allowed to move forward, moves them forward
        /// </summary>
        /// <returns></returns>
        public bool TryMoveForward(MovementType movementType = MovementType.Walk)
        {

            if (CanMoveForward)
            {

                MoveForward(movementType);
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

        #endregion

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

        public IEnumerator Exclaim()
        {

            GameObject exclaimGameObject = Instantiate(Resources.Load<GameObject>(exclaimPrefabResourcesPath), spriteRenderer.transform);

            yield return new WaitForSeconds(exclaimDuration);

            Destroy(exclaimGameObject);

        }

        #region Wild Pokemon Area

        public void SetWildPokemonArea(WildPokemonAreaController areaController)
        {
            currentWildPokemonArea = areaController;
        }

        public void ExitWildPokemonArea()
        {
            currentWildPokemonArea = null;
        }

        #endregion

    }
}