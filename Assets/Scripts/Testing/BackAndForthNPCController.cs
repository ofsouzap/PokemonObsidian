using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming;
using FreeRoaming.NPCs;

//This shouldn't be included in builds
#if UNITY_EDITOR

namespace Testing
{
    public class BackAndForthNPCController : NPCController
    {

        public ushort walkLength;

        protected override void Start()
        {

            base.Start();

            StartCoroutine(WalkingCoroutine());

        }

        private bool canContinue;

        private IEnumerator WalkingCoroutine()
        {

            bool goingRight = true;

            yield return new WaitUntil(() => TryTurn(goingRight ? FacingDirection.Right : FacingDirection.Left));

            while (true)
            {

                canContinue = false;

                MoveForwardSteps(walkLength);

                MoveForwardStepsComplete += (s) =>
                {

                    if (!s)
                    {
                        Debug.Log("Failed to complete movement");
                    }

                    goingRight = !goingRight;

                    canContinue = false;
                    StartCoroutine(WaitUntilTurned(goingRight));

                };

                yield return new WaitUntil(() => canContinue);

            }

        }

        private IEnumerator WaitUntilTurned(bool turnRight)
        {
            yield return new WaitUntil(() => TryTurn(turnRight ? FacingDirection.Right : FacingDirection.Left));
            canContinue = true;
        }

    }
}

#endif