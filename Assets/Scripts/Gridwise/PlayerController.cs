using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gridwise;

namespace Gridwise
{
    public class PlayerController : GameCharacterController
    {

        [Min(0)]
        [Tooltip("A short delay between rotating and being able to move to allow rotating on the spot without accidently moving")]
        public float rotateToMoveDelay = 0;
        protected float lastRotate = 0;

        protected override void Update()
        {

            base.Update();

            #region Movement

            FacingDirection selectedDirection = FacingDirection.Up;
            bool movementChosen = false;

            if (Input.GetAxis("Horizontal") > 0)
            {
                selectedDirection = FacingDirection.Up;
                movementChosen = true;
            }
            else if (Input.GetAxis("Horiontal") < 0)
            {
                selectedDirection = FacingDirection.Down;
                movementChosen = true;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                selectedDirection = FacingDirection.Left;
                movementChosen = true;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                selectedDirection = FacingDirection.Right;
                movementChosen = true;
            }

            if (movementChosen)
            {

                if (directionFacing == selectedDirection)
                {

                    if (Time.time - lastRotate >= rotateToMoveDelay)
                        TryMoveForward();

                }
                else
                {

                    TryTurn(selectedDirection);
                    lastRotate = Time.time;

                }

            }

            #endregion

        }

    }
}