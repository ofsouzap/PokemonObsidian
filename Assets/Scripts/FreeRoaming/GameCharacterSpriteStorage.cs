using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using FreeRoaming;

namespace FreeRoaming
{
    public static class GameCharacterSpriteStorage
    {

        /// <summary>
        /// A dictionary storing all the sprites loaded.
        /// The keys are identifiers for the sprites in the format (using the '?' regex quantifier)
        ///     "{spriteName}_{state}(_{direction})?(_{index})?"
        ///     Where everything is lower case, {spriteName} is the name of the sprite, {state} is the state of the sprite (eg. walking, neutral), {direction} is the direction of the sprite which may be "up","down","left" or "right" and {index} is the index of that sprite. for example, there are two sprites for a walking character
        /// The values are the sprite instances
        /// </summary>
        private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        private static bool loaded = false;

        /// <summary>
        /// Load the sprites if they haven't already been loaded
        /// </summary>
        public static void TryLoad()
        {
            if (!loaded)
                LoadAll();
        }

        private static void LoadAll()
        {

            List<Sprite> spritesToStore = new List<Sprite>();

            spritesToStore.AddRange(LoadAllInDefaultDirectory());
            spritesToStore.AddRange(LoadSpriteSheet("sprite_sheet_npcs"));
            spritesToStore.AddRange(LoadSpriteSheet("sprite_sheet_player_male"));
            spritesToStore.AddRange(LoadSpriteSheet("sprite_sheet_player_female"));

            foreach (Sprite sprite in spritesToStore)
            {

                //TODO - make sure that sprite.name is the name that I am hoping that it is
                if (!sprites.ContainsKey(sprite.name))
                {
                    sprites.Add(sprite.name, sprite);
                }
                else
                {
                    Debug.LogError("Duplicate sprite name found - " + sprite.name);
                }

            }

            loaded = true;

        }

        private static string GenerateIdentifier(string spriteName,
            string stateIdentifier,
            GameCharacterController.FacingDirection direction,
            int index = -1)
        {

            string directionIdentifier;

            switch (direction)
            {

                case GameCharacterController.FacingDirection.Down:
                    directionIdentifier = "d";
                    break;

                case GameCharacterController.FacingDirection.Left:
                    directionIdentifier = "l";
                    break;

                case GameCharacterController.FacingDirection.Up:
                    directionIdentifier = "u";
                    break;

                default:
                    Debug.LogWarning($"Invalid direction facing ({direction})");
                    directionIdentifier = "l";
                    break;

            }

            return spriteName + '_' + stateIdentifier + '_' + directionIdentifier + (index == -1 ? "" : '_' + index.ToString());

        }

        /// <summary>
        /// Get a sprite referenced by a full identifier
        /// </summary>
        /// <param name="fullIdentifier">The full identifier as explained for Sprite.sprites</param>
        /// <returns>The sprite as specified if found, otherwise null</returns>
        private static Sprite Get(string fullIdentifier)
        {

            if (sprites.ContainsKey(fullIdentifier))
                return sprites[fullIdentifier];
            else
                return null;

        }

        /// <summary>
        /// Gets a sprite referenced by a state identifier and a FacingDirection direction. The full identifier will then be formed from these parameters
        /// </summary>
        /// /// <param name="flipSprite">Whether the sprite should be flipped once returned</param>
        /// <param name="spriteName">The name of the sprite</param>
        /// <param name="stateIdentifier">The name of the state that is being requested (eg. "neutral")</param>
        /// <param name="direction">The direction of sprite to request</param>
        /// <param name="index">The index of the sprite to request</param>
        /// <returns>The sprite as specified if found, otherwise null</returns>
        public static Sprite Get(out bool flipSprite,
            string spriteName,
            string stateIdentifier,
            GameCharacterController.FacingDirection direction,
            int index = -1)
        {

            string identifier = GenerateIdentifier(spriteName,
                stateIdentifier,
                direction == GameCharacterController.FacingDirection.Right ? GameCharacterController.FacingDirection.Left : direction,
                index);

            flipSprite = direction == GameCharacterController.FacingDirection.Right;

            return Get(identifier);

        }

        /// <summary>
        /// Load a sprite with a specified full identifier
        /// </summary>
        /// <param name="fullIdentifier">The full identifier</param>
        /// <returns>The sprite loaded</returns>
        private static Sprite[] LoadAllInDefaultDirectory()
        {

            string resourcePath = Path.Combine("Sprites", "Characters");

            return Resources.LoadAll<Sprite>(resourcePath);

        }

        /// <summary>
        /// Load all sprites from a sprite sheet
        /// </summary>
        /// <param name="spriteSheetName">The name of the sprite sheet. The sheet should be in Resources/Sprites</param>
        /// <returns>The array of sprites loaded from the sheet</returns>
        public static Sprite[] LoadSpriteSheet(string spriteSheetName)
        {
            return Resources.LoadAll<Sprite>(Path.Combine("Sprites", spriteSheetName));
        }

    }
}
