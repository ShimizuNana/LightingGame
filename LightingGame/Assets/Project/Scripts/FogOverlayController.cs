using System.Collections;
using UnityEngine;

public class FogOverlayController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fogSpriteRenderer;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        if (fogSpriteRenderer == null)
        {
            fogSpriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public IEnumerator FadeOutFog()
    {
        if (fogSpriteRenderer == null) yield break;

        Color startColor = fogSpriteRenderer.color;
        float startAlpha = startColor.a;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            Color c = fogSpriteRenderer.color;
            c.a = Mathf.Lerp(startAlpha, 0f, t);
            fogSpriteRenderer.color = c;

            yield return null;
        }

        Color finalColor = fogSpriteRenderer.color;
        finalColor.a = 0f;
        fogSpriteRenderer.color = finalColor;

        fogSpriteRenderer.gameObject.SetActive(false);
    }
}