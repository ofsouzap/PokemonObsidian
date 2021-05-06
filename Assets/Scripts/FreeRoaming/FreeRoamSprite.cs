using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using FreeRoaming.AreaControllers;

namespace FreeRoaming
{
    public class FreeRoamSprite : MonoBehaviour
    {

        [SerializeField]
        protected bool billboardSprite = true;

        private int previousSceneIndex;
        public UnityEvent SceneChanged;

        public Scene Scene => gameObject.scene;
        protected FreeRoamSceneController sceneController;

        protected Camera sceneCamera;
        private float oldSceneCameraRotationEulerX;

        [Tooltip("Character's sprite renderer component. Might be seperated from this script if sprite must be offset from root transform position")]
        public SpriteRenderer spriteRenderer;

        protected virtual void Start()
        {

            previousSceneIndex = Scene.buildIndex;
            RefreshSceneController();

            SceneChanged.AddListener(RefreshSceneController);

            RefreshSceneCamera();

            SceneChanged.AddListener(RefreshSceneCamera);

        }

        protected virtual void Update()
        {

            if (previousSceneIndex != Scene.buildIndex)
            {
                previousSceneIndex = Scene.buildIndex;
                SceneChanged.Invoke();
            }

            if (sceneCamera.transform.rotation.eulerAngles.x != oldSceneCameraRotationEulerX)
                RefreshSpriteAngling();

        }

        protected virtual void RefreshSceneController()
        {
            sceneController = FreeRoamSceneController.GetFreeRoamSceneController(Scene);
        }

        protected void RefreshSceneCamera()
        {
            sceneCamera = FindObjectsOfType<Camera>()
                .Where(x => x.gameObject.scene == Scene)
                .ToArray()[0];
            RefreshSpriteAngling();
        }

        /// <summary>
        /// Makes sure that the sprite is still billboarded to the camera's angle in the correct axis
        /// </summary>
        protected virtual void RefreshSpriteAngling()
        {

            if (!billboardSprite)
                return;

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
