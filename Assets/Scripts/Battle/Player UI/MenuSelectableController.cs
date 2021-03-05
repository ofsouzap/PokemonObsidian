using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Battle.PlayerUI
{
    [RequireComponent(typeof(Selectable))]
    public class MenuSelectableController : MonoBehaviour, ISelectHandler, IDeselectHandler
    {

        private GameObject selectedImageGameObject;

        public void ShowSelectedImage()
        {
            selectedImageGameObject = Instantiate(PlayerBattleUIController.singleton.selectableSelectionPrefab,
                transform);
        }

        public void HideSelectedImage()
        {
            Destroy(selectedImageGameObject);
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            ShowSelectedImage();
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            HideSelectedImage();
        }

    }
}