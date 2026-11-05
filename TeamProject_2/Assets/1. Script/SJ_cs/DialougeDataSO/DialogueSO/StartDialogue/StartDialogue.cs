using UnityEngine;

public class StartDialogue : MonoBehaviour
{
    public TextTyping textTyping;
    public DialogueDataSO openingDialogue;

    public void Start()
    {
        textTyping = FindAnyObjectByType<TextTyping>();

        textTyping.StartDialogue(openingDialogue);
    }
}
