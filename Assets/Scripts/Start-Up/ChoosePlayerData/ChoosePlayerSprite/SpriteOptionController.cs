using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Menus;

namespace StartUp.ChoosePlayerData.ChoosePlayerSprite
{
    public class SpriteOptionController : MenuSelectableController
    {

        public UnityEvent OnClick = new UnityEvent();

        public Image spriteImage;
        public Button spriteButton;
        public string spriteName;

        protected override void Start()
        {

            base.Start();
            
            RefreshImage();

            spriteButton.onClick.AddListener(() => OnClick.Invoke());

        }

        public void RefreshImage()
        {

            bool flipSprite;
            spriteImage.sprite = SpriteStorage.GetCharacterSprite(out flipSprite, spriteName, "idle", FreeRoaming.GameCharacterController.FacingDirection.Down);

            int flipMultiplier = flipSprite ? -1 : 1;

            //If needing to flip sprite, make x-scale negative
            spriteImage.rectTransform.localScale = new Vector3(
                Mathf.Abs(spriteImage.rectTransform.localScale.x) * flipMultiplier,
                spriteImage.rectTransform.localScale.y,
                spriteImage.rectTransform.localScale.z
                );

        }

    }
}