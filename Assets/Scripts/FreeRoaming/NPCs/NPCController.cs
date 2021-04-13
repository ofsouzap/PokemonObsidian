using System.Collections;
using UnityEngine;

namespace FreeRoaming.NPCs
{
    public abstract class NPCController : GameCharacterController
    {

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

        }

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

            MoveForwardStepsComplete = null;

            movingForwardSteps = true;
            remainingForwardMoveDistance = maxDistance;

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
