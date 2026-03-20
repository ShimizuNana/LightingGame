using System.Collections;
using UnityEngine;

public class MechanismController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite beforeSprite;
    [SerializeField] private Sprite changingSprite;
    [SerializeField] private Sprite afterSprite;

    [Header("Timing")]
    [SerializeField] private float changingDuration = 1.0f;

    private bool hasActivated = false;
    private bool isPlaying = false;

    private void Start()
    {
        if (spriteRenderer != null && beforeSprite != null)
        {
            spriteRenderer.sprite = beforeSprite;
        }
    }

    public bool HasActivated()
    {
        return hasActivated;
    }

    public IEnumerator PlayActivationSequence()
    {
        if (isPlaying || hasActivated)
            yield break;

        isPlaying = true;

        if (spriteRenderer != null && changingSprite != null)
        {
            spriteRenderer.sprite = changingSprite;
        }

        yield return new WaitForSeconds(changingDuration);

        if (spriteRenderer != null && afterSprite != null)
        {
            spriteRenderer.sprite = afterSprite;
        }

        hasActivated = true;
        isPlaying = false;

        Debug.Log("机关变化完成");
    }
}