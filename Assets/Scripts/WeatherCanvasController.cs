using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WeatherCanvasController : MonoBehaviour
{

    [SerializeField]
    private GameObject harshSunRoot;
    [SerializeField]
    private GameObject rainRoot;
    [SerializeField]
    private GameObject sandstormRoot;
    [SerializeField]
    private GameObject hailRoot;
    [SerializeField]
    private GameObject fogRoot;

    private const float defaultShowcaseDuration = 1.5F;
    private const float fadeDuration = 0.2F;

    private bool initialWeatherSet = false;

    public static WeatherCanvasController GetWeatherCanvasController(Scene scene)
    {

        WeatherCanvasController[] weatherCanvasControllerCandidates = FindObjectsOfType<WeatherCanvasController>()
            .Where(x => x.gameObject.scene == scene)
            .ToArray();

        if (weatherCanvasControllerCandidates.Length == 0)
        {
            Debug.LogError("No valid WeatherCanvasController found");
            return null;
        }
        else
            return weatherCanvasControllerCandidates[0];

    }

    private void Start()
    {

        if (!initialWeatherSet)
            HideAll();

        if (harshSunRoot.GetComponent<Image>() == null)
            Debug.LogError("No Image component on harshSunRoot");
        if (rainRoot.GetComponent<Image>() == null)
            Debug.LogError("No Image component on rainRoot");
        if (sandstormRoot.GetComponent<Image>() == null)
            Debug.LogError("No Image component on sandstormRoot");
        if (hailRoot.GetComponent<Image>() == null)
            Debug.LogError("No Image component on hailRoot");
        if (fogRoot.GetComponent<Image>() == null)
            Debug.LogError("No Image component on fogRoot");

    }

    public void SetWeather(Weather weather)
    {

        TryStopWeatherShowcaseCoroutine();

        SetWeatherUnchecked(weather);

    }

    private void SetWeatherUnchecked(Weather weather)
    {

        initialWeatherSet = true;

        HideAll();

        if (weather != null)
        {

            switch (weather.id)
            {

                case 0: //Clear sky (no weather)
                    break;

                case 1:
                    harshSunRoot.SetActive(true);
                    break;

                case 2:
                    rainRoot.SetActive(true);
                    break;

                case 3:
                    sandstormRoot.SetActive(true);
                    break;

                case 4:
                    hailRoot.SetActive(true);
                    break;

                case 5:
                    fogRoot.SetActive(true);
                    break;

                default:
                    Debug.LogError("Unknown weather id - " + weather.id);
                    break;

            }

        }

    }

    private Coroutine weatherShowcaseCoroutine = null;

    private void TryStopWeatherShowcaseCoroutine()
    {
        if (weatherShowcaseCoroutine != null)
        {
            StopCoroutine(weatherShowcaseCoroutine);
            weatherShowcaseCoroutine = null;
        }
    }

    public IEnumerator ShowcaseWeather(Weather weather,
        float duration= defaultShowcaseDuration)
    {

        TryStopFadeCoroutine();

        yield return StartCoroutine(FadeRootsAlphaCoroutine(1, 0));

        SetWeatherUnchecked(weather);

        yield return StartCoroutine(FadeRootsAlphaCoroutine(0, 1));

        yield return new WaitForSeconds(duration);

        yield return StartCoroutine(FadeRootsAlphaCoroutine(1, 0));

        SetWeatherUnchecked(null);

        yield return StartCoroutine(FadeRootsAlphaCoroutine(0, 1));

    }

    private void HideAll()
    {

        harshSunRoot.SetActive(false);
        rainRoot.SetActive(false);
        sandstormRoot.SetActive(false);
        hailRoot.SetActive(false);
        fogRoot.SetActive(false);

    }

    #region Fading In/Out

    private Coroutine fadeCoroutine = null;

    private void TryStopFadeCoroutine()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    private IEnumerator FadeRootsAlphaCoroutine(float start,
        float end,
        float duration = fadeDuration)
    {

        IEnumerable<Image> images = new GameObject[]
        {
            harshSunRoot,
            rainRoot,
            sandstormRoot,
            hailRoot,
            fogRoot
        }.Select(x => x.GetComponent<Image>());

        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {

            float t = (Time.time - startTime) / duration;

            foreach (Image image in images)
                image.color = new Color(
                    image.color.r,
                    image.color.g,
                    image.color.b,
                    Mathf.Lerp(start, end, t));

            yield return new WaitForEndOfFrame();

        }

    }

    #endregion

}