using System;
using System.Collections.Generic;
using UnityEngine;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace FreeRoaming
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class DroppedItemController : ObstacleController, IInteractable
    {

        public const string getItemSoundFXName = "get_item";

        [Tooltip("The id of the dropped item which refers to its entry in the data file")]
        public int droppedItemId = 0;

        protected DroppedItem DroppedItem
            => DroppedItem.registry.LinearSearch(droppedItemId);

        protected Item GetItem()
            => Item.GetItemById(DroppedItem.ItemTypeId, DroppedItem.itemId);

        protected override void Start()
        {

            base.Start();

            RefreshEnabledState();

        }

        public void Interact(GameCharacterController interacter)
        {

            if (droppedItemId == 0)
            {
                Debug.LogError("Dropped item id unset for " + name);
                return;
            }
            
            //Give the player the item(s) if the scene is active
            if (sceneController.SceneIsActive)
            {
                TryGiveItemToPlayer();
            }

        }

        protected void TryGiveItemToPlayer()
        {
            
            if (PlayerData.singleton.GetDroppedItemHasBeenCollected(droppedItemId))
            {

                Debug.LogWarning($"Player has already collected dropped item (dropped item id {droppedItemId})");

            }
            else
            {

                PlayerController.singleton.ObtainItem(GetItem(), DroppedItem.quantity);
                PlayerData.singleton.SetDroppedItemCollected(droppedItemId);

                RefreshEnabledState();

            }

        }

        public void RefreshEnabledState()
        {

            if (PlayerData.singleton.GetDroppedItemHasBeenCollected(droppedItemId))
                Disable();
            else
                Enable();

        }

        /// <summary>
        /// Hides this and stops it occupying a position
        /// </summary>
        protected void Disable()
        {

            gameObject.SetActive(false);

        }

        /// <summary>
        /// Shows this and makes it occupy a position
        /// </summary>
        protected void Enable()
        {

            gameObject.SetActive(true);

        }

    }
}
