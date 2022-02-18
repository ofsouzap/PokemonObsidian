using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pokemon;
using Audio;

namespace EvolutionScene
{
    public class EvolutionSceneController : MonoBehaviour
    {

        public const float initialPauseTime = 1F;
        public const float animationTime = 5F;

        public const byte bounceCount = 2;
        public const float bounceHeight = 1;
        public const float bounceSingleTime = 0.5F;

        public const float bounceToShrinkDelayTime = 1;

        public const float shrinkTime = 1.5F;
        public const float unshrinkTime = 1.5F;

        public const float endDelayTime = 1;

        public const float musicVolume = 0.2F;

        public PokemonSpriteController pokemonSpriteController;

        public delegate void OnComplete();
        public event OnComplete EvolutionAnimationComplete;

        private bool evolutionHasBeenCancelled = false;

        #region Entrance Arguments

        public struct EntranceArguments
        {

            public PokemonInstance pokemon;
            public PokemonSpecies.Evolution evolution;

            public Sprite PokemonSprite => pokemon.LoadSprite(PokemonSpecies.SpriteType.Front1);

        }

        public static EntranceArguments entranceArguments;

        #endregion

        private TextBoxController textBoxController;

        public static EvolutionSceneController GetEvolutionSceneController(Scene scene)
        {

            EvolutionSceneController[] controllers = FindObjectsOfType<EvolutionSceneController>()
                .Where(x => x.gameObject.scene == scene)
                .ToArray();

            switch (controllers.Length)
            {

                case 0:
                    return null;

                case 1:
                    return controllers[0];

                default:
                    Debug.LogError("Multiple EvolutionSceneControllers found");
                    return controllers[0];

            }

        }

        private void Start()
        {

            SetUp();

            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);

        }

        private void Update()
        {
            
            if (Input.GetButtonDown("Cancel"))
            {
                evolutionHasBeenCancelled = true;
            }

        }

        private void SetUp()
        {
            
            pokemonSpriteController.SetSprite(entranceArguments.PokemonSprite);

        }

        public void StartAnimation()
        {
            StartCoroutine(AnimationCoroutine());
        }

        private IEnumerator AnimationCoroutine()
        {

            evolutionHasBeenCancelled = false;
            EvolutionAnimationComplete = null;

            float musicInitialVolume = MusicSourceController.singleton.GetVolume();

            MusicSourceController.singleton.SetVolume(musicVolume);

            Sprite startSprite = entranceArguments.PokemonSprite;

            PokemonSpecies endSpecies = PokemonSpecies.GetPokemonSpeciesById(entranceArguments.evolution.targetId);

            Sprite endSprite = endSpecies.LoadSprite(PokemonSpecies.SpriteType.Front1, entranceArguments.pokemon.gender);

            pokemonSpriteController.SetSprite(startSprite);

            yield return new WaitForSeconds(initialPauseTime);

            for (byte i = 0; i < bounceCount; i++)
                yield return StartCoroutine(Animation_Bounce(pokemonSpriteController.pokemonSpriteObject, bounceHeight, bounceSingleTime));

            yield return new WaitForSeconds(bounceToShrinkDelayTime);

            yield return StartCoroutine(GradualEffect((t) =>
            {
                pokemonSpriteController.SetScale(1 - t);
                pokemonSpriteController.SetWhiteness(t);
            },
            shrinkTime));

            if (!evolutionHasBeenCancelled)
                pokemonSpriteController.SetSprite(endSprite);

            yield return StartCoroutine(GradualEffect((t) =>
            {
                pokemonSpriteController.SetScale(t);
                pokemonSpriteController.SetWhiteness(1 - t);
            },
            unshrinkTime));

            if (!evolutionHasBeenCancelled)
            {

                //Evolution is allowed to be completed

                SoundFXController.singleton.PlaySound("evolution_end");

                textBoxController.RevealText(entranceArguments.pokemon.GetDisplayName()
                    + " evolved into a "
                    + endSpecies.name
                    + '!');

                //Once animation is completed, evolve the pokemon
                entranceArguments.pokemon.Evolve(entranceArguments.evolution);

            }
            else
            {

                //Evolution is cancelled

                textBoxController.RevealText("Oh, "
                    + entranceArguments.pokemon.GetDisplayName()
                    + " stopped evolving");

            }

            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            yield return new WaitForSeconds(endDelayTime);

            MusicSourceController.singleton.SetVolume(musicInitialVolume);

            EvolutionAnimationComplete?.Invoke();
            yield break;

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

        private IEnumerator Animation_Translate(GameObject go,
            Vector3 targetPosition,
            float timeToTake)
        {

            Vector3 startPosition = go.transform.position;

            yield return StartCoroutine(GradualEffect((t) =>
            {
                go.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            },
            timeToTake));

            go.transform.position = targetPosition;

        }

        private IEnumerator Animation_Bounce(GameObject go,
            float height,
            float timeToTake)
        {

            Vector3 startPosition = go.transform.position;
            Vector3 heightPosition = startPosition + Vector3.up * height;

            yield return StartCoroutine(Animation_Translate(go, heightPosition, timeToTake / 2));
            yield return StartCoroutine(Animation_Translate(go, startPosition, timeToTake / 2));

        }

    }
}
