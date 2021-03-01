using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class MenuFightController : MonoBehaviour
    {

        public Button buttonBack;

        public Button buttonMove0;
        public Button buttonMove1;
        public Button buttonMove2;
        public Button buttonMove3;

        public Text textMovePowerValue;
        public Text textMoveAccuracyValue;
        public Text textMovePPValue;

        public Image imageMoveCategory;
        public Image imageMoveType;
        
        private void Start()
        {

            Debug.Assert(buttonMove0.GetComponentInChildren<Text>() != null);
            Debug.Assert(buttonMove1.GetComponentInChildren<Text>() != null);
            Debug.Assert(buttonMove2.GetComponentInChildren<Text>() != null);
            Debug.Assert(buttonMove3.GetComponentInChildren<Text>() != null);

        }

        /// <summary>
        /// Set the names of the active pokemon's moves for the buttons
        /// </summary>
        /// <param name="nameMove0">The name of the first move</param>
        /// <param name="nameMove1">The name of the second move</param>
        /// <param name="nameMove2">The name of the third move</param>
        /// <param name="nameMove3">The name of the fourth move</param>
        public void SetMoveNames(
            string nameMove0,
            string nameMove1,
            string nameMove2,
            string nameMove3
            )
        {

            buttonMove0.GetComponentInChildren<Text>().text = nameMove0;
            buttonMove1.GetComponentInChildren<Text>().text = nameMove1;
            buttonMove2.GetComponentInChildren<Text>().text = nameMove2;
            buttonMove3.GetComponentInChildren<Text>().text = nameMove3;

        }

        /// <summary>
        /// Set the details of the selected move for the details panel
        /// </summary>
        /// <param name="ppRemaining">The PP remaining of the move</param>
        /// <param name="ppMax">The maximum PP of the move</param>
        /// <param name="power">The power of the move</param>
        /// <param name="accuracy">The accuracy of the move</param>
        /// <param name="categoryImagePath">The path to the icon for the move's category (physical, special or status)</param>
        /// <param name="typeImagePath">The path to the icon for the move's (elemental) type</param>
        public void SetMoveDetails(
            byte ppRemaining,
            byte ppMax,
            byte power,
            byte accuracy,
            string categoryImagePath,
            string typeImagePath
            )
        {

            textMovePPValue.text = ppRemaining + "/" + ppMax;
            textMovePowerValue.text = power.ToString();
            textMoveAccuracyValue.text = accuracy.ToString();

            imageMoveCategory.sprite = (Sprite)Resources.Load(categoryImagePath);
            imageMoveType.sprite = (Sprite)Resources.Load(typeImagePath);

        }

    }
}
