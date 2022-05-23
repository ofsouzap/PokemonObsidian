using System.Collections;
using UnityEngine;

namespace FreeRoaming
{
    public class MultiSpriteObstacleController : ObstacleController
    {

        [Tooltip("Sprites to switch between (in order)")]
        public Sprite[] sprites;

        private int currentIndex = 0;
        private float lastSwap = 0;

        [Tooltip("Speed for switching sprites in seconds")]
        public float speed = 0.75f;

        private float Delay => 1 / speed;

        protected override void Start()
        {

            base.Start();

            currentIndex = 0;

        }

        protected override void Update()
        {

            base.Update();

            if (sceneController.SceneIsActive)
            {
                if (Time.time >= lastSwap + Delay)
                {
                    MoveToNextSprite();
                    lastSwap = Time.time;
                }
            }

        }

        private void OnValidate()
        {
            if (sprites.Length > 0)
                spriteRenderer.sprite = sprites[0];
        }

        protected void MoveToNextSprite()
        {
            currentIndex = (currentIndex + 1) % sprites.Length;
            Sprite newSprite = sprites[currentIndex];
            spriteRenderer.sprite = sprites[currentIndex];
        }

    }
}