using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeatherCanvasScrollingViewController : MonoBehaviour
{

    public Vector2 scrollSpeed = new Vector2(1, 1);

    private Image Image => GetComponent<Image>();
    private Texture Texture => Image.mainTexture;

    private void OnEnable()
    {

        TryStartScroll();

    }

    private Coroutine scrollCoroutine = null;

    private void TryStartScroll()
    {

        if (scrollCoroutine != null)
            StopCoroutine(scrollCoroutine);

        scrollCoroutine = StartCoroutine(ScrollCoroutine());

    }

    private IEnumerator ScrollCoroutine()
    {

        while (true)
        {
            
            float x = -Mathf.Repeat(Time.time * scrollSpeed.x, Texture.width);
            float y = -Mathf.Repeat(Time.time * scrollSpeed.y, Texture.height);

            Image.material.mainTextureOffset = new Vector2(x, y);

            yield return new WaitForEndOfFrame();

        }

    }

}