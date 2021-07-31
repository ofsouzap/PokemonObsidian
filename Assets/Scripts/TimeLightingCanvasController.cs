//#define DEBUG_USE_MINUTE_FACTOR //Whether to use how far through a minute it is for the canvas color calculation instead of how far through a day it is

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class TimeLightingCanvasController : MonoBehaviour
{

    #region Singleton

    public static TimeLightingCanvasController singleton;

    private void Awake()
    {

        TrySetSingleton();

    }

    private void TrySetSingleton()
    {

        if (singleton == null)
        {
            singleton = this;
        }
        else
        {
            Debug.LogError("Multiple time lighting canvas controllers present. Destroying self");
            Destroy(gameObject);
        }

    }

    #endregion

    [Tooltip("How long to wait between refreshing the canvas' color in seconds")]
    public float colorRefreshDelay = 5;

    [Tooltip("The color gradient for the canvas. t=0 and t=1 represent midnight at the start and end of days respectively")]
    public Gradient colorGradient;

    public Image colorImage;

    private void Start()
    {

        StartCoroutine(ColorRefreshCoroutine());

    }

    private IEnumerator ColorRefreshCoroutine()
    {

        while (true)
        {

            RefreshColor();

            yield return new WaitForSeconds(colorRefreshDelay);

        }

    }

    private void RefreshColor()
    {

        float currentTimeFactor = GetCurrentTimeFactor();
        colorImage.color = colorGradient.Evaluate(currentTimeFactor);

    }

    private float GetCurrentTimeFactor()
    {

#if DEBUG_USE_MINUTE_FACTOR

        const float minMinuteSeconds = 0;
        const float maxMinuteSeconds = 60;

        return Mathf.InverseLerp(minMinuteSeconds, maxMinuteSeconds, DateTime.Now.Second + ((float)DateTime.Now.Millisecond / 1000));

#else

        const float minDaySeconds = 0;
        const float maxDaySeconds = 86400;

        return Mathf.InverseLerp(minDaySeconds, maxDaySeconds, DateTimeToDaySeconds(DateTime.Now));

#endif

    }

    private float DateTimeToDaySeconds(DateTime dt)
    {

        const float minuteSeconds = 60;
        const float hourSeconds = 3600;

        return dt.Second + (dt.Minute * minuteSeconds) + (dt.Hour * hourSeconds);

    }

}
