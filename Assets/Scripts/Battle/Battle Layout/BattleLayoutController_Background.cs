using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.BattleLayout
{
    public partial class BattleLayoutController : MonoBehaviour
    {

        public Image backgroundSpriteRenderer;
        public SpriteRenderer[] battleCircleSpriteRenderers;

        /// <summary>
        /// Sets up the background and circles of the battle layout
        /// </summary>
        /// <param name="settingResourceName">The resource name of the background and the circles (eg. "mountain", "snow", "forest" etc.)</param>
        public void SetUpBackground(string settingResourceName)
        {

            Sprite backgroundSprite = SpriteStorage.GetBattleBackgroundSprite(settingResourceName);
            Sprite circleSprite = SpriteStorage.GetBattleCircleSprite(settingResourceName);

            backgroundSpriteRenderer.sprite = backgroundSprite;
            foreach (SpriteRenderer sr in battleCircleSpriteRenderers)
                sr.sprite = circleSprite;

        }

    }
}
