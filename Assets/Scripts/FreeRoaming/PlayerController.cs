using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FreeRoaming.Menu;
using FreeRoaming.WildPokemonArea;
using Items;
using Battle;
using Audio;

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

        #region Wild Pokemon Battle

        public void WildPokemonBattleLaunchUpdate()
        {

            if (currentWildPokemonArea != null)
            {
                
                if (currentWildPokemonArea.RunEncounterCheck(CurrentEncounterChanceMultiplier))
                {
                    LaunchWildPokemonBattle(currentWildPokemonArea);

                }

            }

        }

        private void LaunchWildPokemonBattle(WildPokemonAreaController pokemonArea)
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
                opponentInstance = pokemonArea.GenerateWildPokemon()
            };

            MusicSourceController.singleton.SetTrack(BattleEntranceArguments.defaultPokemonBattleMusicName, true);

            GameSceneManager.LaunchBattleScene();

        }

        #endregion

        #region Current Scene Area

        public SceneArea? currentSceneArea = null;

        public bool TrySetCurrentSceneArea(SceneArea sceneArea)
        {

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
