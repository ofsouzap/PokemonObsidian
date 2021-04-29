using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlackFadeController : MonoBehaviour
{

    public const float fadeTime = 0.6f;

    [SerializeField]
    private Image imageBlack;

    private Color LerpA(Color color, float a, float b, float t)
    {
        float alpha = Mathf.Lerp(a, b, t);
        color.a = alpha;
        return color;
    }

    private IEnumerator FadeCoroutine(float timeToTake, float startA, float endA, bool destroyOnComplete)
    {

        imageBlack.gameObject.SetActive(true);
        imageBlack.enabled = true;

        Color baseColor = imageBlack.color;

        float startTime = Time.time;

        while (true)
        {

            float t = (Time.time - startTime) / timeToTake;

            if (t >= 1)
                break;
            else
                imageBlack.color = LerpA(baseColor, startA, endA, t);

            yield return new WaitForFixedUpdate();

        }

        imageBlack.color = LerpA(baseColor, startA, endA, 1);

        FadeCompleted?.Invoke();
        if (destroyOnComplete)
            Destroy(gameObject);

    }

    public delegate void OnComplete();
    public event OnComplete FadeCompleted;

    public void FadeIn() => StartCoroutine(FadeCoroutine(fadeTime, 1, 0, true));
    public void FadeOut() => StartCoroutine(FadeCoroutine(fadeTime, 0, 1, false));

}
