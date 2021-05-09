using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FreeRoaming.Menu.StorageSystem
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    public class PointerController : MonoBehaviour
    {

        public enum SpriteType
        {
            Neutral,
            Grabbing
        }

        public Sprite neutralSprite;
        public Sprite grabbingSprite;

        [SerializeField]
        private Vector2 anchoredPositionOffset;

        private StorageSystemMenuController menuController;

        private void Start()
        {

            SetSpriteType(SpriteType.Neutral);
            InputMethodMonitor.singleton.InputMethodChanged.AddListener((oldMethod) =>
            {

                switch (InputMethodMonitor.singleton.CurrentInputMethod)
                {

                    case InputMethodMonitor.InputMethod.Buttons:
                        Show();
                        break;

                    case InputMethodMonitor.InputMethod.Mouse:
                        Hide();
                        break;

                }

            });

        }

        public void SetMenuController(StorageSystemMenuController menuController)
        {
            this.menuController = menuController;
        }

        private void SetSpriteType(SpriteType type)
        {

            GetComponent<Image>().sprite = type switch
            {
                SpriteType.Neutral => neutralSprite,
                SpriteType.Grabbing => grabbingSprite,
                _ => null
            };

        }

        public void SetPosition(Vector2 position)
        {
            if (menuController.GetControlEnabled())
                transform.position = position + anchoredPositionOffset;
        }

        public void Show()
            => GetComponent<Image>().enabled = true;

        public void Hide()
            => GetComponent<Image>().enabled = false;

    }
}