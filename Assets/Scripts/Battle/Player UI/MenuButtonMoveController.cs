using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Menus;

namespace Battle.PlayerUI
{
    public class MenuButtonMoveController : MenuSelectableController, IPointerEnterHandler, IPointerExitHandler
    {

        public Button Button
        {
            get => GetComponent<Button>();
        }

        public Text textName;

        public UnityEvent MoveSelected = new UnityEvent();
        public UnityEvent MoveDeselected = new UnityEvent();

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            MoveSelected.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            MoveDeselected.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData) => MoveSelected.Invoke();
        public void OnPointerExit(PointerEventData eventData) => MoveDeselected.Invoke();

        public void SetName(string name) => textName.text = name;

        public void SetInteractable(bool state)
        {

            if (!state)
                textName.text = "";
            Button.interactable = state;

        }

    }
}