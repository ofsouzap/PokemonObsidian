using System.Collections;
using UnityEngine;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Decorations
{
    [RequireComponent(typeof(Collider2D))]
    public class TownSignController : MonoBehaviour, IInteractable
    {

        [Tooltip("An array of the messages to display when the player interacts with the sign")]
        public string[] messages;

        public Vector2Int[] GetPositions()
            => new Vector2Int[] { Vector2Int.RoundToInt(transform.position) };

        private FreeRoamSceneController sceneController;
        private TextBoxController textBoxController;

        private void Start()
        {
            sceneController = FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene);
            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);
        }

        public void Interact(GameCharacterController interactee)
        {

            StartCoroutine(DisplayMessages());

        }

        private IEnumerator DisplayMessages()
        {

            sceneController.SetSceneRunningState(false);
            textBoxController.Show();

            foreach (string message in messages)
            {
                textBoxController.RevealText(message);
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());
            }

            textBoxController.Hide();
            sceneController.SetSceneRunningState(true);

        }

    }
}