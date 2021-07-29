using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FreeRoaming.AreaControllers;

namespace FreeRoaming
{
    [RequireComponent(typeof(RectTransform))]
    public class AreaNameSignController : MonoBehaviour
    {

        private const string areaNameSignSpriteSheetResourcesPath = "Sprites/sprite_sheet_area_name_signs";
        private const string areaNameSignSpriteNamePrefix = "area_name_sign_";

        private const string defaultAreaNameSpriteTypeName = "forest";

        public static AreaNameSignController GetAreaNameSignController(Scene scene)
        {

            AreaNameSignController[] controllers = FindObjectsOfType<AreaNameSignController>()
                .Where(x => x.gameObject.scene == scene)
                .ToArray();

            switch (controllers.Length)
            {

                case 0:
                    return null;

                case 1:
                    return controllers[0];

                default:
                    Debug.LogError("Multiple AreaNameSignControllers found");
                    return controllers[0];

            }

        }

        public float signMovementTime = 0.2f;
        public float signRemainTime = 2f;

        [Header("References")]

        public RectTransform signRootTransform;
        public Text nameText;
        public Image backgroundImage;

        [Header("Positions")]

        public Vector2 signOffScreenPosition;
        public Vector2 signOnScreenPosition;

        private void Start()
        {

            Hide();

        }

        private static Sprite LoadAreaNameSignSprite(string typeName)
        {

            foreach (Sprite sprite in Resources.LoadAll<Sprite>(areaNameSignSpriteSheetResourcesPath))
            {

                if (sprite.name == areaNameSignSpriteNamePrefix + typeName)
                    return sprite;

            }

            return null;

        }

        public void DisplayAreaName(string name)
        {

            //TODO - allow for specifying a specific area name sign sprite instead of using a default one

            if (showSignCoroutine != null)
                StopCoroutine(showSignCoroutine);

            showSignCoroutine = StartCoroutine(ShowAreaNameSignCoroutine(name, LoadAreaNameSignSprite(defaultAreaNameSpriteTypeName)));

        }

        private Coroutine showSignCoroutine = null;

        private IEnumerator ShowAreaNameSignCoroutine(string name, Sprite areaNameSignSprite)
        {

            nameText.text = name;
            backgroundImage.sprite = areaNameSignSprite;

            yield return SmoothTranslateSign(signOffScreenPosition, signOnScreenPosition, signMovementTime);

            yield return new WaitForSeconds(signRemainTime);

            yield return SmoothTranslateSign(signOnScreenPosition, signOffScreenPosition, signMovementTime);

        }

        private IEnumerator SmoothTranslateSign(Vector2 startPos, Vector2 endPos, float animTime)
        {

            signRootTransform.anchoredPosition = startPos;

            float startTime = Time.time;

            while (Time.time <= startTime + animTime)
            {

                float t = Mathf.InverseLerp(startTime, startTime + animTime, Time.time);

                Vector2 newPosition = Vector2.Lerp(startPos, endPos, t);

                signRootTransform.anchoredPosition = newPosition;

                yield return new WaitForEndOfFrame();

            }

            signRootTransform.anchoredPosition = endPos;

        }

        private void Hide()
        {
            signRootTransform.anchoredPosition = signOffScreenPosition;
        }

    }
}