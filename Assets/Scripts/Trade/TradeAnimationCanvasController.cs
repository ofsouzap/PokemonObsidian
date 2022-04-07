using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items.PokeBalls;
using Audio;

namespace Trade
{
    [RequireComponent(typeof(Canvas))]
    public class TradeAnimationCanvasController : MonoBehaviour
    {

        public Image pokemonImage;
        private Transform PokemonTransform => pokemonImage.transform;

        public Image pokeBallImage;
        private Transform PokeBallTransform => pokeBallImage.transform;

        public Transform mainRootTransform;
        public Transform pokeBallOffScreenTransform;

        public AnimationCurve pokeBallAddedDisplacementCurve_x;
        public AnimationCurve pokeBallFactorDisplacementCurve_y;

        private TextBoxController textBoxController;

        private SoundFXController SoundFXController => SoundFXController.singleton;

        #region Constants

        const float bounceHeight = 75;
        const float bounceSingleTime = 0.5F;

        const float shrinkGrowTime = 1.5F;

        const float jumpScaleDelay = 1F;

        const float addedDisplacementFactor = 20;

        const float pokeBallSendReturnTime = 2F;

        const byte sendPmonBonuceCount = 2;

        const string goodbyeMessagePrefix = "Goodbye, ";
        const string goodbyeMessageSuffix = "!";

        const float pokeBallCloseSendDelay = 1F;

        const float pokeBallReturnDelay = 1;

        const float pokeBallRecvOpenDelay = 1F;

        const byte recvPmonBounceCount = 3;

        const float endDelayTime = 0.5F;

        const string receiveMessagePrefix = "";
        const string receiveMessageSuffix = " was received!";

        #endregion

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public IEnumerator RunAnimation(PokemonInstance sendPmon,
            PokemonInstance recvPmon)
        {

            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);

            PokeBall sendPokeBall = PokeBall.GetPokeBallById(sendPmon.pokeBallId);
            PokeBall recvPokeBall = PokeBall.GetPokeBallById(recvPmon.pokeBallId);

            Vector3 pokemonInitialScale = PokemonTransform.localScale;

            //Initialise visibilities
            pokemonImage.enabled = true;
            pokeBallImage.enabled = false;

            //Initialise sprites
            pokemonImage.sprite = sendPmon.LoadSprite(PokemonSpecies.SpriteType.Front1);
            pokeBallImage.sprite = sendPokeBall.GetSprite(PokeBall.SpriteType.Neutral);

            //Initialise positions
            PokemonTransform.position = mainRootTransform.position;
            PokeBallTransform.position = mainRootTransform.position;

            //Goodbye message
            textBoxController.RevealText(goodbyeMessagePrefix + sendPmon.GetDisplayName() + goodbyeMessageSuffix);
            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            //Send pokemon cry
            SoundFXController.PlayPokemonCry(sendPmon.speciesId);

            //Send pokemon bouncing
            for (byte i = 0; i < sendPmonBonuceCount; i++)
                yield return StartCoroutine(Animation_Bounce(PokemonTransform));

            //Delay
            yield return new WaitForSeconds(jumpScaleDelay);

            //Open and show poke ball
            pokeBallImage.enabled = true;
            pokeBallImage.sprite = sendPokeBall.GetSprite(PokeBall.SpriteType.Open);

            //Shrink and hide pokemon
            yield return StartCoroutine(GradualEffect(
                t =>
                {
                    PokemonTransform.localScale = pokemonInitialScale * (1 - t);
                },
                shrinkGrowTime));
            pokemonImage.enabled = false;

            //Close poke ball
            pokeBallImage.sprite = sendPokeBall.GetSprite(PokeBall.SpriteType.Neutral);

            //Delay
            yield return new WaitForSeconds(pokeBallCloseSendDelay);

            //Send away poke ball
            yield return StartCoroutine(Animation_GradualLinearFunctionMovement(PokeBallTransform,
                pokeBallOffScreenTransform.position,
                t => Vector2.up * pokeBallFactorDisplacementCurve_y.Evaluate(t),
                t => Vector2.right * pokeBallAddedDisplacementCurve_x.Evaluate(t) * addedDisplacementFactor,
                pokeBallSendReturnTime));

            //Wait
            yield return new WaitForSeconds(pokeBallReturnDelay);

            //Switch pokemon and poke ball sprites
            pokemonImage.sprite = recvPmon.LoadSprite(PokemonSpecies.SpriteType.Front1);
            pokeBallImage.sprite = recvPokeBall.GetSprite(PokeBall.SpriteType.Neutral);

            //Return poke ball
            yield return StartCoroutine(Animation_GradualLinearFunctionMovement(PokeBallTransform,
                mainRootTransform.position,
                t => Vector2.up * pokeBallFactorDisplacementCurve_y.Evaluate(t),
                t => Vector2.right * pokeBallAddedDisplacementCurve_x.Evaluate(t) * addedDisplacementFactor,
                pokeBallSendReturnTime));

            //Delay
            yield return new WaitForSeconds(pokeBallRecvOpenDelay);

            //Open poke ball
            pokeBallImage.sprite = recvPokeBall.GetSprite(PokeBall.SpriteType.Open);

            //Show and grow pokemon
            pokemonImage.enabled = true;
            yield return StartCoroutine(GradualEffect(
                t =>
                {
                    PokemonTransform.localScale = pokemonInitialScale * t;
                },
                shrinkGrowTime));

            //Delay
            yield return new WaitForSeconds(jumpScaleDelay);

            //Recv pokemon cry
            SoundFXController.PlayPokemonCry(recvPmon.speciesId);

            //Receive pokemon bouncing
            for (byte i = 0; i < recvPmonBounceCount; i++)
                yield return StartCoroutine(Animation_Bounce(PokemonTransform));

            //Delay
            yield return new WaitForSeconds(endDelayTime);

            //Receiving message
            textBoxController.RevealText(receiveMessagePrefix + recvPmon.GetDisplayName() + receiveMessageSuffix);
            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

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
        /// Gradually moves a transform from its starting position to its final position using the displacement function.
        /// The displacement function returning 0,0 means the transform should be in the initial position.
        /// The displacement function returning 1,1 means the transform should be in the final position.
        /// </summary>
        private IEnumerator Animation_GradualLinearFunctionMovement(Transform transform,
            Vector2 finalPosition,
            Func<float, Vector2> factorDisplacementFunction,
            Func<float, Vector2> addedDisplacementFunction,
            float timeToTake)
        {

            Vector2 initialPosition = transform.position;

            yield return StartCoroutine(GradualEffect(
                (t) =>
                {
                    Vector2 fac = factorDisplacementFunction(t);
                    Vector2 added = addedDisplacementFunction(t);
                    transform.position = ((finalPosition - initialPosition) * fac) + initialPosition + added;
                },
                timeToTake));

            transform.position = finalPosition;

        }

        private IEnumerator Animation_Translate(Transform transform,
            Vector3 finalPosition,
            float timeToTake)
        {

            Vector3 initialPosition = transform.position;

            yield return StartCoroutine(Animation_GradualLinearFunctionMovement(transform,
                finalPosition,
                t => Vector2.Lerp(Vector2.zero, Vector2.one, t),
                _ => Vector2.zero,
                timeToTake));

            transform.position = finalPosition;

        }

        private IEnumerator Animation_Bounce(Transform transform,
            float height = bounceHeight,
            float timeToTake = bounceSingleTime)
        {

            Vector3 startPosition = transform.position;
            Vector3 heightPosition = startPosition + Vector3.up * height;

            yield return StartCoroutine(Animation_Translate(transform, heightPosition, timeToTake / 2));
            yield return StartCoroutine(Animation_Translate(transform, startPosition, timeToTake / 2));

        }

    }
}