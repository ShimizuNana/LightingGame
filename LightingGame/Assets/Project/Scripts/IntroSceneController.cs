using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text introText;
    [SerializeField] private TMP_Text continueText;

    [Header("Content")]
    [TextArea(2, 6)]
    [SerializeField] private string[] storyPages;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1f;

    [Header("Scene")]
    [SerializeField] private string nextSceneName = "MainLevel";

    private bool canContinue = false;

    private void Start()
    {
        if (continueText != null)
        {
            continueText.gameObject.SetActive(false);
        }

        StartCoroutine(PlayIntroSequence());
    }

    private void Update()
    {
        if (!canContinue) return;

        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        if (introText == null) yield break;

        Color textColor = introText.color;

        for (int i = 0; i < storyPages.Length; i++)
        {
            introText.text = storyPages[i];

            // 先设为透明
            textColor.a = 0f;
            introText.color = textColor;

            // 淡入
            yield return StartCoroutine(FadeText(introText, 0f, 1f, fadeInDuration));

            // 停留
            yield return new WaitForSeconds(showDuration);

            // 淡出
            yield return StartCoroutine(FadeText(introText, 1f, 0f, fadeOutDuration));
        }

        // 最后一页结束后，显示继续提示
        if (continueText != null)
        {
            continueText.gameObject.SetActive(true);
            yield return StartCoroutine(FadeText(continueText, 0f, 1f, 0.8f));
        }

        canContinue = true;
    }

    private IEnumerator FadeText(TMP_Text text, float startAlpha, float endAlpha, float duration)
    {
        if (text == null) yield break;

        float elapsed = 0f;
        Color color = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            text.color = color;

            yield return null;
        }

        color.a = endAlpha;
        text.color = color;
    }
}