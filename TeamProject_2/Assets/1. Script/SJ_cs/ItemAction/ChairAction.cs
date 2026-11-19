
using UnityEngine;
using UnityEngine.Events;

public class ChairAction : ItemAction
{
    [Header("대화 매니저")]
    [SerializeField] private TextTyping textTyping;

    [Header("실행할 다이얼로그")]
    public DialogueDataSO dialogueData;

    [Header("대화 상대 NPC")]
    public DialogueSpeaker npcSpeaker;

    [Header("다이얼로그 종료 후 실행할 액션")]
    public UnityEvent onDialogueFinished;

    [Header("한 번만 실행")]
    public bool executeOnce = true;

    private bool hasExecuted = false;
    private bool isWaitingForFinish = false;

    private void Awake()
    {
        if (textTyping == null)
        {
            textTyping = FindAnyObjectByType<TextTyping>();
        }
    }

    // 의자 F키 상호작용 시 실행
    public override void Execute(PickupItem item)
    {
        if (executeOnce && hasExecuted)
            return;

        if (isWaitingForFinish)
            return;

        if (textTyping == null || dialogueData == null)
            return;

        // 다른 대화가 진행 중이면 실행 방지
        if (textTyping.IsPlaying)
            return;

        // 대화 상대 NPC 설정
        if (!textTyping.SetSpeakers(npcSpeaker))
            return;

        // 다이얼로그 종료 이벤트 등록
        textTyping.OnDialogueFinished -= HandleDialogueFinished;
        textTyping.OnDialogueFinished += HandleDialogueFinished;

        isWaitingForFinish = true;

        // 다이얼로그 시작
        if (textTyping.StartDialogue(dialogueData))
        {
            if (executeOnce)
                hasExecuted = true;
        }
        else
        {
            isWaitingForFinish = false;

            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }
    }

    // 다이얼로그 종료 후 실행
    private void HandleDialogueFinished()
    {
        if (!isWaitingForFinish)
            return;

        isWaitingForFinish = false;

        // 중복 이벤트 호출 방지
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }

        Debug.Log("의자 다이얼로그 종료!");

        // Inspector에서 등록한 함수 실행
        onDialogueFinished?.Invoke();
    }

    private void OnDisable()
    {
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }

        isWaitingForFinish = false;
    }

    private void OnDestroy()
    {
        if (textTyping != null)
        {
            textTyping.OnDialogueFinished -= HandleDialogueFinished;
        }
    }
}
