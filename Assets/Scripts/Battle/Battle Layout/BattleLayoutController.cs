using System;
using System.Collections;
using UnityEngine;
using Pokemon;
using Items.PokeBalls;

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

        public GameObject playerPokemonMoveParticleSystemObject;
        public GameObject opponentPokemonMoveParticleSystemObject;

        public GameObject opponentTrainerSprite;
        private float opponentTrainerSpriteRootX;

        public GameObject playerPokeBallSprite;
        public GameObject opponentPokeBallSprite;

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
            overviewPaneManager.HidePanes();
        }

        /// <summary>
        /// Gradually move a game object from its current position to another position
        /// </summary>
        /// <param name="gameObject">The game object to move</param>
        /// <param name="endPos">The local position to move to</param>
        /// <param name="timeToTake">The time the move should take</param>
        /// <param name="refreshTime">The time to wait before refreshing the object's position. 0 means to refresh each frame</param>
        /// <returns></returns>
        private IEnumerator GradualTranslatePosition(GameObject gameObject,
            Vector3 endPos,
            float timeToTake,
            float refreshTime = 0)
        {

            Vector3 startPos = gameObject.transform.localPosition;

            float startTime = Time.time;
            float endTime = startTime + timeToTake;

            while (true)
            {

                if (Time.time >= endTime)
                    break;

                float timeFactor = (Time.time - startTime) / timeToTake;

                gameObject.transform.localPosition = Vector3.Lerp(startPos, endPos, timeFactor);

                if (refreshTime == 0)
                    yield return new WaitForFixedUpdate();
                else
                    yield return new WaitForSeconds(refreshTime);

            }

            gameObject.transform.localPosition = endPos;

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

            float startTime = Time.time;
            float endTime = startTime + timeToTake;

            while (true)
            {

                if (Time.time >= endTime)
                    break;

                float timeFactor = (Time.time - startTime) / timeToTake;

                gameObject.transform.localScale = Vector3.Lerp(startScale, endScale, timeFactor);

                if (refreshTime == 0)
                    yield return new WaitForFixedUpdate();
                else
                    yield return new WaitForSeconds(refreshTime);

            }

            gameObject.transform.localScale = endScale;

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

            yield return StartCoroutine(GradualTranslatePosition(spriteObject, targetPosition, sendInPokemonTime));

        }

        public IEnumerator SendInWildOpponentPokemon(PokemonInstance pokemon)
        {
            overviewPaneManager.opponentPokemonOverviewPaneController.FullUpdate(pokemon);
            yield return StartCoroutine(SendInWildPokemon(pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1), opponentPokemonSprite, opponentPokemonSpriteRootX, pokemonSpriteOffScreenRightLocalPositionX));
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
            Sprite sprite,
            float initialSize,
            float targetSize,
            float timeToTake)
        {

            spriteObject.GetComponent<SpriteRenderer>().sprite = sprite;
            spriteObject.SetActive(true);

            spriteObject.transform.localScale = Vector2.one * initialSize;
            yield return StartCoroutine(GradualChangeScale(spriteObject,
                Vector2.one * targetSize,
                timeToTake));

            //TODO - later on, have sprite turn from pure white to normal color whilst growing. To do this, can't use GradualChangeScale, as must have color change simultaneous

        }

        public IEnumerator SendInPlayerPokemon(PokemonInstance pokemon)
        {
            overviewPaneManager.playerPokemonOverviewPaneController.FullUpdate(pokemon);

            PokeBall pokeball = PokeBall.GetPokeBallById(pokemon.pokeBallId);
            Sprite pokeBallNeutral = pokeball.GetSprite(PokeBall.SpriteType.Neutral);
            Sprite pokeBallSquashed = pokeball.GetSprite(PokeBall.SpriteType.Squashed);
            Sprite pokeBallOpen = pokeball.GetSprite(PokeBall.SpriteType.Open);

            yield return StartCoroutine(AnimatePokeBallOpening(playerPokeBallSprite,
                pokeBallNeutral,
                pokeBallSquashed,
                pokeBallOpen));

            yield return StartCoroutine(AnimatePokemonEmerge(playerPokemonSprite,
                pokemon.LoadSprite(PokemonSpecies.SpriteType.Back),
                0,
                playerPokemonSprite.transform.localScale.x,
                pokemonEmergeTime));

            playerPokeBallSprite.SetActive(false);

            yield return StartCoroutine(overviewPaneManager.RevealPlayerOverviewPane());
        }

        public IEnumerator SendInTrainerOpponentPokemon(PokemonInstance pokemon)
        {
            overviewPaneManager.opponentPokemonOverviewPaneController.FullUpdate(pokemon);

            PokeBall pokeball = PokeBall.GetPokeBallById(pokemon.pokeBallId);
            Sprite pokeBallNeutral = pokeball.GetSprite(PokeBall.SpriteType.Neutral);
            Sprite pokeBallSquashed = pokeball.GetSprite(PokeBall.SpriteType.Squashed);
            Sprite pokeBallOpen = pokeball.GetSprite(PokeBall.SpriteType.Open);

            yield return StartCoroutine(AnimatePokeBallOpening(opponentPokeBallSprite,
                pokeBallNeutral,
                pokeBallSquashed,
                pokeBallOpen));

            yield return StartCoroutine(AnimatePokemonEmerge(opponentPokemonSprite,
                pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1),
                0,
                opponentPokemonSprite.transform.localScale.x,
                pokemonEmergeTime));

            opponentPokeBallSprite.SetActive(false);

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

            yield return StartCoroutine(GradualTranslatePosition(attacker, leanBackTargetPosition, genericPhysicalMoveLeanBackTime));

            yield return new WaitForSeconds(genericPhysicalMovePauseTime);

            Vector2 lungeTargetPosition = attackerStartPosition + (lungeDirection * genericMoveLungeDistance);

            yield return StartCoroutine(GradualTranslatePosition(attacker, lungeTargetPosition, genericPhysicalMoveLungeTime));

            if (!moveNoOpponentEffects)
                yield return StartCoroutine(GenericMoveParticleEffects(targetParticleSystemObject, moveParticle));

            Vector2 targetJerkBackTargetPosition = targetStartPosition + (lungeDirection * genericPhysicalMoveTargetJerkBackDistance);

            yield return StartCoroutine(GradualTranslatePosition(target, targetJerkBackTargetPosition, genericPhysicalMoveTargetJerkBackTime));

            yield return StartCoroutine(GradualTranslatePosition(attacker, attackerStartPosition, genericMoveReturnBackTime));

            yield return StartCoroutine(GradualTranslatePosition(target, targetStartPosition, genericPhysicalMoveTargetReturnTime));

        }

        private IEnumerator GenericPokemonMoveSpecialAttackMovement(GameObject attacker, GameObject target, bool moveNoOpponentEffects, Sprite moveParticle, GameObject targetParticleSystemObject)
        {

            Vector2 attackerStartPosition = attacker.transform.localPosition;

            Vector2 lungeDirection = GetGenericPokemonMoveLungeDirection(attacker.transform, target.transform);

            Vector2 lungeTargetPosition = (Vector2)attacker.transform.localPosition + (lungeDirection * genericMoveLungeDistance);

            yield return StartCoroutine(GradualTranslatePosition(attacker, lungeTargetPosition, genericSpecialMoveLungeTime));

            if (!moveNoOpponentEffects)
                yield return StartCoroutine(GenericMoveParticleEffects(targetParticleSystemObject, moveParticle));

            yield return StartCoroutine(GradualTranslatePosition(attacker, attackerStartPosition, genericMoveReturnBackTime));

        }

        private IEnumerator GenericMoveParticleEffects(GameObject particleSystemObject,
            Sprite particleSprite)
        {

            if (particleSystemObject.GetComponent<ParticleSystem>() == null)
                throw new ArgumentException("particleSystemObject has no particle system");

            if (particleSprite == null)
                yield break;

            particleSystemObject.GetComponent<ParticleSystem>().textureSheetAnimation.SetSprite(0, particleSprite);
            particleSystemObject.GetComponent<ParticleSystem>().Play();

        }

        public IEnumerator PlayerUseMoveGeneric(int moveId)
        {

            Pokemon.Moves.PokemonMove move = Pokemon.Moves.PokemonMove.GetPokemonMoveById(moveId);
            Sprite moveParticle = TypeFunc.LoadTypeParticleSprite(move.type);

            if (move.moveType == Pokemon.Moves.PokemonMove.MoveType.Physical)
            {
                yield return StartCoroutine(GenericPokemonMovePhysicalAttackMovement(playerPokemonSprite, opponentPokemonSprite, move.noOpponentEffects, moveParticle, opponentPokemonMoveParticleSystemObject));
            }
            else if (move.moveType == Pokemon.Moves.PokemonMove.MoveType.Special)
            {
                yield return StartCoroutine(GenericPokemonMoveSpecialAttackMovement(playerPokemonSprite, opponentPokemonSprite, move.noOpponentEffects, moveParticle, opponentPokemonMoveParticleSystemObject));
            }
            else if (move.moveType == Pokemon.Moves.PokemonMove.MoveType.Status)
            {
                if (!move.noOpponentEffects)
                {
                    yield return StartCoroutine(GenericMoveParticleEffects(opponentPokemonMoveParticleSystemObject, moveParticle));
                    yield return new WaitUntil(() => !opponentPokemonMoveParticleSystemObject.GetComponent<ParticleSystem>().isPlaying);
                }
            }

        }

        public IEnumerator OpponentUseMoveGeneric(int moveId)
        {

            Pokemon.Moves.PokemonMove move = Pokemon.Moves.PokemonMove.GetPokemonMoveById(moveId);
            Sprite moveParticle = TypeFunc.LoadTypeParticleSprite(move.type);

            if (move.moveType == Pokemon.Moves.PokemonMove.MoveType.Physical)
            {
                yield return StartCoroutine(GenericPokemonMovePhysicalAttackMovement(opponentPokemonSprite, playerPokemonSprite, move.noOpponentEffects, moveParticle, playerPokemonMoveParticleSystemObject));
            }
            else if (move.moveType == Pokemon.Moves.PokemonMove.MoveType.Special)
            {
                yield return StartCoroutine(GenericPokemonMoveSpecialAttackMovement(opponentPokemonSprite, playerPokemonSprite, move.noOpponentEffects, moveParticle, playerPokemonMoveParticleSystemObject));
            }
            else if (move.moveType == Pokemon.Moves.PokemonMove.MoveType.Status)
            {
                if (!move.noOpponentEffects)
                {
                    yield return StartCoroutine(GenericMoveParticleEffects(playerPokemonMoveParticleSystemObject, moveParticle));
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

            float startTime = Time.time;
            float endTime = startTime + timeToTake;

            while (true)
            {

                if (Time.time >= endTime)
                    break;

                float timeFactor = (Time.time - startTime) / timeToTake;

                gameObject.transform.localScale = Vector3.Lerp(startScale, targetScale, timeFactor);
                gameObject.GetComponent<SpriteRenderer>().color = Color.Lerp(startColor, targetColor, timeFactor);

                if (refreshTime == 0)
                    yield return new WaitForFixedUpdate();
                else
                    yield return new WaitForSeconds(refreshTime);

            }

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

            yield return StartCoroutine(GradualTranslatePosition(opponentTrainerSprite, targetPosition, opponentTrainerShowcaseMovementTime));

        }

        public IEnumerator OpponentTrainerShowcaseStop()
        {

            opponentTrainerSprite.SetActive(true);

            Vector3 targetPosition = new Vector3(
                pokemonSpriteOffScreenRightLocalPositionX,
                opponentTrainerSprite.transform.localPosition.y,
                opponentTrainerSprite.transform.localPosition.z
            );

            yield return StartCoroutine(GradualTranslatePosition(opponentTrainerSprite, targetPosition, opponentTrainerShowcaseMovementTime));

            opponentTrainerSprite.SetActive(false);

        }

        #endregion

        //TODO - have methods for each type of animation that this may need to deal with

    }
}
