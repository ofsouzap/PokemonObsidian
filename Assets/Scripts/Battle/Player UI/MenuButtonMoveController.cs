using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace Battle.PlayerUI
{
    public class MenuButtonMoveController : MenuSelectableController, IPointerEnterHandler, IPointerExitHandler
    {

        public int moveIndex;
        public UnityEvent MoveSelected;
        public UnityEvent MoveDeselected;

        private void Awake()
        {
            MoveSelected = new UnityEvent();
            MoveDeselected = new UnityEvent();
        }

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
    }
}