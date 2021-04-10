using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FreeRoaming
{
    public abstract class FreeRoamSprite : MonoBehaviour
    {

        public Scene Scene => gameObject.scene;
        protected FreeRoamSceneController sceneController;

        protected Camera sceneCamera;
        private float oldSceneCameraRotationEulerX;

        [Tooltip("Character's sprite renderer component. Might be seperated from this script if sprite must be offset from root transform position")]
        public SpriteRenderer spriteRenderer;

        protected virtual void Start()
        {

            sceneController = FreeRoamSceneController.GetFreeRoamSceneController(Scene);

            sceneCamera = FindObjectsOfType<Camera>()
                .Where(x => x.gameObject.scene == Scene)
                .ToArray()[0];
            RefreshSpriteAngling();

        }

        protected virtual void Update()
        {

            if (sceneCamera.transform.rotation.eulerAngles.x != oldSceneCameraRotationEulerX)
                RefreshSpriteAngling();

        }

        /// <summary>
        /// Makes sure that the sprite is still billboarded to the camera's angle in the correct axis
        /// </summary>
        protected virtual void RefreshSpriteAngling()
        {

            oldSceneCameraRotationEulerX = sceneCamera.transform.rotation.eulerAngles.x;

            spriteRenderer.transform.rotation = Quaternion.Euler(
                new Vector3(
                    sceneCamera.transform.rotation.eulerAngles.x,
                    spriteRenderer.transform.rotation.eulerAngles.y,
                    spriteRenderer.transform.rotation.eulerAngles.z
                    )
                );

        }

    }
}
