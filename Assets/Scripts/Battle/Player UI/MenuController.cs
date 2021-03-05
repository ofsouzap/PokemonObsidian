using UnityEngine;

namespace Battle.PlayerUI
{
    public abstract class MenuController : MonoBehaviour
    {

        public void Show()
        {

            gameObject.SetActive(true);

            foreach (MenuSelectableController selectableController in GetSelectables())
                selectableController.HideSelectedImage();

        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        protected void HideSelectableSelectedImages()
        {
            foreach (MenuSelectableController selectableController in GetSelectables())
                selectableController.HideSelectedImage();
        }

        protected abstract MenuSelectableController[] GetSelectables();

    }
}