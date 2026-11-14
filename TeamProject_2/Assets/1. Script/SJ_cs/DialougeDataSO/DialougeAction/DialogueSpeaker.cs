using UnityEngine;
using TMPro;

public class DialogueSpeaker : MonoBehaviour
{
    [Header("대사 출력 텍스트")]
    public TMP_Text dialogueText;

    private void Awake()
    {
        ClearText();
    }

    public void ClearText()
    {
        if (dialogueText != null)
            dialogueText.text = "";
    }
}
