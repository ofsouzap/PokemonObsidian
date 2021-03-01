using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class MenuPartyPokemonMovesController : MonoBehaviour
    {

        public Button buttonMove0;
        public Button buttonMove1;
        public Button buttonMove2;
        public Button buttonMove3;

        public Text textName;
        public Text textMaxPP;
        public Text textDescription;

        public Text textPowerValue;
        public Text textAccuracyValue;

        public Image imageCategory;
        public Image imageType;

        private void Start()
        {

            Debug.Assert(buttonMove0.GetComponentInChildren<Text>() != null);
            Debug.Assert(buttonMove1.GetComponentInChildren<Text>() != null);
            Debug.Assert(buttonMove2.GetComponentInChildren<Text>() != null);
            Debug.Assert(buttonMove3.GetComponentInChildren<Text>() != null);

        }

        public void SetMoveNames(
            string moveName0,
            string moveName1,
            string moveName2,
            string moveName3)
        {

            buttonMove0.GetComponentInChildren<Text>().text = moveName0;
            buttonMove1.GetComponentInChildren<Text>().text = moveName1;
            buttonMove2.GetComponentInChildren<Text>().text = moveName2;
            buttonMove3.GetComponentInChildren<Text>().text = moveName3;

        }

        /// <summary>
        /// Set the details of the currently-selected move with specified values
        /// </summary>
        public void SetMoveDetails(
            string name,
            byte maxPP,
            string description,
            byte power,
            byte accuracy,
            string categoryImagePath,
            string typeImagePath
            )
        {

            textName.text = name;
            textMaxPP.text = maxPP.ToString();
            textDescription.text = description;

            textPowerValue.text = power.ToString();
            textAccuracyValue.text = accuracy.ToString();

            imageCategory.sprite = (Sprite)Resources.Load(categoryImagePath);
            imageType.sprite = (Sprite)Resources.Load(typeImagePath);

        }

        /// <summary>
        /// Set the details of the currently-selected move using a move's id
        /// </summary>
        /// <param name="moveId">The id of the move to set the details of</param>
        public void SetMoveDetails(int moveId)
        {

            Pokemon.Moves.PokemonMove move = Pokemon.Moves.PokemonMove.GetPokemonMoveById(moveId);

            SetMoveDetails(
                move.name,
                move.maxPP,
                move.description,
                move.power,
                move.accuracy,
                "", //TODO - add path once decided structure
                "" //TODO - add path once decided structure
                );

        }

    }
}
