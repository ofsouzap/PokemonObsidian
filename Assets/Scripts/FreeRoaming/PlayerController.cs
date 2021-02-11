using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FreeRoaming;

namespace FreeRoaming
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
                        TryMoveForward();

                }
                else
                {

                    TryTurn(selectedDirection);
                    lastRotate = Time.time;

                }

            }

            #endregion

            #region Interaction

            if (Input.GetButtonDown("Interact"))
            {

                TryInteractInFront();

            }

            #endregion

        }

    }
}