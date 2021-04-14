using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FreeRoaming.Menu;

namespace Menus
{
    [RequireComponent(typeof(Selectable))]
    public class MenuSelectableController : MonoBehaviour, ISelectHandler, IDeselectHandler
    {

        private bool selectedImageActive = false;
        private GameObject selectedImageGameObject = null;

        private void Start()
        {

            InputMethodMonitor.singleton.InputMethodChanged.AddListener((o) => RefreshSelectedImage());
            RefreshSelectedImage();

        }

        private void TryShowSelectedImage()
        {
            if (InputMethodMonitor.singleton.CurrentInputMethod != InputMethodMonitor.InputMethod.Mouse)
                ShowSelectedImage();
        }

        private void ShowSelectedImage()
        {
            HideSelectedImage();
            selectedImageGameObject = Instantiate(FreeRoamMenuController.singleton.selectableSelectionPrefab,
                    transform);
        }

        private void HideSelectedImage()
        {
            if (selectedImageGameObject != null)
                Destroy(selectedImageGameObject);
        }

        private void RefreshSelectedImage()
        {
            if (selectedImageActive)
                TryShowSelectedImage();
            else
                HideSelectedImage();
        }

        public void SetSelectedImageState(bool state)
        {

            selectedImageActive = state;
            RefreshSelectedImage();

        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            SetSelectedImageState(true);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            SetSelectedImageState(false);
        }

    }
}