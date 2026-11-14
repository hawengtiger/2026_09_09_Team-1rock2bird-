
using UnityEngine;
using UnityEngine.Events;

public class DialogueTrigger : MonoBehaviour
{
    [Header("대사 데이터")]
    public DialogueDataSO dialogueData;

    [Header("대화 상대 NPC")]
    public DialogueSpeaker npcSpeaker;

    [Header("대사 매니저")]
    public TextTyping textTyping;

    [Header("한 번만 실행")]
    public bool triggerOnce = true;

    [Header("대화가 끝나면 실행")]
    public UnityEvent onDialogueFinished;

    private bool hasTriggered = false;
    private bool isWaitingForFinish = false;

    private void Start()
    {
        if (textTyping == null)
        {
            textTyping = FindAnyObjectByType<TextTyping>();
        }
    }

    private void OnEnable()
    {
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished += HandleDialogueFinished;
        }
    }

    private void OnDisable()
    {
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        StartConversation();
    }

    public void StartConversation()
    {
        if (triggerOnce && hasTriggered)
            return;

        if (textTyping == null || dialogueData == null)
            return;

        if (textTyping.IsPlaying)
            return;

        if (!textTyping.SetSpeakers(npcSpeaker))
            return;

        // 이전 이벤트 연결 해제
        textTyping.OnDialogueFinished -= HandleDialogueFinished;

        // 현재 대화의 종료 이벤트 등록
        textTyping.OnDialogueFinished += HandleDialogueFinished;

        isWaitingForFinish = true;

        if (textTyping.StartDialogue(dialogueData))
        {
            if (triggerOnce)
                hasTriggered = true;
        }
        else
        {
            isWaitingForFinish = false;
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }
    }

    private void HandleDialogueFinished()
    {
        if (!isWaitingForFinish)
            return;

        isWaitingForFinish = false;

        // 이벤트 중복 실행 방지
        textTyping.OnDialogueFinished -= HandleDialogueFinished;

        Debug.Log($"{gameObject.name} 대화 종료!");

        // Inspector에 등록된 함수 실행
        onDialogueFinished?.Invoke();
    }
}
