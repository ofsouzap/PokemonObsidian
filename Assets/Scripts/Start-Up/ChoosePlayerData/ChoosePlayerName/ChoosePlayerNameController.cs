using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace StartUp.ChoosePlayerData.ChoosePlayerName
{
    public class ChoosePlayerNameController : ChoosePlayerDataController
    {

        public GameObject defaultSelectedGameObject;

        public InputField nameField;
        public Button continueButton;

        protected override IEnumerator MainCoroutine()
        {

            EventSystem.current.SetSelectedGameObject(defaultSelectedGameObject);

            bool nameEntered = false;

            continueButton.onClick.AddListener(() =>
            {
                nameEntered = true;
            });

            yield return new WaitUntil(() => nameEntered || Input.GetKeyUp(KeyCode.Return));

            //If return key was used to continue
            nameEntered = true;

            PlayerData.singleton.profile.name = nameField.text;

            CloseScene();

        }

    }
}