using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Menus;

namespace StartUp.ChoosePlayerData.ChoosePlayerSprite
{
    public class SpriteOptionController : GenericImageOptionController<string>
    {

        public override void RefreshImage()
        {

            bool flipSprite;
            image.sprite = SpriteStorage.GetCharacterSprite(out flipSprite, instanceId, "idle", FreeRoaming.GameCharacterController.FacingDirection.Down);

            int flipMultiplier = flipSprite ? -1 : 1;

            //If needing to flip sprite, make x-scale negative
            image.rectTransform.localScale = new Vector3(
                Mathf.Abs(image.rectTransform.localScale.x) * flipMultiplier,
                image.rectTransform.localScale.y,
                image.rectTransform.localScale.z
                );

        }

    }
}