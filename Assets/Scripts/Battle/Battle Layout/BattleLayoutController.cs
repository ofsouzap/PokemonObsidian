using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Battle;

namespace Battle.BattleLayout
{
    public class BattleLayoutController : MonoBehaviour
    {

        public float pokemonSpriteOffScreenLeftLocalPositionX;
        public float pokemonSpriteOffScreenRightLocalPositionX;

        public OverviewPaneManager overviewPaneManager;

        public GameObject playerPokemonSprite;
        private float playerPokemonSpriteRootX;
        public GameObject opponentPokemonSprite;
        private float opponentPokemonSpriteRootX;

        #region Constants

        #region Constant Timings

        public const float retractPokemonTime = 0.3F;
        public const float sendInPokemonTime = 0.5F;

        /// <summary>
        /// How long it should take to change the health bar for each individual health point
        /// </summary>
        public const float healthBarChangeRate = 0.05F;

        /// <summary>
        /// The time between a sprite disappearing then appearing again when flashing (eg. to show damage has been taken)
        /// </summary>
        public const float spriteFlashInterval = 0.1F;

        /// <summary>
        /// The time between the pokemon being at the top and it being at the bottom
        /// </summary>
        public const float playerPokemonBobbingInterval = 0.4F;

        /// <summary>
        /// The time between the pokemon being at the top and it being at the bottom when the movement should be slowed (eg. if the pokemon has a non-volatile status condition)
        /// </summary>
        public const float slowedPlayerPokemonBobbingInterval = 0.8F;

        #endregion

        #region Other Constants

        public const int pokemonSpriteFlashIterations = 3;
        public const float playerPokemonBobbingDistance = 0.1F;

        #endregion

        #endregion

        private void Start()
        {

            if (playerPokemonSprite.GetComponent<SpriteRenderer>() == null)
                Debug.LogError("No SpriteRenderer component found for playerPokemonObject");

            if (opponentPokemonSprite.GetComponent<SpriteRenderer>() == null)
                Debug.LogError("No SpriteRenderer component found for opponentPokemonObject");

            playerPokemonSpriteRootX = playerPokemonSprite.transform.localPosition.x;
            opponentPokemonSpriteRootX = opponentPokemonSprite.transform.localPosition.x;

        }

        public void HidePokemonAndPanes()
        {
            playerPokemonSprite.SetActive(false);
            opponentPokemonSprite.SetActive(false);
            overviewPaneManager.HidePanes();
        }

        #region Player Pokemon Bobbing

        public void StartPlayerPokemonBobbing(bool slowed) => playerPokemonBobbingCoroutine = StartCoroutine(PlayerPokemonBobbing(slowed));
        public void StopPlayerPokemonBobbing()
        {
            playerPokemonSprite.transform.localPosition = new Vector3(
                playerPokemonSprite.transform.localPosition.x,
                playerBobbingBaseLocalHeight,
                playerPokemonSprite.transform.localPosition.z
                );
            if (playerPokemonBobbingCoroutine != null)
                StopCoroutine(playerPokemonBobbingCoroutine);
        }

        private float playerBobbingBaseLocalHeight;

        private Coroutine playerPokemonBobbingCoroutine;

        private IEnumerator PlayerPokemonBobbing(bool slowed)
        {

            playerBobbingBaseLocalHeight = playerPokemonSprite.transform.localPosition.y;

            float startTime = Time.time;
            float interval = !slowed ? playerPokemonBobbingInterval : slowedPlayerPokemonBobbingInterval;

            while (true)
            {

                float time = Time.time - startTime;
                float currentHeight = (playerPokemonBobbingDistance * (Mathf.Abs(((time / interval) % 2) - 1) - 1)) + playerBobbingBaseLocalHeight;

                playerPokemonSprite.transform.localPosition = new Vector3(
                    playerPokemonSprite.transform.localPosition.x,
                    currentHeight,
                    playerPokemonSprite.transform.localPosition.z
                );

                //Waiting for this long will help with the low-quality graphics effect
                yield return new WaitForSeconds(0.1F);

            }

        }

        #endregion

        #region Pokemon Retracting

        private IEnumerator RetractPokemon(GameObject pokemonObject)
        {

            float startTime = Time.time;
            float endTime = startTime + retractPokemonTime;

            Vector2 initialScale = pokemonObject.transform.localScale;

            while (true)
            {

                float timeFactor = (Time.time - startTime) / retractPokemonTime;

                pokemonObject.transform.localScale = new Vector2(
                    Mathf.Lerp(initialScale.x, 0, timeFactor),
                    Mathf.Lerp(initialScale.y, 0, timeFactor)
                );

                if (Time.time >= endTime)
                    break;

                yield return new WaitForFixedUpdate();

            }

            pokemonObject.transform.localScale = initialScale;
            pokemonObject.SetActive(false);

        }

        public IEnumerator RetractPlayerPokemon()
        {
            yield return StartCoroutine(RetractPokemon(playerPokemonSprite));
            yield return StartCoroutine(overviewPaneManager.UnRevealPlayerOverviewPane());
        }

        public IEnumerator RetractOpponentPokemon()
        {
            yield return StartCoroutine(RetractPokemon(opponentPokemonSprite));
            yield return StartCoroutine(overviewPaneManager.UnRevealOpponentOverviewPane());
        }

        #endregion

        #region Pokemon Sending Out

        private IEnumerator SendInPokemon(Sprite pokemonSprite, GameObject spriteObject, float rootXPos, float offScreenXPos)
        {

            spriteObject.GetComponent<SpriteRenderer>().sprite = pokemonSprite;
            spriteObject.transform.localPosition = new Vector3(
                offScreenXPos,
                spriteObject.transform.localPosition.y,
                spriteObject.transform.localPosition.z
                );
            spriteObject.SetActive(true);

            float startTime = Time.time;
            float endTime = Time.time + sendInPokemonTime;

            while (true)
            {

                if (Time.time >= endTime)
                    break;

                float timeFactor = (Time.time - startTime) / sendInPokemonTime;

                spriteObject.transform.localPosition = new Vector3(
                    Mathf.Lerp(offScreenXPos,
                        rootXPos,
                        timeFactor),
                    spriteObject.transform.localPosition.y,
                    spriteObject.transform.localPosition.z
                    );

                yield return new WaitForFixedUpdate();

            }

            spriteObject.transform.localPosition = new Vector3(
                rootXPos,
                spriteObject.transform.localPosition.y,
                spriteObject.transform.localPosition.z
                );

        }

        public IEnumerator SendInPlayerPokemon(PokemonInstance pokemon)
        {
            overviewPaneManager.playerPokemonOverviewPaneController.FullUpdate(pokemon);
            yield return StartCoroutine(SendInPokemon(pokemon.LoadSprite(PokemonSpecies.SpriteType.Back), playerPokemonSprite, playerPokemonSpriteRootX, pokemonSpriteOffScreenLeftLocalPositionX));
            yield return StartCoroutine(overviewPaneManager.RevealPlayerOverviewPane());
        }

        public IEnumerator SendInOpponentPokemon(PokemonInstance pokemon)
        {
            overviewPaneManager.opponentPokemonOverviewPaneController.FullUpdate(pokemon);
            yield return StartCoroutine(SendInPokemon(pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1), opponentPokemonSprite, opponentPokemonSpriteRootX, pokemonSpriteOffScreenRightLocalPositionX));
            yield return StartCoroutine(overviewPaneManager.RevealOpponentOverviewPane());
        }

        #endregion

        #region Pokemon Damage Taking

        private IEnumerator FlashSprite(GameObject sprite,
            int iterations)
        {

            bool flashState = true;

            for (int i = 0; i < iterations; i++)
            {

                flashState = !flashState;

                sprite.SetActive(flashState);

                yield return new WaitForSeconds(spriteFlashInterval);

            }

            sprite.SetActive(true);

        }

        private IEnumerator GradualHealthBarChange(Action<int> updateBarAction,
            int startValue,
            int newValue,
            float timeToTake)
        {

            float startTime = Time.time;
            float endTime = Time.time + timeToTake;

            while (true)
            {

                if (Time.time >= endTime)
                    break;

                float timeFactor = (Time.time - startTime) / timeToTake;

                updateBarAction(Mathf.RoundToInt(Mathf.Lerp(
                    startValue,
                    newValue,
                    timeFactor)));

                yield return new WaitForFixedUpdate();

            }

            updateBarAction(newValue);

        }

        public IEnumerator TakeDamagePlayerPokemon(int newHealth,
            int startHealth,
            int maxHealth)
        {

            yield return StartCoroutine(FlashSprite(playerPokemonSprite, pokemonSpriteFlashIterations));

            yield return StartCoroutine(GradualHealthBarChange(
                (int value) => overviewPaneManager.playerPokemonOverviewPaneController.UpdateHealthBar(value, maxHealth),
                startHealth,
                newHealth,
                healthBarChangeRate * Mathf.Abs(newHealth - startHealth)
            ));

        }

        public IEnumerator TakeDamageOpponentPokemon(int newHealth,
            int startHealth,
            int maxHealth)
        {

            yield return StartCoroutine(FlashSprite(opponentPokemonSprite, pokemonSpriteFlashIterations));

            yield return StartCoroutine(GradualHealthBarChange(
                (int value) => overviewPaneManager.opponentPokemonOverviewPaneController.UpdateHealthBar(value, maxHealth),
                startHealth,
                newHealth,
                healthBarChangeRate * Mathf.Abs(newHealth - startHealth)
            ));

        }

        #endregion

        //TODO - have methods for each type of animation that this may need to deal with

    }
}
