using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FreeRoaming.Menu.StorageSystem
{
    [RequireComponent(typeof(Selectable))]
    public abstract class PointerSelectable : MonoBehaviour, ISelectHandler, IDeselectHandler
    {

        [Tooltip("A position for the pointer to copy the position of when it should be over this pokemon position")]
        public Transform pointerPositionTransform;

        /// <summary>
        /// The game object for the pointer. When this pokemon position is selected, this pointer will be moved to this game object
        /// </summary>
        protected PointerController pointer;
        public void SetPointerGameObject(PointerController pointer) => this.pointer = pointer;

        public virtual void OnSelect(BaseEventData eventData)
        {
            pointer.Show();
            pointer.SetPosition(pointerPositionTransform.position);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            pointer.Hide();
        }

    }
}