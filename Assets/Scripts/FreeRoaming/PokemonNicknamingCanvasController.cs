using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace FreeRoaming
{
    [RequireComponent(typeof(Canvas))]
    public class PokemonNicknamingCanvasController : MonoBehaviour
    {

        private bool isActive;

        public InputField inputField;
        public Button submitButton;

        private UnityEvent nameSubmitted = new UnityEvent();

        private void Start()
        {

            Hide();

            submitButton.onClick.AddListener(() => SubmitName());
            
        }

        private void Update()
        {
            
            if (isActive)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                    SubmitName();
            }

        }

        private void SubmitName()
        {
            nameSubmitted.Invoke();
        }

        public void RunCanvas(Action<string> onNameSubmit)
        {

            nameSubmitted.RemoveAllListeners();

            nameSubmitted.AddListener(() =>
            {
                onNameSubmit.Invoke(inputField.text);
                Hide();
            });

            Show();

        }

        public void Show()
        {
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            GetComponent<Canvas>().enabled = true;
            inputField.interactable = true;
            submitButton.interactable = true;
            isActive = true;
        }

        public void Hide()
        {
            EventSystem.current.SetSelectedGameObject(null);
            GetComponent<Canvas>().enabled = false;
            inputField.interactable = false;
            submitButton.interactable = false;
            isActive = false;
        }

    }
}