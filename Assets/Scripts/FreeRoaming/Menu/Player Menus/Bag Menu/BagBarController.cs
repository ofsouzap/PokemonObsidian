using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;

namespace FreeRoaming.Menu.PlayerMenus.BagMenu
{
    public abstract class BagBarController : MonoBehaviour
    {

        protected abstract GameObject[] GetItems();

        protected GameObject currentBorder;
        protected GameObject borderPrefab;

        protected BagMenuController menuController;

        public virtual void SetUp(BagMenuController menuController,
            GameObject borderPrefab)
        {

            this.menuController = menuController;
            this.borderPrefab = borderPrefab;

            TryDestroyBorder();

        }

        protected void TryDestroyBorder()
        {
            if (currentBorder != null)
                Destroy(currentBorder);
        }

        public virtual void SetCurrentSelectionIndex(int index)
        {

            TryDestroyBorder();

            currentBorder = Instantiate(borderPrefab, GetItems()[index].transform);

        }

    }
}
