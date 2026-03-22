using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float lineDisplayTime = 2f;

    private bool isPlaying = false;

    private void Start()
    {
        HideDialogue();
    }

    public void PlayDialogue(string[] lines, Action onComplete = null)
    {
        if (!isPlaying)
        {
            StartCoroutine(PlayDialogueCoroutine(lines, onComplete));
        }
    }

    private IEnumerator PlayDialogueCoroutine(string[] lines, Action onComplete)
    {
        isPlaying = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        for (int i = 0; i < lines.Length; i++)
        {
            if (dialogueText != null)
            {
                dialogueText.text = lines[i];
            }

            // 每显示一行字幕时播放一次 NPC 说话音效
            if (AudioManager.Instance != null && AudioManager.Instance.npcTalkClip != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.npcTalkClip);
            }

            yield return new WaitForSeconds(lineDisplayTime);
        }

        HideDialogue();
        isPlaying = false;

        onComplete?.Invoke();
    }

    private void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}