using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public enum TextState
{
    Idle,       // 대기
    Typing,     // 출력 중
    Deleting    // 삭제 중
}

public class TextTyping : MonoBehaviour
{
    // 대화 종료 이벤트
    public event Action OnDialogueFinished;

    [Header("모놀로그 텍스트")]
    public TextMeshPro textMesh;

    [Header("대사 데이터")]
    public DialogueDataSO dialogueData;

    [Header("스토리 UI 효과")]
    public DarkUIEffect darkUIEffect;

    [Header("플레이어 화자 (A)")]
    public DialogueSpeaker playerSpeaker;

    [Header("현재 대화 상대 NPC (B)")]
    public DialogueSpeaker npcSpeaker;

    [Header("타이핑 속도")]
    public float typingSpeed = 0.1f;

    [Header("삭제 속도")]
    public float deleteSpeed = 0.05f;

    public TextState currentState { get; private set; }
        = TextState.Idle;

    private Coroutine typingCoroutine;
    private int currentDialogueIndex = 0;
    private bool isFinished = true;

    // 현재 대사를 출력할 텍스트
    private TMP_Text currentText;

    // 다른 스크립트에서 진행 여부 확인
    public bool IsPlaying => !isFinished;

    private void Start()
    {
        ClearAllTexts();
    }

    private void Update()
    {
        if (isFinished)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.xKey.wasPressedThisFrame)
        {
            FinishDialogue();
            return;
        }

        // 좌클릭 또는 Z키 : 다음 대사
        bool mouseClick =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        bool zKey =
            Keyboard.current != null &&
            Keyboard.current.zKey.wasPressedThisFrame;

        if (mouseClick || zKey)
        {
            NextDialogue();
        }
    }

    // 현재 대화 상대 변경
    public bool SetSpeakers(DialogueSpeaker npc)
    {
        if (IsPlaying)
        {
            Debug.LogWarning(
                "대사 진행 중에는 NPC를 변경할 수 없습니다.");
            return false;
        }

        // 이전 NPC 텍스트 제거
        if (npcSpeaker != null)
        {
            npcSpeaker.ClearText();
        }

        npcSpeaker = npc;

        return true;
    }

    // 스토리 시작
    public bool StartDialogue(DialogueDataSO data)
    {
        if (IsPlaying)
        {
            Debug.LogWarning("이미 대사가 진행 중입니다.");
            return false;
        }

        if (data == null ||
            data.dialogues == null ||
            data.dialogues.Count == 0)
        {
            Debug.LogWarning("대사 데이터가 없습니다.");
            return false;
        }

        StopCurrentCoroutine();

        dialogueData = data;
        currentDialogueIndex = 0;
        isFinished = false;
        currentState = TextState.Idle;

        ClearAllTexts();

        if (darkUIEffect != null)
        {
            darkUIEffect.isStory = true;
        }

        PlayTyping();

        return !isFinished;
    }

    // 현재 대사의 출력 대상 설정
    private void SetCurrentText()
    {
        DialogueSetting setting =
            dialogueData.GetSetting(currentDialogueIndex);

        // 이전 출력 내용 초기화
        ClearAllTexts();

        if (setting.dialogueType == DialogueType.Monologue)
        {
            currentText = textMesh;
            return;
        }

        // 다이얼로그일 경우 A/B 선택
        DialogueSpeaker speaker =
            setting.bubbleType == BubbleType.A
            ? playerSpeaker
            : npcSpeaker;

        if (speaker == null)
        {
            currentText = null;
            Debug.LogWarning("DialogueSpeaker가 없습니다.");
            return;
        }

        currentText = speaker.dialogueText;
    }

    // 모든 텍스트 초기화
    private void ClearAllTexts()
    {
        if (textMesh != null)
            textMesh.text = "";

        if (playerSpeaker != null)
            playerSpeaker.ClearText();

        if (npcSpeaker != null)
            npcSpeaker.ClearText();
    }

    // 다음 대사 진행
    public void NextDialogue()
    {
        if (isFinished)
            return;

        // 출력 중이면 문장 즉시 완성
        if (currentState == TextState.Typing)
        {
            SkipText();
            return;
        }

        // 삭제 중이면 삭제 즉시 완료
        if (currentState == TextState.Deleting)
        {
            SkipText();
            return;
        }

        // 마지막 대사여도 삭제 실행
        PlayDelete();
    }

    // 텍스트 출력 시작
    public void PlayTyping()
    {
        if (isFinished || dialogueData == null)
            return;

        StopCurrentCoroutine();

        SetCurrentText();

        if (currentText == null)
        {
            Debug.LogError(
                "대사를 출력할 TextMeshPro가 할당되지 않았습니다.");

            FinishDialogue();
            return;
        }

        currentState = TextState.Typing;
        typingCoroutine = StartCoroutine(Typing());
    }

    // 텍스트 삭제 시작
    public void PlayDelete()
    {
        if (isFinished)
            return;

        StopCurrentCoroutine();

        currentState = TextState.Deleting;
        typingCoroutine = StartCoroutine(Delete());
    }

    // 한 글자씩 출력
    private IEnumerator Typing()
    {
        currentText.text = "";

        string dialogue =
            dialogueData.dialogues[currentDialogueIndex] ?? "";

        for (int i = 0 ; i < dialogue.Length ; i++)
        {
            currentText.text += dialogue[i];

            yield return new WaitForSeconds(typingSpeed);
        }

        currentState = TextState.Idle;
        typingCoroutine = null;
    }

    // 뒤에서부터 한 글자씩 삭제
    private IEnumerator Delete()
    {
        if (currentText != null)
        {
            while (currentText.text.Length > 0)
            {
                currentText.text =
                    currentText.text.Substring(
                        0,
                        currentText.text.Length - 1
                    );

                yield return new WaitForSeconds(deleteSpeed);
            }
        }

        typingCoroutine = null;
        currentState = TextState.Idle;

        AdvanceAfterDelete();
    }

    // 삭제 완료 후 다음 대사 또는 종료
    private void AdvanceAfterDelete()
    {
        if (currentDialogueIndex + 1 >=
            dialogueData.dialogues.Count)
        {
            FinishDialogue();
            return;
        }

        MoveNextDialogue();
    }

    // 다음 대사로 이동
    private void MoveNextDialogue()
    {
        currentDialogueIndex++;

        typingCoroutine = null;
        currentState = TextState.Idle;

        PlayTyping();
    }

    // 실행 중인 코루틴 중지
    private void StopCurrentCoroutine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    // 현재 상태에 따라 즉시 출력 또는 삭제
    public void SkipText()
    {
        if (isFinished || currentState == TextState.Idle)
            return;

        TextState previousState = currentState;

        StopCurrentCoroutine();

        switch (previousState)
        {
            case TextState.Typing:

                if (currentText != null)
                {
                    currentText.text =
                        dialogueData.dialogues[
                            currentDialogueIndex] ?? "";
                }

                currentState = TextState.Idle;
                break;

            case TextState.Deleting:

                if (currentText != null)
                    currentText.text = "";

                currentState = TextState.Idle;

                AdvanceAfterDelete();
                break;
        }
    }

    // 모든 대사 종료
    private void FinishDialogue()
    {
        if (isFinished)
            return;

        StopCurrentCoroutine();

        isFinished = true;
        currentState = TextState.Idle;
        typingCoroutine = null;
        currentText = null;

        ClearAllTexts();

        if (darkUIEffect != null)
        {
            darkUIEffect.isStory = false;
        }

        Debug.Log("모든 대사 종료");

        // 대화 종료를 Trigger에 알림
        OnDialogueFinished?.Invoke();
    }
}
