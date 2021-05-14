﻿using System;
using System.Collections;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public abstract class NPCController : GameCharacterController, IInteractable
    {

        #region ID

        //Negative ids should be used for NPCs in testing scenes and positive IDs for NPCs for the release but this isn't strict

        [Tooltip("An id to identify the NPC. If left as 0, the id should be assumed to have not been set and to be irrelevant")]
        public int id = 0;

        public bool HasId
            => id != 0;

        public static bool IdIsUnset(int id)
            => id == 0;

        #endregion

        protected override void Start()
        {

            base.Start();

            usingAutomaticMovement = automaticMovementStages != null && automaticMovementStages.Length > 0;

            if (usingAutomaticMovement)
            {

                currentAutomaticMovementIndex = 0;
                StartCoroutine(ExecuteNextAutomaticMovementStep());

            }

        }

        protected override void Update()
        {

            base.Update();

            if (!isMoving && movingForwardSteps)
            {

                if (remainingForwardMoveDistance <= 0)
                {
                    movingForwardSteps = false;
                    MoveForwardStepsComplete?.Invoke(true);
                }
                else
                {
                    if (TryMoveForward())
                        remainingForwardMoveDistance--;
                    else
                    {
                        MoveForwardStepsComplete?.Invoke(false);
                        movingForwardSteps = false;
                    }
                }

            }
            else if (usingAutomaticMovement && !currentlyExecutingAutomaticMovementStep)
            {

                StartCoroutine(ExecuteNextAutomaticMovementStep());

            }

        }

        public abstract void Interact(GameCharacterController interacter);

        #region Move Forward Steps

        private bool movingForwardSteps = false;
        private ushort remainingForwardMoveDistance;

        public delegate void OnMovementComplete(bool success);
        public event OnMovementComplete MoveForwardStepsComplete;

        /// <summary>
        /// Tries to move the NPC forward a given number of steps. If can't move any further forwards, it stops.
        /// </summary>
        /// <param name="maxDistance">The distance to try to move</param>
        protected virtual void MoveForwardSteps(ushort maxDistance)
        {

            usingAutomaticMovement = false;

            MoveForwardStepsComplete = null;

            movingForwardSteps = true;
            remainingForwardMoveDistance = maxDistance;

        }

        #endregion

        #region Automatic Movement

        protected bool usingAutomaticMovement;
        private bool currentlyExecutingAutomaticMovementStep = false;
        private int currentAutomaticMovementIndex = 0;

        //If the NPC is currently using moveForward then they won't ever use automatic movement again

        [Serializable]
        public struct AutomaticMovementStage
        {

            [Min(0)]
            /// <summary>
            /// How lon got wait before trying to do this stage.
            /// This can also be used with a distance of 0 set to mean to delay for some time without necessarily moving afterwards
            /// </summary>
            public float initialDelayTime;

            /// <summary>
            /// The direction to move in
            /// </summary>
            public FacingDirection direction;

            /// <summary>
            /// How far to move
            /// </summary>
            public byte distance;

        }

        [SerializeField]
        [Tooltip("The stages of this NPC's automatic movement. These will be looped and so should return the NPC back to its original position")]
        protected AutomaticMovementStage[] automaticMovementStages;

        private IEnumerator ExecuteNextAutomaticMovementStep()
        {

            currentlyExecutingAutomaticMovementStep = true;

            AutomaticMovementStage currentStage = automaticMovementStages[currentAutomaticMovementIndex];
            currentAutomaticMovementIndex = (currentAutomaticMovementIndex + 1) % automaticMovementStages.Length;

            yield return new WaitUntil(() => sceneController.SceneIsRunning);
            yield return new WaitForSeconds(currentStage.initialDelayTime);
            if (!usingAutomaticMovement)
                yield break;

            yield return new WaitUntil(() => sceneController.SceneIsRunning);
            yield return new WaitUntil(() => !usingAutomaticMovement || TryTurn(currentStage.direction));
            if (!usingAutomaticMovement)
                yield break;

            for (byte i = 0; i < currentStage.distance; i++)
            {
                yield return new WaitUntil(() => sceneController.SceneIsRunning);
                yield return new WaitUntil(() => !usingAutomaticMovement || TryMoveForward());
                if (!usingAutomaticMovement)
                    yield break;
            }

            currentlyExecutingAutomaticMovementStep = false;

        }

        #endregion

        #region Talking

        protected IEnumerator Speak(string[] messages)
        {
            foreach (string message in messages)
                yield return StartCoroutine(Speak(message));
        }

        protected virtual IEnumerator Speak(string message)
        {

            textBoxController.Show();
            
            bool thisPausedScene = sceneController.SceneIsRunning;

            if (thisPausedScene)
                sceneController.SetSceneRunningState(false);

            textBoxController.RevealText(message);

            yield return new WaitUntil(() => textBoxController.textRevealComplete);

            if (thisPausedScene)
                sceneController.SetSceneRunningState(true);

            //Can't hide the text box instantly because it wouldn't give the user time to read the message

        }

        #endregion

    }
}
