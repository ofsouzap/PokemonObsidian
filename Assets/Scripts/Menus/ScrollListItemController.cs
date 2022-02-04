using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    [RequireComponent(typeof(Button))]
    public abstract class ScrollListItemController<T> : MonoBehaviour
    {

        public void SetPositionIndex(int index,
            float padding)
        {

            RectTransform rt = GetComponent<RectTransform>();

            float yAnchoredPos = -1 * (((2 * padding) + index * (rt.rect.height + padding)) + (rt.rect.height / 2));

            rt.anchoredPosition = new Vector2(
                rt.anchoredPosition.x,
                yAnchoredPos);

        }

        public abstract void SetValues(T vs);

    }
}