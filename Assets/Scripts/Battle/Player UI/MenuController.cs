using UnityEngine;

namespace Battle.PlayerUI
{
    public abstract class MenuController : MonoBehaviour
    {

        public void Show()
        {

            gameObject.SetActive(true);

            foreach (MenuSelectableController selectableController in GetSelectables())
                selectableController.SetSelectedImageState(false);

        }

        public void Hide()
        {

            gameObject.SetActive(false);

            foreach (MenuSelectableController selectableController in GetSelectables())
                selectableController.SetSelectedImageState(false);

        }

        protected void HideSelectableSelectedImages()
        {
            foreach (MenuSelectableController selectableController in GetSelectables())
                selectableController.SetSelectedImageState(false);
        }

        protected abstract MenuSelectableController[] GetSelectables();

    }
}
