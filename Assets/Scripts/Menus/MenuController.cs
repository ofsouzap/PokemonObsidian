using UnityEngine;
using Battle.PlayerUI;

namespace Menus
{
    public abstract class MenuController : MonoBehaviour
    {

        public virtual void Show()
        {

            gameObject.SetActive(true);

            foreach (MenuSelectableController selectableController in GetSelectables())
                selectableController.SetSelectedImageState(false);

        }

        public virtual void Hide()
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
