using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FreeRoaming
{
    public abstract class FreeRoamSprite : MonoBehaviour
    {

        public Scene Scene => gameObject.scene;

        [Tooltip("Character's sprite renderer component. Might be seperated from this script if sprite must be offset from root transform position")]
        public SpriteRenderer spriteRenderer;

        protected virtual void Start() { }

        protected virtual void Update()
        {

            RefreshSpriteBillboard();

        }

        /// <summary>
        /// Makes sure that the sprite is still billboarded to the camera's angle in the correct axis
        /// </summary>
        protected virtual void RefreshSpriteBillboard()
        {

            Vector3 cameraPosition = FindObjectsOfType<Camera>()
                .Select(x => x.transform)
                .Where(x => x.gameObject.scene == Scene)
                .ToArray()[0]
                .position;

            Vector3 displacement = cameraPosition - spriteRenderer.transform.position;

            spriteRenderer.transform.rotation = Quaternion.Euler(
                new Vector3(
                    -1 * Mathf.Atan(displacement.z / displacement.y) * Mathf.Rad2Deg,
                    spriteRenderer.transform.rotation.eulerAngles.y,
                    spriteRenderer.transform.rotation.eulerAngles.z
                    )
                );

        }

    }
}
