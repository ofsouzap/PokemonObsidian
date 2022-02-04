using System;
using UnityEngine;

namespace Menus
{
    public abstract class ScrollListController<T> : MonoBehaviour
    {

        public Transform itemsListContentTransform;
        protected Vector2 itemsListContentInitialAnchoredPosition;

        protected GameObject[] itemGameObjects;
        public GameObject[] GetItems()
            => itemGameObjects;

        //This must have a ScrollListItemController<T> component
        public GameObject scrollListItemPrefab;

        [SerializeField]
        protected float itemsPadding = 5;

        [SerializeField]
        protected byte itemsNoScrollBuffer = 3;

        protected int currentScrollIndex = 0;

        protected GameObject currentBorder;
        protected GameObject borderPrefab;

        protected Action<int> itemSelectedAction;

        protected void Start()
        {

            if (transform == itemsListContentTransform)
                Debug.LogWarning("Items list content transform shouldn't be set to items list controller's transform");

            itemsListContentInitialAnchoredPosition = itemsListContentTransform.GetComponent<RectTransform>().anchoredPosition;

        }

        public virtual void SetUp(GameObject borderPrefab,
            Action<int> itemSelectedAction)
        {

            this.borderPrefab = borderPrefab;
            this.itemSelectedAction = itemSelectedAction;

            TryDestroyBorder();

        }

        public void SetItems(T[] items)
        {

            ClearList();

            itemGameObjects = new GameObject[items.Length];

            for (int i = items.Length - 1; i >= 0; i--) //Go in descending order so that first element is at top
            {
                itemGameObjects[i] = GenerateListItem(items[i], i);
            }

            currentScrollIndex = 0;

        }

        protected abstract GameObject GenerateListItem(T item, int index);

        protected void ClearList()
        {

            if (itemGameObjects == null)
                return;

            if (itemGameObjects.Length <= 0)
                return;

            foreach (GameObject go in itemGameObjects)
                Destroy(go);

            itemGameObjects = new GameObject[0];

        }

        protected void TryDestroyBorder()
        {
            if (currentBorder != null)
                Destroy(currentBorder);
        }

        public void SetCurrentSelectionIndex(int index)
        {

            TryDestroyBorder();

            currentBorder = Instantiate(borderPrefab, GetItems()[index].transform);

            int prospectiveScrollIndex = index - itemsNoScrollBuffer;

            if (prospectiveScrollIndex >= 0)
            {

                currentScrollIndex = prospectiveScrollIndex;
                RefreshItemOffset();

            }
            else
            {
                currentScrollIndex = 0;
                RefreshItemOffset();
            }

        }

        protected void RefreshItemOffset()
        {

            if (itemGameObjects.Length == 0)
                return;

            float itemHeight = itemGameObjects[0].GetComponent<RectTransform>().rect.height; //All item gameobjects are from same prefab so have same height

            itemsListContentTransform.GetComponent<RectTransform>().anchoredPosition = itemsListContentInitialAnchoredPosition
                + Vector2.up * (itemHeight + itemsPadding) * currentScrollIndex;

        }

    }
}