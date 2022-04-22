using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FreeRoaming.Menu;
using FreeRoaming.WildPokemonArea;
using Items;
using Battle;
using Audio;
using Pokemon;

namespace FreeRoaming
{
    public class PlayerController : GameCharacterController
    {

        public static PlayerController singleton;

        [Min(0)]
        [Tooltip("A short delay between rotating and being able to move to allow rotating on the spot without accidently moving")]
        public float rotateToMoveDelay = 0;
        protected float lastRotate = 0;

        protected float moveDelayStartTime = float.MinValue;
        protected float moveDelay;
        private bool AllowedToMove_delay => Time.time - moveDelayStartTime >= moveDelay;

        protected bool movementLocked = false;
        private bool AllowedToMove_locked => !movementLocked;

        protected bool AllowedToMove => AllowedToAct && AllowedToMove_delay && AllowedToMove_locked;

        #region Encounter Chance Multiplier

        /// <summary>
        /// A multiplier for the player's wild encounter chance used only for cheats
        /// </summary>
        private float wildEncounterCheatMultiplier = 1;

        public void SetWildEncounterCheatMultiplier(float value)
            => wildEncounterCheatMultiplier = value;

        public const float runningEncounterChanceMultiplier = 2;

        protected float CurrentEncounterChanceMultiplier
            => (currentMovementType switch
            {
                MovementType.Walk => 1,
                MovementType.Run => runningEncounterChanceMultiplier,
                _ => 1
            })
            * wildEncounterCheatMultiplier;

        #endregion

        #region Trainer Encounters Enabled (Cheat)

        protected bool trainerEncounterCheatEnabled = true;

        public void SetTrainerEncounterCheatEnabled(bool state)
            => trainerEncounterCheatEnabled = state;

        public bool GetTrainerEncountersCheatEnabled() => trainerEncounterCheatEnabled;

        #endregion

        public void TrySetSingleton()
        {

            if (singleton != null && singleton != this)
            {
                Debug.LogError("Multiple PlayerController instances present");
                Destroy(gameObject);
            }
            else
            {
                singleton = this;
            }

        }

        protected override void Start()
        {

            RefreshSpriteFromPlayerData();

            base.Start();

            stepCycle = 0;
            MovementCompleted += () => IncrementStepCycle(); //Add a step everytime the player finishes a step
            SetUpStepCycleListeners();

            PlayerData.singleton.respawnSceneStackSet = false;

            currentSceneArea = null;

            wildEncounterCheatMultiplier = 1;

            TrySetSingleton();

            //Whenever the player completes a movement, check if a wild pokemon battle should be launched
            MovementCompleted += () => WildPokemonBattleLaunchUpdate();

            //Track player steps walked
            MovementCompleted += () => PlayerData.singleton.AddStepWalked();

            //Reduce repel steps remaining
            MovementCompleted += () => TryDecrementRepelSteps();

            //When movement completed, make sure a respawn scene stack is set. Otherwise set one and display an error message
            MovementCompleted += () =>
            {
                if (!PlayerData.singleton.respawnSceneStackSet)
                    RefreshRespawnSceneStackFromCurrent();
            };
            
        }

        protected override void Update()
        {

            base.Update();

            #region Movement

            if (AllowedToMove)
            {

                Vector2 inputMovementVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                FacingDirection selectedDirection = GetVector2MaximumMagnitudeDirection(inputMovementVector);

                if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                {

                    if (directionFacing == selectedDirection)
                    {

                        if (Time.time - lastRotate >= rotateToMoveDelay)
                            TryMoveForward(Input.GetButton("Run") ? MovementType.Run : MovementType.Walk);

                    }
                    else
                    {

                        if (TryTurn(selectedDirection))
                        {

                            WildPokemonBattleLaunchUpdate();
                            lastRotate = Time.time;

                        }

                    }

                }

            }

            #endregion

            #region Interaction

            if (AllowedToAct)
            {

                if (Input.GetButtonDown("Interact"))
                {

                    TryInteractInFront();

                }

            }

            #endregion

            #region Menu

            if (!FreeRoamMenuController.singleton.IsShown)
            {
                if (Input.GetButtonDown("Menu") && AllowedToAct)
                {
                    FreeRoamMenuController.singleton.Show();
                    sceneController.SetSceneRunningState(false);
                }
            }
            else
            {
                if (Input.GetButtonDown("Menu") || Input.GetButtonDown("Cancel"))
                {
                    FreeRoamMenuController.singleton.CloseMenu(); //Scene is resumed by FreeRoamMenuController.CloseMenu
                }
            }

            #endregion

            CurrentSceneAreaRefreshUpdate();

        }

        public void RefreshSpriteFromPlayerData()
        {

            if (!isMoving)
                RefreshNeutralSprite();

            spriteGroupName = PlayerData.singleton.profile.SpriteName;

        }

        public void SetMoveDelay(float delay)
        {
            moveDelayStartTime = Time.time;
            moveDelay = delay;
        }

        public void SetMovementLockState(bool state)
        {
            movementLocked = state;
        }

        #region Step Cycle

        private byte stepCycle = 0;

        public event OnComplete OnStepCycleCompleted;

        private void IncrementStepCycle()
        {

            //Overflow is intended
            stepCycle++;

            if (stepCycle == 0)
                OnStepCycleCompleted?.Invoke();

        }

        private void SetUpStepCycleListeners()
        {

            //Adding pokemon friendship
            OnStepCycleCompleted += () =>
            {
                PlayerData.singleton.RefreshAddFriendshipForStepCycle();
            };

        }

        #endregion

        #region Respawning

        /// <summary>
        /// Sets the player's respawn scene stack as their current position and the current scene stack
        /// </summary>
        public void RefreshRespawnSceneStackFromCurrent()
            => SetRespawnSceneStack(GameSceneManager.CurrentSceneStack);

        /// <summary>
        /// Sets the player's respawn scene stack as the provided position and the current scene stack
        /// </summary>
        public void RefreshRespawnSceneStackFromCurrent(Vector2Int position)
        {

            GameSceneManager.SceneStack stack = GameSceneManager.CurrentSceneStack;

            //Manually set the respawn position
            stack.elements[stack.Length - 1].position = position;

            SetRespawnSceneStack(stack);

        }

        /// <summary>
        /// Sets the player's respawn scene stack and position
        /// </summary>
        public void SetRespawnSceneStack(GameSceneManager.SceneStack stack)
        {

            PlayerData.singleton.respawnSceneStackSet = true;
            PlayerData.singleton.respawnSceneStack = stack;

        }

        /// <summary>
        /// Fully heals the player's party pokemon and returns them to their respawn point
        /// </summary>
        public void Respawn()
        {

            if (!PlayerData.singleton.respawnSceneStackSet)
            {
                Debug.LogError("No respawn scene stack has been set yet");
                return;
            }

            PlayerData.singleton.HealPartyPokemon();
            TryTurn(FacingDirection.Down);

            GameSceneManager.LoadSceneStack(PlayerData.singleton.respawnSceneStack);

        }

        #endregion

        #region Repel

        protected int repelStepsRemaining = 0;

        /// <summary>
        /// Reduces number of repel steps remaining by 1 (if repel in use) and notifies the player if the repel's effect now runs out
        /// </summary>
        protected void TryDecrementRepelSteps()
        {

            if (!RepelActive)
                return;

            repelStepsRemaining--;

            if (!RepelActive) //If repel has just lost its effect
            {

                StartCoroutine(NotifyPlayerRepelWearOff());

            }

        }

        private IEnumerator NotifyPlayerRepelWearOff()
        {

            //Text box shouldn't already be shown otherwise the player shouldn't be able to move
            if (textBoxController.IsShown)
            {
                Debug.LogError("Text box was already shown when decrementing player repel steps");
                yield break;
            }

            //Scene controller should be active otherwise the player shouldn't be able to move
            if (!sceneController.SceneIsActive)
            {
                Debug.LogError("Scene was not active when decrementing player repel steps");
                yield break;
            }

            sceneController.SetSceneRunningState(false);
            textBoxController.Show();

            textBoxController.RevealText(GeneralItem.repelWearOffMessage);
            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            textBoxController.Hide();
            sceneController.SetSceneRunningState(true);

        }

        public bool RepelActive => repelStepsRemaining > 0;

        /// <summary>
        /// Sets how many steps of repel the player has using an Item instance. This Item instance must be a repel item (this is checked using its id)
        /// </summary>
        public void SetRepelSteps(Item item)
        {

            int steps = item.id switch
            {
                GeneralItem.repelId => GeneralItem.repelSteps,
                GeneralItem.superRepelId => GeneralItem.superRepelSteps,
                GeneralItem.maxRepelId => GeneralItem.maxRepelSteps,
                _ => -1
            };

            if (steps > 0)
            {
                SetRepelSteps(steps);
            }
            else
            {
                Debug.LogError("Item provided to set repel steps isn't a repel item");
            }

        }

        public void SetRepelSteps(int steps)
        {

            repelStepsRemaining = steps;

        }

        #endregion

        #region Wild Pokemon Area/Battle

        public void WildPokemonBattleLaunchUpdate()
        {

            if (currentWildPokemonArea != null)
            {
                
                if (currentWildPokemonArea.RunEncounterCheck(CurrentEncounterChanceMultiplier))
                {

                    PokemonInstance prospectiveOpponentInstance = currentWildPokemonArea.GenerateWildPokemon();

                    //If repel active and prospective opponent has lower level than player party conscious head, don't launch the battle
                    if (!RepelActive
                        || prospectiveOpponentInstance.GetLevel() >= PlayerData.singleton.PartyConsciousHead.GetLevel())
                    {

                        LaunchWildPokemonBattle(currentWildPokemonArea, prospectiveOpponentInstance);

                    }

                }

            }

        }

        private void LaunchWildPokemonBattle(WildPokemonAreaController pokemonArea,
            PokemonInstance opponentInstance = null)
        {

            BattleEntranceArguments.argumentsSet = true;

            BattleEntranceArguments.battleBackgroundResourceName =
                pokemonArea.GetBattleBackgroundResourceName() == null || pokemonArea.GetBattleBackgroundResourceName() == ""
                ? BattleEntranceArguments.defaultBackgroundName
                : pokemonArea.GetBattleBackgroundResourceName();

            //TODO - set initial weather

            BattleEntranceArguments.battleType = BattleType.WildPokemon;
            BattleEntranceArguments.wildPokemonBattleArguments = new BattleEntranceArguments.WildPokemonBattleArguments()
            {
                opponentInstance = opponentInstance ?? pokemonArea.GenerateWildPokemon()
            };

            MusicSourceController.singleton.SetTrack(BattleEntranceArguments.defaultPokemonBattleMusicName, true);

            GameSceneManager.LaunchBattleScene();

        }

        #endregion

        #region Current Scene Area

        public SceneArea? currentSceneArea = null;

        protected void CurrentSceneAreaRefreshUpdate()
        {

            if (!sceneController.SceneIsActive)
                return;

            //If moving, the position to refresh the wild pokemon area with is the movement-targetted position
            Vector2 wildPokemonAreaQueryPos = isMoving ? movementTargettedGridPosition : position;
            SceneAreaController areaController = SceneAreaController.GetPositionSceneArea(wildPokemonAreaQueryPos, Scene);

            if (areaController != null)
            {
                if (TrySetCurrentSceneArea(areaController.GetArea()))
                {
                    areaController.OnPlayerEnterArea();
                }
            }

        }

        protected bool TrySetCurrentSceneArea(SceneArea? _sceneArea)
        {

            if (_sceneArea == null)
                return false;

            SceneArea sceneArea = (SceneArea)_sceneArea;

            if (currentSceneArea != null && ((SceneArea)currentSceneArea).id == sceneArea.id)
                return false;
            else
            {
                currentSceneArea = sceneArea;
                return true;
            }

        }

        public void PlaySceneAreaMusic()
        {
            currentSceneArea?.TryPlayAreaMusic();
        }

        #endregion

        #region Item Receiving

        public static string GetItemAnnouncementMessage(Item item,
            uint quantity)
            => PlayerData.singleton.profile.name + " obtained " + item.itemName + " x" + quantity.ToString() + "!";

        /// <summary>
        /// Adds the specified item (and of the specified quantity) to the player's inventory and announces is using the text box
        /// </summary>
        /// <param name="item">The item to add to the player's inventory</param>
        /// <param name="quantity">How many of the item to give the player</param>
        public IEnumerator ObtainItem(Item item,
            uint quantity = 1)
        {

            #region Adding to player inventory

            PlayerData.singleton.inventory.AddItem(item, quantity);

            #endregion

            #region Announcing

            //Sound FX
            SoundFXController.singleton.PlaySound(DroppedItemController.getItemSoundFXName);

            //Text box message
            yield return StartCoroutine(ObtainItemAnnouncementCoroutine(item, quantity));

            #endregion

        }

        protected IEnumerator ObtainItemAnnouncementCoroutine(Item item,
            uint quantity = 1)
        {

            bool textBoxControllerWasShowing = textBoxController.IsShown;
            bool wasPaused = !sceneController.SceneIsRunning;

            if (!textBoxControllerWasShowing)
                textBoxController.Show();

            if (!wasPaused)
                sceneController.SetSceneRunningState(false);

            textBoxController.RevealText(GetItemAnnouncementMessage(item, quantity));
            yield return textBoxController.PromptAndWaitUntilUserContinue();

            if (!textBoxControllerWasShowing)
                textBoxController.Hide();

            if (!wasPaused)
                sceneController.SetSceneRunningState(true);

        }

        #endregion

    }
}
