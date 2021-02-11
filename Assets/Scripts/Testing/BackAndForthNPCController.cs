using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FreeRoaming;

//This shouldn't be included in builds
#if UNITY_EDITOR

namespace Testing
{
    public class BackAndForthNPCController : GameCharacterController
    {

        public int walkLength;

        protected override void Start()
        {

            base.Start();

            StartCoroutine(WalkingCoroutine());

        }

        private IEnumerator WalkingCoroutine()
        {

            bool goingRight = true;

            yield return new WaitUntil(() => TryTurn(goingRight ? FacingDirection.Right : FacingDirection.Left));

            while (true)
            {

                for (int i = 0; i < walkLength; i++)
                    yield return new WaitUntil(() => TryMoveForward());

                yield return new WaitUntil(() => isMoving == false);

                goingRight = !goingRight;

                yield return new WaitUntil(() => TryTurn(goingRight ? FacingDirection.Right : FacingDirection.Left));

            }

        }

    }
}

#endif