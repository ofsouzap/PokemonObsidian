using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Items;

namespace FreeRoaming.Menu.PlayerMenus.BagMenu
{
    public class ItemsListController : BagBarController
    {

        public Transform itemsListContentTransform;
        private Vector2 itemsListContentInitialAnchoredPosition;

        private GameObject[] itemGameObjects;
        protected override GameObject[] GetItems()
            => itemGameObjects;

        //This must have a ItemsListItemController component
        public GameObject itemListItemPrefab;

        [SerializeField]
        private float itemsPadding = 5;

        [SerializeField]
        private byte itemsNoScrollBuffer = 3;

        private int currentScrollIndex = 0;

        private void Start()
        {

            if (transform == itemsListContentTransform)
                Debug.LogWarning("Items list content transform shouldn't be set to items list controller's transform");

            itemsListContentInitialAnchoredPosition = itemsListContentTransform.GetComponent<RectTransform>().anchoredPosition;

        }

        public void SetItems(Item[] items,
            int[] quantities)
        {

            if (items.Length != quantities.Length)
            {
                Debug.LogError("Items array length not the same as quantities array length");
                return;
            }

            ClearList();

            itemGameObjects = new GameObject[items.Length];
            
            for (int i = items.Length - 1; i >= 0; i--) //Go in descending order so that first element is at top
            {
                itemGameObjects[i] = GenerateItemsListItem(items[i], quantities[i], i);
            }

            currentScrollIndex = 0;

        }

        private GameObject GenerateItemsListItem(Item item,
            int quantity,
            int index)
        {

            GameObject newItem = Instantiate(itemListItemPrefab, itemsListContentTransform);
            ItemsListItemController controller = newItem.GetComponent<ItemsListItemController>();

            newItem.GetComponent<Button>().onClick.AddListener(() => menuController.OnSelectItem(index));

            controller.SetPositionIndex(index, itemsPadding);
            controller.SetValues(item.itemName, quantity);

            return newItem;

        }

        private void ClearList()
        {

            if (itemGameObjects == null)
                return;

            if (itemGameObjects.Length <= 0)
                return;

            foreach (GameObject go in itemGameObjects)
                Destroy(go);

            itemGameObjects = new GameObject[0];

        }

        public override void SetCurrentSelectionIndex(int index)
        {

            base.SetCurrentSelectionIndex(index);

            //TODO - Check about editing currentScrollIndex. If changed, call RefreshItemOffsets

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

        private void RefreshItemOffset()
        {

            if (itemGameObjects.Length == 0)
                return;

            float itemHeight = itemGameObjects[0].GetComponent<RectTransform>().rect.height; //All item gameobjects are from same prefab so have same height

            itemsListContentTransform.GetComponent<RectTransform>().anchoredPosition = itemsListContentInitialAnchoredPosition
                + Vector2.up * itemHeight * currentScrollIndex;

        }

    }
}