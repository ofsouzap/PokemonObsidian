using UnityEngine;
using Battle.PlayerUI;

namespace Menus
{
    public abstract class MenuController : MonoBehaviour
    {

        protected bool isShown;

        public virtual void Show()
        {

            isShown = true;

            gameObject.SetActive(true);

            foreach (MenuSelectableController selectableController in GetSelectables())
                selectableController.SetSelectedImageState(false);

        }

        public virtual void Hide()
        {

            isShown = false;

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
