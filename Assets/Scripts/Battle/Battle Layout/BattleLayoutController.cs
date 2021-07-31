using System;
using System.Collections;
using UnityEngine;
using Pokemon;
using Pokemon.Moves;
using Items.PokeBalls;
using Audio;

namespace Battle.BattleLayout
{
    public partial class BattleLayoutController : MonoBehaviour
    {

        public float pokemonSpriteOffScreenLeftLocalPositionX;
        public float pokemonSpriteOffScreenRightLocalPositionX;

        public OverviewPaneManager overviewPaneManager;

        public GameObject playerPokemonSprite;
        private float playerPokemonSpriteRootX;
        public GameObject opponentPokemonSprite;
        private float opponentPokemonSpriteRootX;

        public GameObject playerPokemonMoveParticleSystemObject;
        public GameObject opponentPokemonMoveParticleSystemObject;

        public GameObject opponentTrainerSprite;
        private float opponentTrainerSpriteRootX;

        public GameObject playerPokeBallSprite;
        public GameObject opponentPokeBallSprite;

        public GameObject pokeBallThrowGameObject;
        public Transform pokeBallThrowStartPosition;
        public Transform pokeBallThrowEndPosition;
        public Transform pokeBallThrowDroppedPosition;
        public AnimationCurve pokeBallThrowHeightOffsetCurve;

        public PokeBallLineController playerPokeBallLineController;
        public float playerPokeBallLineRootX;
        public float playerPokeBallLineOffScreenLocalXDistance;
        public PokeBallLineController opponentPokeBallLineController;
        public float opponentPokeBallLineRootX;
        public float opponentPokeBallLineOffScreenLocalXDistance;

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

        /// <summary>
        /// The time that it should take for the experience bar to change
        /// </summary>
        public const float experienceBarChangeTime = 0.7F;

        /// <summary>
        /// The time to wait after the experience bar has changed before continuing
        /// </summary>
        public const float experienceBarChangeEndWaitTime = 0.3F;

        public const float opponentPokemonEntranceAnimationSpriteChangeDelay = 0.5F;

        #region Generic Moves

        public const float genericMoveReturnBackTime = 0.2F;

        public const float genericPhysicalMoveLeanBackTime = 0.2F;

        public const float genericPhysicalMoveLungeTime = 0.1F;

        public const float genericPhysicalMovePauseTime = 0.5F;

        public const float genericPhysicalMoveTargetJerkBackTime = 0.1F;

        public const float genericPhysicalMoveTargetReturnTime = 0.2F;

        public const float genericSpecialMoveLungeTime = 0.15F;

        #endregion

        public const float statStageChangeTotalTime = 0.5F;

        /// <summary>
        /// The time it should take for a showcased opponent trainer to move from off-screen to its destination and the inverse
        /// </summary>
        public const float opponentTrainerShowcaseMovementTime = 0.75F;

        #region Poke Ball Opening

        public const float pokeBallOpeningNeutralTime = 0.4F;
        public const float pokeBallOpeningSquashedTime = 0.2F;

        #endregion

        public const float pokemonEmergeTime = 0.3F;

        #region Poke Ball Using

        public const float pokeBallUseThrowTime = 0.7F;
        public const float pokeBallUseThrowToOpenDelay = 0.5F;
        public const float pokeBallUsePokemonEnterPokeBallTime = 0.4F;
        public const float pokeBallUseOpenClampInterval = 0.2F;
        public const float pokeBallUsePokeBallDropTime = 0.4F;

        public const float pokeBallUseWobbleChangeInterval = 0.2F;
        public const float pokeBallUseWobbleDelay = 1;
        public const float pokeBallUseWobbleDelayIncrease = 1;
        public const float pokeBallUseWobbleEndPauseTime = 0.5F;

        #endregion

        public const float pokeBallLineShowTime = 0.3f;
        public const float pokeBallLineHideTime = 0.3f;

        #endregion

        #region Sound FX

        private const string physicalMoveSoundFXName = "move_physical";
        private const string specialMoveSoundFXName = "move_special";
        private const string statusMoveSoundFXName = "move_status";
        private const string statChangeUpSoundFXName = "stat_change_up";
        private const string statChangeDownSoundFXName = "stat_change_down";

        #endregion

        #region Other Constants

        public const int pokemonSpriteFlashIterations = 3;

        public const float playerPokemonBobbingDistance = 0.1F;

        public const float genericPhysicalMoveLeanBackDistance = 1;
        public const float genericMoveLungeDistance = 1;
        public const float genericPhysicalMoveTargetJerkBackDistance = 1.5F;

        public const float statStageChangeScaleFactor = 1.25F;

        public static readonly Color statStageChangeIncreaseColor = new Color(1, 0.66F, 0.23F);
        public static readonly Color statStageChangeDecreaseColor = new Color(0.4F, 0.4F, 1);

        public const float pokeBallThrowMaximumHeightOffset = 2;
        public const float pokeBallWobbleRotationAngle = 10;

        #endregion

        #endregion

        private void Start()
        {

            if (playerPokemonSprite.GetComponent<SpriteRenderer>() == null)
                Debug.LogError("No SpriteRenderer component found for playerPokemonObject");

            if (opponentPokemonSprite.GetComponent<SpriteRenderer>() == null)
                Debug.LogError("No SpriteRenderer component found for opponentPokemonObject");

            if (playerPokemonMoveParticleSystemObject.GetComponent<ParticleSystem>() == null)
                Debug.LogError("No ParticleSystem component found for playerPokemonMoveParticleSystemObject");

            if (opponentPokemonMoveParticleSystemObject.GetComponent<ParticleSystem>() == null)
                Debug.LogError("No ParticleSystem component found for opponentPokemonMoveParticleSystemObject");

            if (opponentTrainerSprite.GetComponent<SpriteRenderer>() == null)
                Debug.LogError("No SpriteRenderer component found for opponentTrainerSprite");

            if (playerPokeBallSprite.GetComponent<SpriteRenderer>() == null)
                Debug.LogError("No SpriteRenderer component found for playerPokeBallSprite");

            if (opponentPokeBallSprite.GetComponent<SpriteRenderer>() == null)
                Debug.LogError("No SpriteRenderer component found for opponentPokeBallSprite");

            playerPokemonSpriteRootX = playerPokemonSprite.transform.localPosition.x;
            opponentPokemonSpriteRootX = opponentPokemonSprite.transform.localPosition.x;

            opponentTrainerSpriteRootX = opponentTrainerSprite.transform.localPosition.x;

        }

        public void HidePokemonAndPanes()
        {
            playerPokemonSprite.SetActive(false);
            opponentPokemonSprite.SetActive(false);
            opponentTrainerSprite.SetActive(false);
            playerPokeBallSprite.SetActive(false);
            opponentPokeBallSprite.SetActive(false);
            pokeBallThrowGameObject.SetActive(false);
            overviewPaneManager.HidePanes();
            HidePlayerPokeBallLineInstant();
            HideOpponentPokeBallLineInstant();
        }

        public void UpdatePlayerPokemon(PokemonInstance pokemon)
        {

            playerPokemonSprite.GetComponent<SpriteRenderer>().sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Back);

            overviewPaneManager.playerPokemonOverviewPaneController.FullUpdate(pokemon);

        }

        private IEnumerator GradualEffect(Action<float> effect,
            float timeToTake,
            float refreshTime = 0)
        {

            float startTime = Time.time;
            float endTime = startTime + timeToTake;

            while (true)
            {

                if (Time.time >= endTime)
                    break;

                float timeFactor = (Time.time - startTime) / timeToTake;

                effect(timeFactor);

                if (refreshTime == 0)
                    yield return new WaitForFixedUpdate();
                else
                    yield return new WaitForSeconds(refreshTime);

            }

        }

        /// <summary>
        /// Gradually move a game object from its current position to another position
        /// </summary>
        /// <param name="gameObject">The game object to move</param>
        /// <param name="endPos">The local position to move to</param>
        /// <param name="timeToTake">The time the move should take</param>
        /// <param name="refreshTime">The time to wait before refreshing the object's position. 0 means to refresh each frame</param>
        /// <returns></returns>
        private IEnumerator GradualTranslateLocalPosition(GameObject gameObject,
            Vector3 endPos,
            float timeToTake,
            float refreshTime = 0)
        {

            Vector3 startPos = gameObject.transform.localPosition;

            yield return StartCoroutine(GradualEffect(
                (t) => gameObject.transform.localPosition = Vector3.Lerp(startPos, endPos, t),
                timeToTake,
                refreshTime));

            gameObject.transform.localPosition = endPos;

        }

        private IEnumerator GradualTranslateGlobalPosition(GameObject gameObject,
            Vector3 endPos,
            float timeToTake,
            float refreshTime = 0)
        {

            Vector3 startPos = gameObject.transform.position;

            yield return StartCoroutine(GradualEffect(
                (t) => gameObject.transform.position = Vector3.Lerp(startPos, endPos, t),
                timeToTake,
                refreshTime));

            gameObject.transform.position = endPos;

        }

        private IEnumerator GradualTranslateAnchoredPosition(GameObject gameObject,
            Vector2 endPos,
            float timeToTake,
            float refreshTime = 0)
        {

            if (gameObject.GetComponent<RectTransform>() == null)
            {
                Debug.LogError("No RectTransform component on provided game object");
                yield break;
            }

            Vector2 startPos = gameObject.GetComponent<RectTransform>().anchoredPosition;

            yield return StartCoroutine(GradualEffect(
                (t) => gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(startPos, endPos, t),
                timeToTake,
                refreshTime));

            gameObject.GetComponent<RectTransform>().anchoredPosition = endPos;

        }

        /// <summary>
        /// Gradually scale a game object from its current scale to another scale
        /// </summary>
        /// <param name="gameObject">The game object to scale</param>
        /// <param name="endScale">The local scale to scale to</param>
        /// <param name="timeToTake">The time the scale should take</param>
        /// <param name="refreshTime">The time to wait before refreshing the object's scale. 0 means to refresh each frame</param>
        /// <returns></returns>
        private IEnumerator GradualChangeScale(GameObject gameObject,
            Vector3 endScale,
            float timeToTake,
            float refreshTime = 0)
        {

            Vector3 startScale = gameObject.transform.localScale;

            yield return StartCoroutine(GradualEffect(
                (t) => { gameObject.transform.localScale = Vector3.Lerp(startScale, endScale, t); },
                timeToTake,
                refreshTime));

            gameObject.transform.localScale = endScale;

        }

        private IEnumerator GameObjectSingleShake(GameObject gameObject,
            Vector3 shakeDirection,
            float distance,
            float timeToTake,
            float refreshTime = 0)
        {

            Vector3 startPos = gameObject.transform.localPosition;

            yield return StartCoroutine(GradualEffect(
                (t) =>
                {

                    float currentOffsetFactor = Mathf.FloorToInt(t * 4) switch
                    {
                        0 => 4 * t,
                        1 => (-4 * t) + 2,
                        2 => (-4 * t) + 2,
                        3 => (4 * t) - 4,
                        _ => 0
                    };

                    float currentOffset = currentOffsetFactor * distance;

                    gameObject.transform.localPosition = startPos + (shakeDirection.normalized * currentOffset);

                },
                timeToTake,
                refreshTime
            ));

            gameObject.transform.localPosition = startPos;

        }

        private IEnumerator GameObjectShake(GameObject gameObject,
            Vector3 shakeDirection,
            float distance,
            ushort shakeCount,
            float timeToTake,
            float refreshTime = 0)
        {

            float singleShakeTime = timeToTake / shakeCount;

            for (int i = 0; i < shakeCount; i++)
                yield return StartCoroutine(GameObjectSingleShake(gameObject, shakeDirection, distance, singleShakeTime, refreshTime));

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

        #region Poke Ball Line Display

        private void HidePlayerPokeBallLineInstant()
        {
            playerPokeBallLineController.GetComponent<RectTransform>().anchoredPosition = new Vector2
            (
                playerPokeBallLineRootX - playerPokeBallLineOffScreenLocalXDistance,
                playerPokeBallLineController.GetComponent<RectTransform>().anchoredPosition.y
            );
        }

        private void HideOpponentPokeBallLineInstant()
        {
            opponentPokeBallLineController.GetComponent<RectTransform>().anchoredPosition = new Vector2
            (
                opponentPokeBallLineRootX - opponentPokeBallLineOffScreenLocalXDistance,
                opponentPokeBallLineController.GetComponent<RectTransform>().anchoredPosition.y
            );
        }

        private IEnumerator DisplayPokeBallLine(PokeBallLineController controller,
            float targetX)
        {

            Vector2 targetAnchoredPosition = controller.GetComponent<RectTransform>().anchoredPosition;
            targetAnchoredPosition.x = targetX;

            yield return GradualTranslateAnchoredPosition(controller.gameObject,
                targetAnchoredPosition,
                pokeBallLineShowTime);

        }

        private IEnumerator DisplayPlayerPokeBallLine(PokeBallLineController.BallState[] states)
        {
            HidePlayerPokeBallLineInstant();
            playerPokeBallLineController.SetStates(states);
            yield return StartCoroutine(DisplayPokeBallLine(playerPokeBallLineController, playerPokeBallLineRootX));
        }

        private IEnumerator DisplayOpponentPokeBallLine(PokeBallLineController.BallState[] states)
        {
            HideOpponentPokeBallLineInstant();
            opponentPokeBallLineController.SetStates(states);
            yield return StartCoroutine(DisplayPokeBallLine(opponentPokeBallLineController, opponentPokeBallLineRootX));
        }

        private IEnumerator HidePokeBallLine(PokeBallLineController controller,
            float targetX)
        {

            Vector2 targetAnchoredPosition = controller.GetComponent<RectTransform>().anchoredPosition;
            targetAnchoredPosition.x = targetX;

            yield return GradualTranslateAnchoredPosition(controller.gameObject,
                targetAnchoredPosition,
                pokeBallLineHideTime);

        }

        private IEnumerator HidePlayerPokeBallLine()
            => HidePokeBallLine(playerPokeBallLineController,
                playerPokeBallLineRootX - playerPokeBallLineOffScreenLocalXDistance);

        private IEnumerator HideOpponentPokeBallLine()
            => HidePokeBallLine(opponentPokeBallLineController,
                opponentPokeBallLineRootX - opponentPokeBallLineOffScreenLocalXDistance);

        #endregion

        /// <param name="mainSprite">The sprite to start and end on</param>
        /// <param name="secondarySprite">The sprite to change to</param>
        private IEnumerator AnimateOpponentPokemonEntrance(SpriteRenderer spriteObject, Sprite mainSprite, Sprite secondarySprite)
        {

            spriteObject.sprite = mainSprite;

            yield return new WaitForSeconds(opponentPokemonEntranceAnimationSpriteChangeDelay);

            spriteObject.sprite = secondarySprite;

            yield return new WaitForSeconds(opponentPokemonEntranceAnimationSpriteChangeDelay);

            spriteObject.sprite = mainSprite;


        }

        #region Wild Pokemon Sending Out

        private IEnumerator SendInWildPokemon(Sprite pokemonSprite, GameObject spriteObject, float rootXPos, float offScreenXPos)
        {

            spriteObject.GetComponent<SpriteRenderer>().sprite = pokemonSprite;
            spriteObject.transform.localPosition = new Vector3(
                offScreenXPos,
                spriteObject.transform.localPosition.y,
                spriteObject.transform.localPosition.z
                );
            spriteObject.SetActive(true);

            Vector3 targetPosition = spriteObject.transform.localPosition;
            targetPosition.x = rootXPos;

            yield return StartCoroutine(GradualTranslateLocalPosition(spriteObject, targetPosition, sendInPokemonTime));

        }

        public IEnumerator SendInWildOpponentPokemon(PokemonInstance pokemon)
        {
            overviewPaneManager.opponentPokemonOverviewPaneController.FullUpdate(pokemon);
            yield return StartCoroutine(SendInWildPokemon(pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1), opponentPokemonSprite, opponentPokemonSpriteRootX, pokemonSpriteOffScreenRightLocalPositionX));
            SoundFXController.singleton.PlayPokemonCry(pokemon.speciesId);
            yield return StartCoroutine(AnimateOpponentPokemonEntrance(
                opponentPokemonSprite.GetComponent<SpriteRenderer>(),
                pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1),
                pokemon.LoadSprite(PokemonSpecies.SpriteType.Front2)
                ));
            yield return StartCoroutine(overviewPaneManager.RevealOpponentOverviewPane());
        }

        #endregion

        #region Trainer Pokemon Sending Out

        private IEnumerator AnimatePokeBallOpening(GameObject pokeBallObject,
            Sprite pokeBallSpriteNeutral,
            Sprite pokeBallSpriteSquashed,
            Sprite pokeBallSpriteOpen)
        {

            pokeBallObject.SetActive(true);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBallSpriteNeutral;

            yield return new WaitForSeconds(pokeBallOpeningNeutralTime);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBallSpriteSquashed;

            yield return new WaitForSeconds(pokeBallOpeningSquashedTime);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBallSpriteOpen;

        }

        private IEnumerator AnimatePokemonEmerge(GameObject spriteObject,
            float initialSize,
            float targetSize,
            float timeToTake,
            int speciesId)
        {

            spriteObject.SetActive(true);

            SoundFXController.singleton.PlayPokemonCry(speciesId);

            spriteObject.transform.localScale = Vector2.one * initialSize;
            yield return StartCoroutine(GradualChangeScale(spriteObject,
                Vector2.one * targetSize,
                timeToTake));

            //TODO - later on, have sprite turn from pure white to normal color whilst growing

        }

        public IEnumerator SendInPlayerPokemon(PokemonInstance pokemon,
            PokeBallLineController.BallState[] participantPokemonStates)
        {

            overviewPaneManager.playerPokemonOverviewPaneController.FullUpdate(pokemon);

            PokeBall pokeball = PokeBall.GetPokeBallById(pokemon.pokeBallId);
            Sprite pokeBallNeutral = pokeball.GetSprite(PokeBall.SpriteType.Neutral);
            Sprite pokeBallSquashed = pokeball.GetSprite(PokeBall.SpriteType.Squashed);
            Sprite pokeBallOpen = pokeball.GetSprite(PokeBall.SpriteType.Open);

            yield return StartCoroutine(DisplayPlayerPokeBallLine(participantPokemonStates));

            yield return StartCoroutine(AnimatePokeBallOpening(playerPokeBallSprite,
                pokeBallNeutral,
                pokeBallSquashed,
                pokeBallOpen));

            playerPokemonSprite.GetComponent<SpriteRenderer>().sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Back);
            yield return StartCoroutine(AnimatePokemonEmerge(playerPokemonSprite,
                0,
                playerPokemonSprite.transform.localScale.x,
                pokemonEmergeTime,
                pokemon.speciesId));

            playerPokeBallSprite.SetActive(false);

            yield return StartCoroutine(HidePlayerPokeBallLine());

            yield return StartCoroutine(overviewPaneManager.RevealPlayerOverviewPane());

        }

        public IEnumerator SendInTrainerOpponentPokemon(PokemonInstance pokemon,
            PokeBallLineController.BallState[] participantPokemonStates)
        {

            overviewPaneManager.opponentPokemonOverviewPaneController.FullUpdate(pokemon);

            PokeBall pokeball = PokeBall.GetPokeBallById(pokemon.pokeBallId);
            Sprite pokeBallNeutral = pokeball.GetSprite(PokeBall.SpriteType.Neutral);
            Sprite pokeBallSquashed = pokeball.GetSprite(PokeBall.SpriteType.Squashed);
            Sprite pokeBallOpen = pokeball.GetSprite(PokeBall.SpriteType.Open);

            yield return StartCoroutine(DisplayOpponentPokeBallLine(participantPokemonStates));

            yield return StartCoroutine(AnimatePokeBallOpening(opponentPokeBallSprite,
                pokeBallNeutral,
                pokeBallSquashed,
                pokeBallOpen));

            opponentPokemonSprite.GetComponent<SpriteRenderer>().sprite = pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1);
            yield return StartCoroutine(AnimatePokemonEmerge(opponentPokemonSprite,
                0,
                opponentPokemonSprite.transform.localScale.x,
                pokemonEmergeTime,
                pokemon.speciesId));

            opponentPokeBallSprite.SetActive(false);

            yield return StartCoroutine(HideOpponentPokeBallLine());

            yield return StartCoroutine(AnimateOpponentPokemonEntrance(
                opponentPokemonSprite.GetComponent<SpriteRenderer>(),
                pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1),
                pokemon.LoadSprite(PokemonSpecies.SpriteType.Front2)
                ));

            yield return StartCoroutine(overviewPaneManager.RevealOpponentOverviewPane());

        }

        #endregion

        #endregion

        #region Pokemon Damage Taking and Healing

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

        public IEnumerator HealHealthPlayerPokmeon(int newHealth,
            int startHealth,
            int maxHealth)
        {
            yield return StartCoroutine(GradualHealthBarChange(
                (int value) => overviewPaneManager.playerPokemonOverviewPaneController.UpdateHealthBar(value, maxHealth),
                startHealth,
                newHealth,
                healthBarChangeRate * Mathf.Abs(newHealth - startHealth)
            ));
        }

        public IEnumerator HealHealthOpponentPokmeon(int newHealth,
            int startHealth,
            int maxHealth)
        {
            yield return StartCoroutine(GradualHealthBarChange(
                (int value) => overviewPaneManager.opponentPokemonOverviewPaneController.UpdateHealthBar(value, maxHealth),
                startHealth,
                newHealth,
                healthBarChangeRate * Mathf.Abs(newHealth - startHealth)
            ));
        }

        #endregion

        #region Player Pokemon Experience Increasing

        private IEnumerator GradualExperienceBarChange(Action<float> updateBarAction,
            int startExperience,
            int endExperience,
            float timeToTake,
            GrowthType growthType)
        {

            float startTime = Time.time;
            float endTime = Time.time + timeToTake;

            while (true)
            {

                if (Time.time >= endTime)
                    break;

                float timeFactor = (Time.time - startTime) / timeToTake;

                int experienceToShow = Mathf.RoundToInt(Mathf.Lerp(
                    startExperience,
                    endExperience,
                    timeFactor));

                float newExperienceBarAmont = CalculateExperienceBarAmount(experienceToShow, growthType);
                updateBarAction(newExperienceBarAmont);

                yield return new WaitForFixedUpdate();

            }

            updateBarAction(CalculateExperienceBarAmount(endExperience, growthType));

        }

        private float CalculateExperienceBarAmount(int pokemonExperience, GrowthType growthType)
        {

            byte lowerLevelBound = GrowthTypeData.GetLevelFromExperience(pokemonExperience, growthType);
            int lowerLevelExperienceBound = GrowthTypeData.GetMinimumExperienceForLevel(lowerLevelBound, growthType);
            int upperLevelExperienceBound = GrowthTypeData.GetMinimumExperienceForLevel((byte)(lowerLevelBound + 1), growthType);

            //This means that if the pokemon has gotten enough experience to level up, the bar doesn't overflow but loops round instead
            return Mathf.InverseLerp(
                lowerLevelExperienceBound,
                upperLevelExperienceBound,
                pokemonExperience
                );

        }

        public IEnumerator IncreasePlayerPokemonExperience(int initialExperience,
            int newExperience,
            GrowthType growthType)
        {

            yield return StartCoroutine(GradualExperienceBarChange(
                (float value) => overviewPaneManager.playerPokemonOverviewPaneController.UpdateExperienceBar(value),
                initialExperience,
                newExperience,
                experienceBarChangeTime,
                growthType
            ));

            yield return new WaitForSeconds(experienceBarChangeEndWaitTime);

        }

        #endregion

        #region Generic Pokemon Move Using

        private Vector2 GetGenericPokemonMoveLungeDirection(Transform lunger, Transform target)
            => ((Vector2)(target.position - lunger.position)).normalized;

        private IEnumerator GenericPokemonMovePhysicalAttackMovement(GameObject attacker, GameObject target, bool moveNoOpponentEffects, Sprite moveParticle, GameObject targetParticleSystemObject)
        {

            Vector2 attackerStartPosition = attacker.transform.localPosition;
            Vector2 targetStartPosition = target.transform.localPosition;

            Vector2 lungeDirection = GetGenericPokemonMoveLungeDirection(attacker.transform, target.transform);

            Vector2 leanBackTargetPosition = (Vector2)attacker.transform.localPosition + (lungeDirection * -genericPhysicalMoveLeanBackDistance);

            yield return StartCoroutine(GradualTranslateLocalPosition(attacker, leanBackTargetPosition, genericPhysicalMoveLeanBackTime));

            yield return new WaitForSeconds(genericPhysicalMovePauseTime);

            Vector2 lungeTargetPosition = attackerStartPosition + (lungeDirection * genericMoveLungeDistance);

            yield return StartCoroutine(GradualTranslateLocalPosition(attacker, lungeTargetPosition, genericPhysicalMoveLungeTime));

            if (!moveNoOpponentEffects)
                yield return StartCoroutine(GenericMoveEffects(targetParticleSystemObject, moveParticle, PokemonMove.MoveType.Physical));

            Vector2 targetJerkBackTargetPosition = targetStartPosition + (lungeDirection * genericPhysicalMoveTargetJerkBackDistance);

            yield return StartCoroutine(GradualTranslateLocalPosition(target, targetJerkBackTargetPosition, genericPhysicalMoveTargetJerkBackTime));

            yield return StartCoroutine(GradualTranslateLocalPosition(attacker, attackerStartPosition, genericMoveReturnBackTime));

            yield return StartCoroutine(GradualTranslateLocalPosition(target, targetStartPosition, genericPhysicalMoveTargetReturnTime));

        }

        private IEnumerator GenericPokemonMoveSpecialAttackMovement(GameObject attacker, GameObject target, bool moveNoOpponentEffects, Sprite moveParticle, GameObject targetParticleSystemObject)
        {

            Vector2 attackerStartPosition = attacker.transform.localPosition;

            Vector2 lungeDirection = GetGenericPokemonMoveLungeDirection(attacker.transform, target.transform);

            Vector2 lungeTargetPosition = (Vector2)attacker.transform.localPosition + (lungeDirection * genericMoveLungeDistance);

            yield return StartCoroutine(GradualTranslateLocalPosition(attacker, lungeTargetPosition, genericSpecialMoveLungeTime));

            if (!moveNoOpponentEffects)
                yield return StartCoroutine(GenericMoveEffects(targetParticleSystemObject, moveParticle, PokemonMove.MoveType.Special));

            yield return StartCoroutine(GradualTranslateLocalPosition(attacker, attackerStartPosition, genericMoveReturnBackTime));

        }

        private IEnumerator GenericMoveEffects(GameObject particleSystemObject,
            Sprite particleSprite,
            PokemonMove.MoveType moveType)
        {

            #region Particles

            if (particleSystemObject.GetComponent<ParticleSystem>() == null)
                throw new ArgumentException("particleSystemObject has no particle system");

            if (particleSprite == null)
                yield break;

            particleSystemObject.GetComponent<ParticleSystem>().textureSheetAnimation.SetSprite(0, particleSprite);
            particleSystemObject.GetComponent<ParticleSystem>().Play();

            #endregion

            #region Sound FX

            string soundFXName = moveType switch
            {
                PokemonMove.MoveType.Physical => physicalMoveSoundFXName,
                PokemonMove.MoveType.Special => specialMoveSoundFXName,
                PokemonMove.MoveType.Status => statusMoveSoundFXName,
                _ => ""
            };

            SoundFXController.singleton.PlaySound(soundFXName);

            #endregion

        }

        public IEnumerator PlayerUseMoveGeneric(int moveId)
        {

            PokemonMove move = PokemonMove.GetPokemonMoveById(moveId);
            Sprite moveParticle = TypeFunc.LoadTypeParticleSprite(move.type);

            if (move.moveType == PokemonMove.MoveType.Physical)
            {
                yield return StartCoroutine(GenericPokemonMovePhysicalAttackMovement(playerPokemonSprite, opponentPokemonSprite, move.noOpponentEffects, moveParticle, opponentPokemonMoveParticleSystemObject));
            }
            else if (move.moveType == PokemonMove.MoveType.Special)
            {
                yield return StartCoroutine(GenericPokemonMoveSpecialAttackMovement(playerPokemonSprite, opponentPokemonSprite, move.noOpponentEffects, moveParticle, opponentPokemonMoveParticleSystemObject));
            }
            else if (move.moveType == PokemonMove.MoveType.Status)
            {
                if (!move.noOpponentEffects)
                {
                    yield return StartCoroutine(GenericMoveEffects(opponentPokemonMoveParticleSystemObject, moveParticle, PokemonMove.MoveType.Status));
                    yield return new WaitUntil(() => !opponentPokemonMoveParticleSystemObject.GetComponent<ParticleSystem>().isPlaying);
                }
            }

        }

        public IEnumerator OpponentUseMoveGeneric(int moveId)
        {

            PokemonMove move = PokemonMove.GetPokemonMoveById(moveId);
            Sprite moveParticle = TypeFunc.LoadTypeParticleSprite(move.type);

            if (move.moveType == PokemonMove.MoveType.Physical)
            {
                yield return StartCoroutine(GenericPokemonMovePhysicalAttackMovement(opponentPokemonSprite, playerPokemonSprite, move.noOpponentEffects, moveParticle, playerPokemonMoveParticleSystemObject));
            }
            else if (move.moveType == PokemonMove.MoveType.Special)
            {
                yield return StartCoroutine(GenericPokemonMoveSpecialAttackMovement(opponentPokemonSprite, playerPokemonSprite, move.noOpponentEffects, moveParticle, playerPokemonMoveParticleSystemObject));
            }
            else if (move.moveType == PokemonMove.MoveType.Status)
            {
                if (!move.noOpponentEffects)
                {
                    yield return StartCoroutine(GenericMoveEffects(playerPokemonMoveParticleSystemObject, moveParticle, PokemonMove.MoveType.Status));
                    yield return new WaitUntil(() => !opponentPokemonMoveParticleSystemObject.GetComponent<ParticleSystem>().isPlaying);
                }
            }

        }

        #endregion

        #region Stat Stage Change

        private IEnumerator HalfPokemonStatChange(GameObject gameObject,
            Vector3 targetScale,
            Color targetColor,
            float timeToTake,
            float refreshTime = 0)
        {

            if (gameObject.GetComponent<SpriteRenderer>() == null)
                Debug.LogError("gameObject provided has no SpriteRenderer component");

            Vector3 startScale = gameObject.transform.localScale;
            Color startColor = gameObject.GetComponent<SpriteRenderer>().color;

            yield return StartCoroutine(GradualEffect(
                (t) =>
                    {
                        gameObject.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                        gameObject.GetComponent<SpriteRenderer>().color = Color.Lerp(startColor, targetColor, t);
                    },
                timeToTake,
                refreshTime));

            gameObject.transform.localScale = targetScale;

        }

        private IEnumerator PokemonStatChange(GameObject pokemonObject,
            bool statIncrease)
        {

            const float halfMovementTime = statStageChangeTotalTime / 2;

            if (pokemonObject.GetComponent<SpriteRenderer>() == null)
                Debug.LogError("pokemonObject provided has no SpriteRenderer component");

            Vector3 originalScale = pokemonObject.transform.localScale;
            Vector3 scaledScale = statIncrease
                ? originalScale * statStageChangeScaleFactor
                : originalScale / statStageChangeScaleFactor;

            Color originalColor = pokemonObject.GetComponent<SpriteRenderer>().color;
            Color changedColor = statIncrease ? statStageChangeIncreaseColor : statStageChangeDecreaseColor;

            #region Sound FX

            string soundFXName = statIncrease ? statChangeUpSoundFXName : statChangeDownSoundFXName;

            SoundFXController.singleton.PlaySound(soundFXName);

            #endregion

            yield return StartCoroutine(HalfPokemonStatChange(pokemonObject,
                scaledScale,
                changedColor,
                halfMovementTime
            ));

            yield return StartCoroutine(HalfPokemonStatChange(pokemonObject,
                originalScale,
                originalColor,
                halfMovementTime
            ));

        }

        public IEnumerator PlayerStatStageChange(bool statIncrease)
        {

            yield return StartCoroutine(PokemonStatChange(playerPokemonSprite, statIncrease));

        }

        public IEnumerator OpponentStatStageChange(bool statIncrease)
        {

            yield return StartCoroutine(PokemonStatChange(opponentPokemonSprite, statIncrease));

        }

        #endregion

        #region Opponent Trainer Showcase

        public IEnumerator OpponentTrainerShowcaseStart(Sprite trainerSprite)
        {

            opponentTrainerSprite.SetActive(true);

            opponentTrainerSprite.GetComponent<SpriteRenderer>().sprite = trainerSprite;
            opponentTrainerSprite.transform.localPosition = new Vector3(
                pokemonSpriteOffScreenRightLocalPositionX,
                opponentTrainerSprite.transform.localPosition.y,
                opponentTrainerSprite.transform.localPosition.z
            );

            Vector3 targetPosition = new Vector3(
                opponentTrainerSpriteRootX,
                opponentTrainerSprite.transform.localPosition.y,
                opponentTrainerSprite.transform.localPosition.z
            );

            yield return StartCoroutine(GradualTranslateLocalPosition(opponentTrainerSprite, targetPosition, opponentTrainerShowcaseMovementTime));

        }

        public IEnumerator OpponentTrainerShowcaseStop()
        {

            opponentTrainerSprite.SetActive(true);

            Vector3 targetPosition = new Vector3(
                pokemonSpriteOffScreenRightLocalPositionX,
                opponentTrainerSprite.transform.localPosition.y,
                opponentTrainerSprite.transform.localPosition.z
            );

            yield return StartCoroutine(GradualTranslateLocalPosition(opponentTrainerSprite, targetPosition, opponentTrainerShowcaseMovementTime));

            opponentTrainerSprite.SetActive(false);

        }

        #endregion

        #region Poke Ball Throw

        private IEnumerator PokemonIntoPokeBallAnimation(GameObject pokemonObject,
            Vector3 pokeBallPosition)
        {

            Vector3 startPosition = pokemonObject.transform.position;
            Vector3 startScale = pokemonObject.transform.localScale;
            float startHeight = pokemonObject.GetComponent<SpriteRenderer>().sprite.rect.height;

            yield return StartCoroutine(GradualEffect(
                (t) =>
                {
                    pokemonObject.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                    pokemonObject.transform.position = Vector3.Lerp(startPosition, pokeBallPosition, t);
                },
                pokeBallUsePokemonEnterPokeBallTime));

            pokemonObject.transform.localScale = startScale;
            pokemonObject.transform.position = startPosition;

            pokemonObject.SetActive(false);

        }

        /// <summary>
        /// Animation for the poke ball being thrown and the pokemon going into the ball
        /// </summary>
        /// <param name="pokeBallObject">The game object for the poke ball</param>
        /// <param name="pokeBall">The poke ball to use. This is used to choose sprites</param>
        /// <param name="startPos">The position to start the poke ball at (this would probably be off-screen)</param>
        /// <param name="throwEndPos">The position to end the throw at. Here, the poke ball stops and opens for the target pokemon to go into</param>
        /// <param name="wobblingStartPos">The position for the poke ball to drop to after the pokemon goes into it</param>
        /// <param name="targetPokemonObject">The target pokemon's game object</param>
        private IEnumerator PokeBallThrow(GameObject pokeBallObject,
            PokeBall pokeBall,
            Vector3 startPos,
            Vector3 throwEndPos,
            Vector3 wobblingStartPos,
            GameObject targetPokemonObject)
        {

            if (pokeBallObject.GetComponent<SpriteRenderer>() == null)
            {
                Debug.LogError("No SpriteRenderer component on pokeBallObject");
            }

            if (targetPokemonObject.GetComponent<SpriteRenderer>() == null)
            {
                Debug.LogError("No SpriteRenderer component on targetPokemonObject");
            }

            pokeBallObject.transform.position = startPos;

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.Center);

            yield return StartCoroutine(GradualEffect(
                (t) => pokeBallObject.transform.position = Vector3.Lerp(startPos, throwEndPos, t) + Vector3.up * pokeBallThrowHeightOffsetCurve.Evaluate(t),
                pokeBallUseThrowTime));

            pokeBallObject.transform.position = throwEndPos;

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.Center);

            yield return new WaitForSeconds(pokeBallUseThrowToOpenDelay);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.Open);

            yield return StartCoroutine(PokemonIntoPokeBallAnimation(targetPokemonObject, pokeBallObject.transform.position));

            yield return new WaitForSeconds(pokeBallUseOpenClampInterval);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.Neutral);

            yield return new WaitForSeconds(pokeBallUseOpenClampInterval);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.Squashed);

            yield return new WaitForSeconds(pokeBallUseOpenClampInterval);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.Neutral);

            yield return new WaitForSeconds(pokeBallUseOpenClampInterval);

            yield return StartCoroutine(GradualTranslateGlobalPosition(pokeBallObject, wobblingStartPos, pokeBallUsePokeBallDropTime));

        }

        private IEnumerator PokeBallWobble(GameObject pokeBallObject,
            PokeBall pokeBall)
        {

            if (pokeBallObject.GetComponent<SpriteRenderer>() == null)
            {
                Debug.LogError("No Sprite Renderer component in pokeBallObject");
                yield break;
            }

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.WobbleCenter);
            pokeBallObject.transform.rotation = Quaternion.Euler(Vector3.zero);

            yield return new WaitForSeconds(pokeBallUseWobbleChangeInterval);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.WobbleLeft);
            pokeBallObject.transform.Rotate(new Vector3(0, 0, pokeBallWobbleRotationAngle));

            yield return new WaitForSeconds(pokeBallUseWobbleChangeInterval);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.WobbleCenter);
            pokeBallObject.transform.Rotate(new Vector3(0, 0, -pokeBallWobbleRotationAngle));

            yield return new WaitForSeconds(pokeBallUseWobbleChangeInterval);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.WobbleRight);
            pokeBallObject.transform.Rotate(new Vector3(0, 0, -pokeBallWobbleRotationAngle));

            yield return new WaitForSeconds(pokeBallUseWobbleChangeInterval);

            pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.WobbleCenter);
            pokeBallObject.transform.Rotate(new Vector3(0, 0, pokeBallWobbleRotationAngle));

        }

        private IEnumerator PokeBallWobbling(GameObject pokeBallObject,
            byte wobbleCount,
            PokeBall pokeBall,
            GameObject targetPokemonObject,
            int speciesId)
        {

            byte wobbles = wobbleCount < PokeBall.shakeTrialsRequired ? wobbleCount : (byte)(PokeBall.shakeTrialsRequired - 1);

            yield return new WaitForSeconds(pokeBallUseWobbleDelay);

            for (byte i = 0; i < wobbles; i++)
            {

                yield return StartCoroutine(PokeBallWobble(pokeBallObject, pokeBall));

                //More time is waited each time to increase the suspense
                yield return new WaitForSeconds(pokeBallUseWobbleDelay + (pokeBallUseWobbleDelayIncrease * (i + 1)));

            }

            bool caught = wobbleCount >= PokeBall.shakeTrialsRequired;

            if (caught)
            {

                pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.Caught);

            }
            else
            {

                pokeBallObject.GetComponent<SpriteRenderer>().sprite = pokeBall.GetSprite(PokeBall.SpriteType.Open);

                //Same as when trainer pokemon emerges
                yield return StartCoroutine(AnimatePokemonEmerge(targetPokemonObject, 0, targetPokemonObject.transform.localScale.x, pokemonEmergeTime, speciesId));

            }

            yield return new WaitForSeconds(pokeBallUseWobbleEndPauseTime);

        }

        /// <summary>
        /// Animation for the player throwing a poke ball and the poke ball wobbling, opening or staying caught
        /// </summary>
        /// <param name="wobbleCount">Below PokeBall.shakeTrialsRequired, this is the number of times the poke ball should wobble. If it is greater than PokeBall.shakeTrialsRequired, it means the pokemon should be caught</param>
        public IEnumerator PokeBallUse(PokeBall pokeBall, byte wobbleCount, int speciesId)
        {

            pokeBallThrowGameObject.SetActive(true);

            yield return StartCoroutine(PokeBallThrow(pokeBallThrowGameObject,
                pokeBall,
                pokeBallThrowStartPosition.position,
                pokeBallThrowEndPosition.position,
                pokeBallThrowDroppedPosition.position,
                opponentPokemonSprite));

            yield return StartCoroutine(PokeBallWobbling(pokeBallThrowGameObject,
                wobbleCount,
                pokeBall,
                opponentPokemonSprite,
                speciesId));

            if (wobbleCount < PokeBall.shakeTrialsRequired)
            {
                pokeBallThrowGameObject.SetActive(false);
                opponentPokemonSprite.SetActive(true);
            }

        }

        #endregion

    }
}
