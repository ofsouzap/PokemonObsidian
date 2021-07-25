using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FreeRoaming.Menu;
using FreeRoaming.WildPokemonArea;
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

        public const float runningEncounterChanceMultiplier = 2;

        protected float CurrentEncounterChanceMultiplier
            => currentMovementType switch
            {
                MovementType.Walk => 1,
                MovementType.Run => runningEncounterChanceMultiplier,
                _ => 1
            };

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

            base.Start();

            TrySetSingleton();

            //Whenever the player completes a movement, check if a wild pokemon battle should be launched
            MovementCompleted += () => WildPokemonBattleLaunchUpdate();
            
        }

        protected override void Update()
        {

            base.Update();

            #region Movement

            if (AllowedToMove)
            {

                FacingDirection selectedDirection = FacingDirection.Up;

                Dictionary<FacingDirection, float> inputMagnitudes = new Dictionary<FacingDirection, float>();
                inputMagnitudes.Add(FacingDirection.Down, -Mathf.Clamp(Input.GetAxis("Vertical"), -1, 0));
                inputMagnitudes.Add(FacingDirection.Up, Mathf.Clamp(Input.GetAxis("Vertical"), 0, 1));
                inputMagnitudes.Add(FacingDirection.Left, -Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 0));
                inputMagnitudes.Add(FacingDirection.Right, Mathf.Clamp(Input.GetAxis("Horizontal"), 0, 1));

                KeyValuePair<FacingDirection, float> maximumMagnitude = inputMagnitudes.Aggregate((a, b) => a.Value > b.Value ? a : b);
                selectedDirection = maximumMagnitude.Key;

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
                    FreeRoamMenuController.singleton.Hide();
                    sceneController.SetSceneRunningState(true);
                }
            }

            #endregion

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

    }
}
