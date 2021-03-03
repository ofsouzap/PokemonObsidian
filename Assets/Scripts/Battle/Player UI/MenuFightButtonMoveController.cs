using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MenuFightButtonMoveController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{

    public int moveIndex;
    public UnityEvent MoveSelected;
    public UnityEvent MoveDeselected;

    private void Awake()
    {
        MoveSelected = new UnityEvent();
        MoveDeselected = new UnityEvent();
    }

    public void OnSelect(BaseEventData eventData) => MoveSelected.Invoke();
    public void OnDeselect(BaseEventData eventData) => MoveDeselected.Invoke();

    public void OnPointerEnter(PointerEventData eventData) => MoveSelected.Invoke();
    public void OnPointerExit(PointerEventData eventData) => MoveDeselected.Invoke();
}
